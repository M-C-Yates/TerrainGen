using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class TerrainData : UpdateableData
{
  public float meshHeightMultiplier = 2f;

  public AnimationCurve meshHeightCurve;
  public float uniformScale = 5f;

  void OnValidate()
  {
    if (meshHeightMultiplier <= 0f)
    {
      meshHeightMultiplier = 0.001f;
    }
  }

}
