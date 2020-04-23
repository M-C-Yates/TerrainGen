using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[SelectionBase]

// improve world generation to look better
public class MapGenerator : MonoBehaviour
{
  public enum FalloffShape { square, circle };
  public enum DrawMode
  {
    noiseMap,
    mesh,
    falloffMap
  };
  public DrawMode drawMode;
  public FalloffShape falloffShape;
  public bool useFalloff = false;

  public TerrainData terrainData;
  public NoiseData noiseData;
  public TextureData textureData;

  public Material terrainMaterial;

  public int seed = 1;


  public const int mapChunkSize = 241; // TODO use 121 if more performance is needed
  [Range(0, 6)] public int editorPreviewLOD; // 241 - 1 is divisible by 2,4,6,8,10,12 for Level of Detail

  public bool autoUpdate = true;

  // public MapGrid mapGrid;
  public MapCell[,] mapGrid;

  public MapData mapData;

  float[,] falloffMap;

  Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
  Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

  void Awake()
  {
    falloffMap = FalloffGenerator.GenerateSquareFalloffMap(mapChunkSize);
  }

  void OnTextureValuesUpdated()
  {
    textureData.ApplyToMaterial(terrainMaterial);
  }

  void OnValuesUpdated()
  {
    if (!Application.isPlaying)
    {
      DrawMapInEditor();
    }
  }

  public void DrawMapInEditor()
  {
    mapData = GenerateMapData(Vector2.zero);
    MapDisplay display = FindObjectOfType<MapDisplay>();
    if (drawMode == DrawMode.noiseMap)
    {
      display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
    }

    else if (drawMode == DrawMode.mesh)
    {
      display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.meshHeightMultiplier, terrainData.meshHeightCurve, editorPreviewLOD));
    }
    else if (drawMode == DrawMode.falloffMap)
    {
      display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateCircularFalloffMap(mapChunkSize)));
    }
  }

  public void RequestMapData(Vector2 center, Action<MapData> callback)
  {
    ThreadStart threadStart = delegate
    {
      MapDataThread(center, callback);
    };

    new Thread(threadStart).Start();
  }

  void MapDataThread(Vector2 center, Action<MapData> callback)
  {
    mapData = GenerateMapData(center);

    lock (mapDataThreadInfoQueue)
    {
      mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
    }
  }


  public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback)
  {
    ThreadStart threadStart = delegate
    {
      MeshDataThread(mapData, lod, callback);
    };

    new Thread(threadStart).Start();
  }

  void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
  {
    MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.meshHeightMultiplier, terrainData.meshHeightCurve, lod);

    lock (mapDataThreadInfoQueue)
    {
      meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
    }
  }

  void Update()
  {
    if (mapDataThreadInfoQueue.Count > 0)
    {
      for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
      {
        MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
        threadInfo.callback(threadInfo.parameter);
      }
    }
    if (meshDataThreadInfoQueue.Count > 0)
    {
      for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
      {
        MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
        threadInfo.callback(threadInfo.parameter);
      }
    }
  }

  MapData GenerateMapData(Vector2 center)
  {
    mapGrid = new MapCell[mapChunkSize, mapChunkSize];
    if (falloffShape == FalloffShape.square)
    {
      falloffMap = FalloffGenerator.GenerateSquareFalloffMap(mapChunkSize);
    }
    else if (falloffShape == FalloffShape.circle)
    {
      falloffMap = FalloffGenerator.GenerateCircularFalloffMap(mapChunkSize);
    }

    float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseData.noiseScale, noiseData.octaves, noiseData.persistance, noiseData.lacunarity, center + noiseData.offset, noiseData.normalizeMode);


    for (int y = 0; y < mapChunkSize; y++)
    {
      for (int x = 0; x < mapChunkSize; x++)
      {
        if (useFalloff)
        {
          noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffMap[x, y]);
        }
        float currentHeight = noiseMap[x, y];
        mapGrid[x, y].height = currentHeight;
      }
    }

    return new MapData(noiseMap);
  }

  void OnValidate()
  {
    if (terrainData != null)
    {
      terrainData.OnValuesUpdated -= OnValuesUpdated;
      terrainData.OnValuesUpdated += OnValuesUpdated;
    }
    if (noiseData != null)
    {
      noiseData.OnValuesUpdated -= OnValuesUpdated;
      noiseData.OnValuesUpdated += OnValuesUpdated;
    }
    if (textureData != null)
    {
      textureData.OnValuesUpdated -= OnTextureValuesUpdated;
      textureData.OnValuesUpdated += OnTextureValuesUpdated;
    }
  }

  struct MapThreadInfo<T>
  {
    public readonly Action<T> callback;
    public readonly T parameter;

    public MapThreadInfo(Action<T> callback, T parameter)
    {
      this.callback = callback;
      this.parameter = parameter;
    }
  }
}


public struct MapData
{
  public float[,] heightMap;
  // public Color32[] colorMap;

  public MapData(float[,] heightMap)
  {
    this.heightMap = heightMap;
  }
}

[System.Serializable]
public struct MapCell
{
  public int xPos;
  public int ZPos;
  public float height;
  public string biome;
}