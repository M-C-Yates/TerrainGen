using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[SelectionBase]

public class MapGenerator : MonoBehaviour
{
  [SerializeField] int mapWidth = 100;
  [SerializeField] int mapHeight = 100;
  [SerializeField] float noiseScale = 25f;

  [Range(0, 1)] [SerializeField] float persistance = 0.6f;
  [SerializeField] float lacunarity = 1.8f;
  [SerializeField] int octaves = 5;

  [SerializeField] Vector2 offset = new Vector2(2f, 3f);
  [SerializeField] int seed = 1;


  public bool autoUpdate = true;

  public void GenerateMap()
  {
    float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);
    MapDisplay display = FindObjectOfType<MapDisplay>();
    display.DrawNoiseMap(noiseMap);
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
