using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{
  public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, float scale, int octaves, float persistance, float lacunarity)
  {
    if (scale <= 0)
    {
      scale = 0.0001f;
    }
    float[,] noiseMap = new float[mapWidth, mapHeight];
    for (int y = 0; y < mapHeight; y++)
    {
      for (int x = 0; x < mapWidth; x++)
      {
        float amplitude = 1;
        float frequency = 1;
        float noiseHeight = 0;

        for (int o = 0; o < octaves; o++)
        {
          float sampleX = x / scale * frequency;
          float sampleY = y / scale * frequency;
          float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1; // sets the range of noise to be between -1 and 1
          noiseHeight += perlinValue * amplitude;

          amplitude *= persistance;
          frequency *= lacunarity;
        }
        noiseMap[x, y] = noiseHeight;
      }
    }
    return noiseMap;
  }
}
