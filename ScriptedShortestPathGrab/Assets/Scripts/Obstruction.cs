using UnityEngine;

  public class Obstruction : MonoBehaviour {
    private Vector3 _previous_position;
    private Quaternion _previous_rotation;

    private void UpdatePreviousTranform() {
      _previous_position = this.transform.position;
      _previous_rotation = this.transform.rotation;
    }


    public bool IsMoving() {
      return this.transform.position != _previous_position || this.transform.rotation != _previous_rotation;
    }

    void Start() {
      UpdatePreviousTranform();
    }

    void Update() {
      UpdatePreviousTranform();
    }
  
}
