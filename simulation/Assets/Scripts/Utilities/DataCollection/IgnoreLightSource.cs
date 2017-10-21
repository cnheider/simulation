using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class IgnoreLightSource : MonoBehaviour {

  public Light[] _lights_to_ignore;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

  void OnPreCull () {
    foreach (var light in _lights_to_ignore) {
      light.enabled = false;
    }
  }

  void OnPreRender() {
    foreach (var light in _lights_to_ignore) {
      light.enabled = false;
    }
  }
  void OnPostRender() {
      foreach (var light in _lights_to_ignore) {
      light.enabled = true;
      }
  }
}
