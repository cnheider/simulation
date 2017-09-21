using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class ReplacementShaderEffect : MonoBehaviour {
  public Shader ReplacementShader;
  public Color OverDrawColor;
  public string _replace_rendertype = "";

  void OnValidate () {
    Shader.SetGlobalColor ("_OverDrawColor", OverDrawColor);
    Shader.SetGlobalColor ("_SegmentationColor", OverDrawColor);
  }

  void OnEnable () {
    if (ReplacementShader != null)
      GetComponent<Camera> ().SetReplacementShader (ReplacementShader, _replace_rendertype);
  }

  void OnDisable () {
    GetComponent<Camera> ().ResetReplacementShader ();
  }
}