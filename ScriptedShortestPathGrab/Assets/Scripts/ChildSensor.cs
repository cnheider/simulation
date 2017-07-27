using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildSensor : MonoBehaviour {
  public delegate void OnCollisionDelegate(GameObject child_game_object, Collision collision);
  public delegate void OnTriggerDelegate(GameObject child_game_object, Collider collider);


  OnCollisionDelegate collisionDelegate;
  public OnCollisionDelegate CollisionDelegate {
    set { collisionDelegate = value; }
  }


  OnTriggerDelegate triggerDelegate;
  public OnTriggerDelegate TriggerDelegate {
    set { triggerDelegate = value; }
  }


  private void OnCollisionEnter(Collision collision) {
    if (collisionDelegate != null) {
      collisionDelegate(this.gameObject, collision);
    }

  }

  private void OnTriggerEnter(Collider other) {
    if (triggerDelegate != null) {
      triggerDelegate(this.gameObject, other);
    }
  }
}
