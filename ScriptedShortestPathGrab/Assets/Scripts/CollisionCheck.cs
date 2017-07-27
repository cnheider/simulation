using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionCheck : MonoBehaviour {

  public bool collided = false;
  public bool created = false;


  //private void Start() {
  //	var childrenWithColliders = GetComponentsInChildren<Collider>(transform.gameObject);

  //	foreach (Collider child in childrenWithColliders) {
  //		HandPart hand_part = child.gameObject.AddComponent<HandPart>();
  //		hand_part.ColliderDelegate = OnTriggerChild;
  //	}
  //	created = true;
  //}

  void OnTriggerChild(Collider other) {
    print("COL CHECK");
    if (other.tag == "Obstacle") {

      collided = true;
      print("Collided obstacle: " + other.gameObject.name);
    }
    Destroy(gameObject);
  }


  public bool GetCollided() {
    return collided;
  }
  public bool GetCreated() {
    return created;
  }
}
