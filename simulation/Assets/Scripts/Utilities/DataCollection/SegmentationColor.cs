using UnityEngine;
using System;

[Serializable]
public struct SegmentationColorByTag{
  public string tag;
  public Color color;
}

[Serializable]
public struct SegmentationColorByGameObject{
  public GameObject game_object;
  public Color color;
}
