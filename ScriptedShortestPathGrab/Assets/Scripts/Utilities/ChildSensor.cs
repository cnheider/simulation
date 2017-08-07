using UnityEngine;

public class ChildSensor : MonoBehaviour {
  public delegate void OnChildCollisionEnterDelegate(GameObject child_game_object, Collision collision);
  public delegate void OnChildTriggerEnterDelegate(GameObject child_game_object, Collider collider);

  public delegate void OnChildCollisionStayDelegate(GameObject child_game_object, Collision collision);
  public delegate void OnChildTriggerStayDelegate(GameObject child_game_object, Collider collider);

  public delegate void OnChildCollisionExitDelegate(GameObject child_game_object, Collision collision);
  public delegate void OnChildTriggerExitDelegate(GameObject child_game_object, Collider collider);


  OnChildCollisionEnterDelegate _on_collision_enter_delegate;
  public OnChildCollisionEnterDelegate OnCollisionEnterDelegate {
    set { _on_collision_enter_delegate = value; }
  }

  OnChildTriggerEnterDelegate _on_trigger_enter_delegate;
  public OnChildTriggerEnterDelegate OnTriggerEnterDelegate {
    set { _on_trigger_enter_delegate = value; }
  }

  OnChildTriggerStayDelegate _on_trigger_stay_delegate;
  public OnChildTriggerStayDelegate OnTriggerStayDelegate {
    set { _on_trigger_stay_delegate = value; }
  }

  OnChildCollisionStayDelegate _on_collision_stay_delegate;
  public OnChildCollisionStayDelegate OnCollisionStayDelegate {
    set { _on_collision_stay_delegate = value; }
  }

  OnChildCollisionExitDelegate _on_collision_exit_delegate;
  public OnChildCollisionExitDelegate OnCollisionExitDelegate {
    set { _on_collision_exit_delegate = value; }
  }

  OnChildTriggerExitDelegate _on_trigger_exit_delegate;
  public OnChildTriggerExitDelegate OnTriggerExitDelegate {
    set { _on_trigger_exit_delegate = value; }
  }


  private void OnCollisionEnter(Collision collision) {
    if (_on_collision_enter_delegate != null) {
      _on_collision_enter_delegate(this.gameObject, collision);
    }
  }

  private void OnTriggerEnter(Collider other) {
    if (_on_trigger_enter_delegate != null) {
      _on_trigger_enter_delegate(this.gameObject, other);
    }
  }

  private void OnTriggerStay(Collider other) {
    if (_on_trigger_stay_delegate != null) {
      _on_trigger_stay_delegate(this.gameObject, other);
    }
  }

  private void OnCollisionStay(Collision collision) {
    if (_on_collision_stay_delegate != null) {
      _on_collision_stay_delegate(this.gameObject, collision);
    }
  }

  private void OnTriggerExit(Collider other) {
    if (_on_trigger_exit_delegate != null) {
      _on_trigger_exit_delegate(this.gameObject, other);
    }
  }

  private void OnCollisionExit(Collision collision) {
    if (_on_collision_exit_delegate != null) {
      _on_collision_exit_delegate(this.gameObject, collision);
    }
  }
}
