using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[SelectionBase]

// improve world generation to look better
public class MapGenerator : MonoBehaviour
{
  public enum DrawMode
  {
    noiseMap,
    colorMap,
    mesh
  }
  public DrawMode drawMode;
  public int mapWidth = 100;
  public int mapHeight = 100;
  public float noiseScale = 25f;

  [Range(0, 1)] public float persistance = 0.6f;
  public float lacunarity = 1.8f;
  public int octaves = 5;

  public Vector2 offset = new Vector2(0f, 0f);
  public int seed = 1;

  public MapGrid mapGrid;

  public TerrainType[] regions;
  public bool autoUpdate = true;

  public void GenerateMap()
  {
    mapGrid.xSize = mapWidth;
    mapGrid.zSize = mapHeight;
    mapGrid.grid = new MapCell[mapGrid.xSize, mapGrid.zSize];

    float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);
    print(regions[7].color);

    Color32[] colorMap = new Color32[mapWidth * mapHeight];

    for (int y = 0; y < mapHeight; y++)
    {
      for (int x = 0; x < mapWidth; x++)
      {
        float currentHeight = noiseMap[x, y];
        mapGrid.grid[x, y].height = currentHeight;
        SetBiomes(x, y);
        colorMap[y * mapWidth + x] = mapGrid.grid[x, y].biome.color;
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
    else if (drawMode == DrawMode.mesh)
    {
      display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap), TextureGenerator.TextureFromColorMap(colorMap, mapWidth, mapHeight));
    }
  }
  void SetBiomes(int x, int y)
  {
    MapCell currentCell = mapGrid.grid[x, y];
    if (currentCell.height <= 0.3)
    {
      mapGrid.grid[x, y].biome = regions[0];
      return;
    }
    else if (currentCell.height <= 0.43)
    {
      mapGrid.grid[x, y].biome = regions[1];
      return;
    }
    else if (currentCell.height <= 0.45)
    {
      mapGrid.grid[x, y].biome = regions[2];
      return;
    }
    else if (currentCell.height <= 0.55)
    {
      mapGrid.grid[x, y].biome = regions[3];
      return;
    }
    else if (currentCell.height <= 0.7)
    {
      mapGrid.grid[x, y].biome = regions[4];
      return;
    }
    else if (currentCell.height <= 0.8)
    {
      mapGrid.grid[x, y].biome = regions[5];
      return;
    }
    else if (currentCell.height <= 0.9)
    {
      mapGrid.grid[x, y].biome = regions[6];
      return;
    }
    else if (currentCell.height <= 1)
    {
      mapGrid.grid[x, y].biome = regions[7];
      return;
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
public struct MapGrid
{
  public int xSize;
  public int zSize;

  public MapCell[,] grid;

}

public struct MapCell
{
  public int xPos;
  public int ZPos;
  public float height;
  public TerrainType biome;
}