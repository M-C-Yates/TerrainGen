using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{
  public Renderer textureRenderer;

  public void DrawNoiseMap(float[,] noiseMap)
  {
    int width = noiseMap.GetLength(0);
    int height = noiseMap.GetLength(1);

    Texture2D texture = new Texture2D(width, height);
    Color32[] colorMap = new Color32[width * height];
    for (int y = 0; y < height; y++)
    {
      for (int x = 0; x < width; x++)
      {
        colorMap[(y * width) + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);
      }
    }
    texture.SetPixels32(colorMap);
    texture.Apply();
    textureRenderer.sharedMaterial.mainTexture = texture;
    textureRenderer.transform.localScale = new Vector3(width, 1, height);
  }
}
