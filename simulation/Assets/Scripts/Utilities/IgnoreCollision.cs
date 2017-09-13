using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnoreCollision : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

  void OnCollisionEnter(Collision collision) {
    if (collision.gameObject.tag == "ignored_by_sub_collider_fish") {
      Physics.IgnoreCollision(this.GetComponent<Collider>(), collision.collider);
    }
  }

  void OnCollisionExit(Collision collision) {
    if (collision.gameObject.tag == "ignored_by_sub_collider_fish") {
      Physics.IgnoreCollision(this.GetComponent<Collider>(), collision.collider);
    }
  }

  private void OnCollisionStay(Collision collision) {
    if (collision.gameObject.tag == "ignored_by_sub_collider_fish") {
      Physics.IgnoreCollision(this.GetComponent<Collider>(), collision.collider);
    }
  }
}
