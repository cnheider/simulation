using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;
using Assets.Scripts.Grasping;

public class TimedRespawn : MonoBehaviour {

  public GraspableObject _graspable_object;
  public Gripper _gripper;
  Grasp _grasp;
  Rigidbody _rigid_body;
  Vector3 _initial_position;
  Quaternion _initial_rotation;

	// Use this for initialization
	void Start () {
    if(!_graspable_object){
      _graspable_object = GetComponent<GraspableObject> ();
    }

    if(!_gripper){
      _gripper = FindObjectOfType<Gripper> ();
    }

    _grasp = _graspable_object.GetOptimalGrasp (_gripper).First;
    _rigid_body =  _grasp.GetComponentInParent<Rigidbody> ();
    _initial_position = _rigid_body.transform.position;
    _initial_rotation = _rigid_body.transform.rotation;
      
    Utilities.RegisterCollisionTriggerCallbacksOnChildren(transform, OnCollisionEnterChild, OnTriggerEnterChild, OnCollisionExitChild, OnTriggerExitChild);
	}
	
  void OnCollisionEnterChild(GameObject child_game_object, Collision collision){
    if (collision.gameObject.tag == "Floor") {
      StopCoroutine ("RespawnObject");
      StartCoroutine ("RespawnObject");
    }
  }

  IEnumerator RespawnObject(){
    yield return new WaitForSeconds(.3f);
    //var copy = Instantiate (_graspable_object, _graspable_object.transform);
    //Destroy(_graspable_object);
    //_graspable_object = copy;
    StopCoroutine ("MakeObjectVisible");
    _graspable_object.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
    _rigid_body.transform.position = _initial_position;
    _rigid_body.transform.rotation = _initial_rotation;
    StartCoroutine ("MakeObjectVisible");
  }

  IEnumerator MakeObjectVisible(){
    yield return new WaitForSeconds (.3f);
    _rigid_body.transform.position = _initial_position;
    _rigid_body.transform.rotation = _initial_rotation;
    _graspable_object.GetComponentInChildren<SkinnedMeshRenderer> ().enabled = true;
  }

  void OnTriggerEnterChild(GameObject child_game_object, Collider collider){
  }

  void OnCollisionExitChild(GameObject child_game_object, Collision collision){
    if (collision.gameObject.tag == "Floor") {
      StopCoroutine ("RespawnObject");
    }
  }

  void OnTriggerExitChild(GameObject child_game_object, Collider collider){
  }

	// Update is called once per frame
	void Update () {
		
	}
}
