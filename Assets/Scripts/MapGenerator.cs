using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[SelectionBase]

public class MapGenerator : MonoBehaviour
{
  public enum DrawMode
  {
    noiseMap,
    colorMap
  }
  public DrawMode drawMode;
  public int mapWidth = 100;
  public int mapHeight = 100;
  public float noiseScale = 25f;
  public float frequency = 1f;

  [Range(0, 1)] public float persistance = 0.6f;
  public float lacunarity = 1.8f;
  public int octaves = 5;

  public Vector2 offset = new Vector2(0f, 0f);
  public int seed = 1;

  public TerrainType[] regions;


  public bool autoUpdate = true;

  public void GenerateMap()
  {
    float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, frequency, offset);
    Color32[] colorMap = new Color32[mapWidth * mapHeight];
    for (int y = 0; y < mapHeight; y++)
    {
      for (int x = 0; x < mapWidth; x++)
      {
        float currentHeight = noiseMap[x, y];
        for (int i = 0; i < regions.Length; i++)
        {
          if (currentHeight <= regions[i].height)
          {
            colorMap[y * mapWidth + x] = regions[i].color;
            break;
          }
        }
      }
    }

    MapDisplay display = FindObjectOfType<MapDisplay>();
    if (drawMode == DrawMode.noiseMap)
    {
      display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
    }
    else if (drawMode == DrawMode.colorMap)
    {
      display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, mapWidth, mapHeight));

    }
  }

  void OnValidate()
  {
    if (mapWidth < 1)
    {
      mapWidth = 1;
    }
    if (mapHeight < 1)
    {
      mapHeight = 1;
    }
    if (lacunarity <= 1)
    {
      lacunarity = 1.8f;
    }
    if (octaves < 0)
    {
      octaves = 0;
    }
    // if (amplitude < 1)
    // {
    //   amplitude = 1f;
    // }
    if (frequency <= 0)
    {
      frequency = 0.001f;
    }
  }
}

[System.Serializable]
public struct TerrainType
{
  public string biome;
  public float height;
  public Color32 color;
}