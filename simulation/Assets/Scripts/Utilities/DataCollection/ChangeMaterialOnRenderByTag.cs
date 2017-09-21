using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[ExecuteInEditMode]
public class ChangeMaterialOnRenderByTag : MonoBehaviour {

  [Serializable]
  public struct TagColor {
    public string tag;
    public Color color;
  }

  public bool _use_shared_materials = false;
  public TagColor[] _tag_colors_array;

  Dictionary<string, Color> _tag_colors;
  Color[] _original_colors;
  Renderer[] _all_renders;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
    Setup ();
	}

  void Setup(){
    _all_renders = FindObjectsOfType<Renderer>();

    _tag_colors = new Dictionary<string, Color> ();
    foreach (var tag_color in _tag_colors_array) {
      _tag_colors.Add (tag_color.tag, tag_color.color);
    }
  }

  void Change(){
    _original_colors = new Color[_all_renders.Length];
    for (int i = 0; i < _all_renders.Length; i++) {
      if(_tag_colors.ContainsKey(_all_renders[i].tag)){
        if (_use_shared_materials) {
          _original_colors [i] = _all_renders [i].sharedMaterial.color;
          _all_renders [i].sharedMaterial.color = _tag_colors [_all_renders [i].tag];
        } else {
          _original_colors [i] = _all_renders [i].material.color;
          _all_renders [i].material.color = _tag_colors [_all_renders [i].tag];
        }
      }
    }
  }
  void Restore(){
    for (int i = 0; i < _all_renders.Length; i++) {
      if (_tag_colors.ContainsKey (_all_renders [i].tag)){
        if (_use_shared_materials) {
          _all_renders [i].sharedMaterial.color = _original_colors [i];
        } else {
          _all_renders [i].material.color = _original_colors [i];
        }
      }
    }
  }

  void OnPreCull () { // change
  }

  void OnPreRender() { // change
    Change();
  }

  void OnPostRender() { // change back
    Restore();
  }

}
