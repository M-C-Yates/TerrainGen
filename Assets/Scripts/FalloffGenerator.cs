using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FalloffGenerator
{
  public static float[,] GenerateSquareFalloffMap(int size)
  {
    float[,] map = new float[size, size];
    for (int i = 0; i < size; i++)
    {
      for (int j = 0; j < size; j++)
      {
        float x = i / (float)size * 2 - 1;
        float y = j / (float)size * 2 - 1;

        float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
        map[i, j] = EvaluateSquare(value);
      }
    }
    return map;
  }

  public static float[,] GenerateCircularFalloffMap(int size)
  {


    float[,] map = new float[size, size];
    Vector2 center = new Vector2(size / 2f, size / 2f);

    for (int i = 0; i < size; i++)
    {
      for (int j = 0; j < size; j++)
      {
        float DistanceFromCenter = Vector2.Distance(center, new Vector2(i, j));
        float currentAlpha = 1;

        if ((1 - (DistanceFromCenter / size)) >= 0)
        {
          currentAlpha = (1 - (DistanceFromCenter / size));
        }
        else
        {
          currentAlpha = 0;
        }

        map[i, j] = EvaluateCircle(currentAlpha);
      }
    }

    return map;
  }

  static float EvaluateSquare(float value)
  {
    float a = 3;
    float b = 2.2f;

    return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
  }

  static float EvaluateCircle(float value)
  {
    float a = 20;
    float b = 1.5f;

    return Mathf.Pow(value, -a) / (Mathf.Pow(value, -a) + Mathf.Pow(b - b * value, -a));
  }

}