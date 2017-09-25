using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ChangeMaterialOnRenderByTag : MonoBehaviour {
 
  public bool _use_shared_materials = false;
  public SegmentationColorByTag[] _colors_by_tag;

  Dictionary<string, Color> _tag_colors;
  LinkedList<Color>[] _original_colors;
  Renderer[] _all_renders;

  public SegmentationColorByTag[] SegmentationColorsByTag{
    get{ return _colors_by_tag; }
  }

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
    if (_colors_by_tag.Length > 0) {
      foreach (var tag_color in _colors_by_tag) {
        if (!_tag_colors.ContainsKey (tag_color.tag)) {
          _tag_colors.Add (tag_color.tag, tag_color.color);
        }
      }
    }
  }

  void Change(){
    _original_colors = new LinkedList<Color>[_all_renders.Length];
    for( int i = 0; i < _original_colors.Length; i++ ) {
      _original_colors[i] = new LinkedList<Color>();
    }

    for (int i = 0; i < _all_renders.Length; i++) {
      if(_tag_colors.ContainsKey(_all_renders[i].tag)){
        if (_use_shared_materials) {
            foreach (var mat in _all_renders[i].sharedMaterials) {
              _original_colors [i].AddFirst (mat.color);
              mat.color = _tag_colors [_all_renders [i].tag];
            }
        
        }else{
          foreach (var mat in _all_renders[i].materials) {
            _original_colors [i].AddFirst (mat.color);
            mat.color = _tag_colors [_all_renders [i].tag];
          }
        }
        /*else if(true){
          int j = 0;
          foreach (var mat in _all_renders[i].sharedMaterials) {
            _original_colors [i].AddFirst (mat.color);
            var temporary_material = new Material (mat);
            temporary_material.color = _tag_colors [_all_renders [i].tag];
            _all_renders[i].sharedMaterials[j] = temporary_material;
            j++;
          }*/
      }
    }
  }
  void Restore(){
    for (int i = 0; i < _all_renders.Length; i++) {
      if (_tag_colors.ContainsKey (_all_renders [i].tag)){
        if (_use_shared_materials) {
            foreach (var mat in _all_renders[i].sharedMaterials) {
              mat.color = _original_colors [i].Last.Value;
              _original_colors [i].RemoveLast ();
            }
        } else {
          foreach (var mat in _all_renders[i].materials) {
            mat.color = _original_colors [i].Last.Value;
            _original_colors [i].RemoveLast ();
          }
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
