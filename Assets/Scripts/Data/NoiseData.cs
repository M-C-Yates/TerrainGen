using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class NoiseData : UpdateableData
{
  public Noise.NormalizeMode normalizeMode;

  public float noiseScale = 25f;
  [Range(0, 1)] public float persistance = 0.6f;
  public float lacunarity = 1.8f;
  public int octaves = 5;

  public Vector2 offset = new Vector2(0f, 0f);

  protected override void OnValidate()
  {
    if (lacunarity <= 1)
    {
      lacunarity = 1.8f;
    }
    if (octaves < 0)
    {
      octaves = 0;
    }
    base.OnValidate();
  }
}
