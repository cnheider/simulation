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
  Rigidbody[] _rigid_bodies;
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
    _rigid_bodies = _graspable_object.GetComponentsInChildren<Rigidbody> ();
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
    yield return new WaitForSeconds(.5f);
    StopCoroutine ("MakeObjectVisible");
    _graspable_object.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
    _rigid_body.transform.position = _initial_position;
    _rigid_body.transform.rotation = _initial_rotation;
    MakeRigidBodiesSleep ();
    StartCoroutine ("MakeObjectVisible");
  }

  void MakeRigidBodiesSleep(){
    foreach (var body in _rigid_bodies) {
      body.useGravity = false;
      //body.isKinematic = true;
      body.Sleep ();
    }
    //_rigid_body.isKinematic = true;
    //_rigid_body.useGravity = false;
    //_rigid_body.Sleep ();
  }

  void WakeUpRigidBodies(){
    foreach (var body in _rigid_bodies) {
      //body.isKinematic = false;
      body.useGravity = true;
      body.WakeUp ();
    }
    //_rigid_body.isKinematic = false;
    //_rigid_body.useGravity = true;
    //_rigid_body.WakeUp ();
  }

  IEnumerator MakeObjectVisible(){
    yield return new WaitForSeconds (.5f);
    _rigid_body.transform.position = _initial_position;
    _rigid_body.transform.rotation = _initial_rotation;
    WakeUpRigidBodies ();
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
