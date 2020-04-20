using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
  public const float MaxViewDist = 450;
  public Transform viewer;
  public static Vector2 viewerPosition;
  static MapGenerator mapGenerator;
  public Material mapMaterial;

  int chunkSize;
  int chunksVisibileInViewDistance;

  Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
  List<TerrainChunk> terrainChunkVisibleLastUpdate = new List<TerrainChunk>();

  void Start()
  {
    mapGenerator = FindObjectOfType<MapGenerator>();
    chunkSize = MapGenerator.mapChunkSize - 1;
    chunksVisibileInViewDistance = Mathf.RoundToInt(MaxViewDist / chunkSize);
  }

  void Update()
  {
    viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
    UpdateVisibleChunks();
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
          terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, transform, mapMaterial));
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

    public TerrainChunk(Vector2 coord, int size, Transform parent, Material material)
    {
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

      mapGenerator.RequestMapData(OnMapDataRecieved);
    }

    void OnMapDataRecieved(MapData mapData)
    {
      mapGenerator.RequestMeshData(mapData, OnMeshDataRecieved);
    }

    void OnMeshDataRecieved(MeshData meshData)
    {
      meshFilter.mesh = meshData.CreateMesh();
    }

    public void UpdateTerrainChunk()
    {
      float viewerDistFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
      bool visible = viewerDistFromNearestEdge <= MaxViewDist;
      SetVisible(visible);
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

}
