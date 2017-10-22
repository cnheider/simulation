using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour {
  [Range(0.0f, 10.0f)]
  public float _time_scale = 1f;
  float interval_size = 0.02f;

	// Use this for initialization
	void Start () {
    Time.timeScale = _time_scale;
    Time.fixedDeltaTime = interval_size * Time.timeScale;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
