using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureGenerator
{
  public static Texture2D TextureFromColorMap(Color32[] colorMap, int chunkSize)
  {
    Texture2D texture = new Texture2D(chunkSize, chunkSize)
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
    int chunkSize = heightMap.GetLength(0);

    Color32[] colorMap = new Color32[chunkSize * chunkSize];

    for (int y = 0; y < chunkSize; y++)
    {
      for (int x = 0; x < chunkSize; x++)
      {
        colorMap[(y * chunkSize) + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);
      }
    }

    return TextureFromColorMap(colorMap, chunkSize);
  }
}
