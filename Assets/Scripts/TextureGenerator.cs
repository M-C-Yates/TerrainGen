﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureGenerator
{
  public static Texture2D TextureFromColorMap(Color32[] colorMap, int width, int height)
  {
    Texture2D texture = new Texture2D(width, height)
    {
      filterMode = FilterMode.Point,
      wrapMode = TextureWrapMode.Clamp
    };
    texture.SetPixels32(colorMap);
    texture.Apply();
    return texture;
  }

  public static Texture2D TextureFromHeightMap(float[,] heightMap)
  {
    int width = heightMap.GetLength(0);
    int height = heightMap.GetLength(1);

    Color32[] colorMap = new Color32[width * height];

    for (int y = 0; y < height; y++)
    {
      for (int x = 0; x < width; x++)
      {
        colorMap[(y * width) + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);
      }
    }

    return TextureFromColorMap(colorMap, width, height);
  }
}
