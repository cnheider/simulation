using UnityEngine;
using Assets.Scripts;

public class Obstruction : MonoBehaviour, MotionTracker {
  private Vector3 _previous_position;
  private Quaternion _previous_rotation;
  private Vector3 _last_recorded_move;
  private Quaternion _last_recorded_rotation;

  private void UpdatePreviousTranform() {
    _previous_position = this.transform.position;
    _previous_rotation = this.transform.rotation;
  }

  private void UpdateLastRecordedTranform() {
    _last_recorded_move = this.transform.position;
    _last_recorded_rotation = this.transform.rotation;
  }


  public bool IsInMotion() {
    return this.transform.position != _previous_position || this.transform.rotation != _previous_rotation;
  }

  public bool IsInMotion(float sensitivity) {
    var distance_moved = Vector3.Distance(this.transform.position, _last_recorded_move);
    var angle_rotated = Quaternion.Angle(this.transform.rotation, _last_recorded_rotation);
    if (distance_moved > sensitivity || angle_rotated > sensitivity) {
      UpdateLastRecordedTranform();
      return true;
    } else {
      return false;
    }
  }

  void Start() {
    UpdatePreviousTranform();
    UpdateLastRecordedTranform();
  }

  void Update() {
    UpdatePreviousTranform();
  }

}
