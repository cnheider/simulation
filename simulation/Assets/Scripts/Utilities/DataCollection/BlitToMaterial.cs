using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
public class BlitToMaterial : MonoBehaviour {

  public Material _material;

  void OnRenderImage (RenderTexture source, RenderTexture destination) {
    Graphics.Blit (source, destination, _material);
  }
}
