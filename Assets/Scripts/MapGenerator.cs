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
    colorMap,
    mesh,
    falloffMap
  };
  public DrawMode drawMode;
  public FalloffShape falloffShape;

  public Noise.NormalizeMode normalizeMode;

  public const int mapChunkSize = 241; // TODO use 121 if more performance is needed
  [Range(0, 6)] public int editorPreviewLOD; // 241 - 1 is divisible by 2,4,6,8,10,12 for Level of Detail

  public float noiseScale = 25f;
  [Range(0, 1)] public float persistance = 0.6f;
  public float lacunarity = 1.8f;
  public int octaves = 5;

  public Vector2 offset = new Vector2(0f, 0f);
  public int seed = 1;
  public bool autoUpdate = true;
  public float meshHeightMultiplier = 2f;

  public AnimationCurve meshHeightCurve;

  // public MapGrid mapGrid;
  public MapCell[,] mapGrid;

  public TerrainType[] regions;
  public MapData mapData;

  float[,] falloffMap;
  public bool useFalloff = false;

  Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
  Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

  void Awake()
  {
    falloffMap = FalloffGenerator.GenerateSquareFalloffMap(mapChunkSize);
  }

  public void DrawMapInEditor()
  {
    mapData = GenerateMapData(Vector2.zero);
    MapDisplay display = FindObjectOfType<MapDisplay>();
    if (drawMode == DrawMode.noiseMap)
    {
      display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
    }
    else if (drawMode == DrawMode.colorMap)
    {
      display.DrawTexture(TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize));
    }
    else if (drawMode == DrawMode.mesh)
    {
      display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, editorPreviewLOD), TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize));
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
    MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, lod);

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

    float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, center + offset, normalizeMode);

    Color32[] colorMap = new Color32[mapChunkSize * mapChunkSize];

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
        SetBiomes(x, y, currentHeight);
        colorMap[y * mapChunkSize + x] = mapGrid[x, y].biome.color;
      }
    }

    return new MapData(noiseMap, colorMap);
  }
  void SetBiomes(int x, int y, float height)
  {
    if (height >= 0.93f)
    {
      mapGrid[x, y].biome = regions[7];

      // mapGrid[x, y].biome.color = new Color32(255,255,255,255);      
      return;
    }

    else if (height >= 0.86f)
    {
      mapGrid[x, y].biome = regions[6];
      return;
    }
    else if (height >= 0.7f)
    {
      mapGrid[x, y].biome = regions[5];
      return;
    }
    else if (height >= 0.6f)
    {
      mapGrid[x, y].biome = regions[4];
      return;
    }
    else if (height >= 0.45f)
    {
      mapGrid[x, y].biome = regions[3];
      return;
    }
    else if (height >= 0.43f)
    {
      mapGrid[x, y].biome = regions[2];
      return;
    }
    else if (height >= 0.3f)
    {
      mapGrid[x, y].biome = regions[1];
      return;
    }
    else if (height >= 0)
    {
      mapGrid[x, y].biome = regions[0];
      return;
    }
  }
  void OnValidate()
  {
    if (lacunarity <= 1)
    {
      lacunarity = 1.8f;
    }
    if (octaves < 0)
    {
      octaves = 0;
    }
    if (meshHeightMultiplier <= 0f)
    {
      meshHeightMultiplier = 0.001f;
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
  public Color32[] colorMap;

  public MapData(float[,] heightMap, Color32[] colorMap)
  {
    this.heightMap = heightMap;
    this.colorMap = colorMap;
  }
}
[System.Serializable]
public struct TerrainType // biome info
{
  public string name;
  public float height;
  public Color32 color;
}


[System.Serializable]
public struct MapCell
{
  public int xPos;
  public int ZPos;
  public float height;
  public TerrainType biome;
}