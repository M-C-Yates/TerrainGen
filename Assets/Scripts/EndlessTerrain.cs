using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
  const float viewerMoveThresholdForChunkUpdate = 25f;
  const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;
  public LODInfo[] detailLevels;
  public static float MaxViewDist;

  public Transform viewer;
  public static Vector2 viewerPosition;
  Vector2 viewerPositionOld;

  static MapGenerator mapGenerator;
  public Material mapMaterial;

  int chunkSize;
  int chunksVisibileInViewDistance;

  Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
  List<TerrainChunk> terrainChunkVisibleLastUpdate = new List<TerrainChunk>();

  void Start()
  {
    mapGenerator = FindObjectOfType<MapGenerator>();
    MaxViewDist = detailLevels[detailLevels.Length - 1].visibleDistThreshold;
    chunkSize = MapGenerator.mapChunkSize - 1;
    chunksVisibileInViewDistance = Mathf.RoundToInt(MaxViewDist / chunkSize);

    UpdateVisibleChunks();
  }

  void Update()
  {
    viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
    if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)
    {
      viewerPositionOld = viewerPosition;
      UpdateVisibleChunks();
    }
  }

  void UpdateVisibleChunks()
  {
    for (int i = 0; i < terrainChunkVisibleLastUpdate.Count; i++)
    {
      terrainChunkVisibleLastUpdate[i].SetVisible(false);
    }
    terrainChunkVisibleLastUpdate.Clear();

    int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
    int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);
    for (int yOffset = -chunksVisibileInViewDistance; yOffset <= chunksVisibileInViewDistance; yOffset++)
    {
      for (int xOffset = -chunksVisibileInViewDistance; xOffset <= chunksVisibileInViewDistance; xOffset++)
      {
        Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

        if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
        {
          terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
          if (terrainChunkDictionary[viewedChunkCoord].isVisible())
          {
            terrainChunkVisibleLastUpdate.Add(terrainChunkDictionary[viewedChunkCoord]);
          }
        }
        else
        {
          terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, transform, mapMaterial));
        }

      }
    }
  }

  public class TerrainChunk
  {

    GameObject meshObject;
    Vector2 position;
    Bounds bounds;

    MapData mapData;

    MeshRenderer meshRenderer;
    MeshFilter meshFilter;

    LODInfo[] detailLevels;
    LODMesh[] lodMeshes;
    bool mapDataRecieved;
    int previousLODIndex = -1;

    public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material material)
    {
      this.detailLevels = detailLevels;
      position = coord * size;
      bounds = new Bounds(position, Vector2.one * size);
      Vector3 positionV3 = new Vector3(position.x, 0, position.y);

      meshObject = new GameObject("TerrainChunk");
      meshRenderer = meshObject.AddComponent<MeshRenderer>();
      meshRenderer.material = material;
      meshFilter = meshObject.AddComponent<MeshFilter>();


      meshObject.transform.position = positionV3;
      meshObject.transform.parent = parent;
      SetVisible(false);

      lodMeshes = new LODMesh[detailLevels.Length];

      for (int i = 0; i < detailLevels.Length; i++)
      {
        lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
      }

      mapGenerator.RequestMapData(position, OnMapDataRecieved);
    }

    void OnMapDataRecieved(MapData mapData)
    {
      this.mapData = mapData;
      mapDataRecieved = true;

      Texture2D texture = TextureGenerator.TextureFromColorMap(mapData.colorMap, MapGenerator.mapChunkSize);
      meshRenderer.material.SetTexture("_BaseColorMap", texture);


      UpdateTerrainChunk();
    }

    public void UpdateTerrainChunk()
    {
      if (mapDataRecieved)
      {

        float viewerDistFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
        bool visible = viewerDistFromNearestEdge <= MaxViewDist;

        if (visible)
        {
          int lodIndex = 0;
          for (int i = 0; i < detailLevels.Length - 1; i++)
          {
            if (viewerDistFromNearestEdge > detailLevels[i].visibleDistThreshold)
            {
              lodIndex = i + 1;
            }
            else
            {
              break;
            }

            if (lodIndex != previousLODIndex)
            {
              LODMesh lodMesh = lodMeshes[lodIndex];
              if (lodMesh.hasMesh)
              {
                previousLODIndex = lodIndex;
                meshFilter.mesh = lodMesh.mesh;
              }
              else if (!lodMesh.hasRequestedMesh)
              {
                lodMesh.RequestMesh(mapData);
              }
            }
          }

        }

        SetVisible(visible);
      }

    }

    public void SetVisible(bool visible)
    {
      meshObject.SetActive(visible);
    }

    public bool isVisible()
    {
      return meshObject.activeSelf;
    }
  }

  class LODMesh
  {

    public Mesh mesh;
    public bool hasRequestedMesh;
    public bool hasMesh;
    int lod;
    System.Action updateCallback;

    public LODMesh(int lod, System.Action updateCallback)
    {
      this.lod = lod;
      this.updateCallback = updateCallback;
    }

    void OnMeshDataReceived(MeshData meshData)
    {
      mesh = meshData.CreateMesh();
      hasMesh = true;

      updateCallback();
    }

    public void RequestMesh(MapData mapData)
    {
      hasRequestedMesh = true;
      mapGenerator.RequestMeshData(mapData, lod, OnMeshDataReceived);
    }

  }

  [System.Serializable]
  public struct LODInfo
  {
    public int lod;
    public float visibleDistThreshold;
  }


}
