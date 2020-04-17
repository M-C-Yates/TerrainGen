using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[SelectionBase]

public class MapGenerator : MonoBehaviour
{
  [SerializeField] int mapWidth;
  [SerializeField] int mapHeight;
  [SerializeField] float noiseScale;

  public bool autoUpdate = false;

  public void GenerateMap()
  {
    float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, noiseScale);
    MapDisplay display = FindObjectOfType<MapDisplay>();
    display.DrawNoiseMap(noiseMap);
  }
}
