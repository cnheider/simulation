
using UnityEngine;

namespace Assets.Scripts.Grasping {

  [ExecuteInEditMode]
  public class Grasp : MonoBehaviour {

    public float _obstruction_cast_length = 0.1f;
    public float _obstruction_cast_radius = 0.01f;
    public bool _draw_ray_cast;

    private void Update() {
      var color = Color.white;
      if (IsObstructed())
        color = Color.red;
      if (_draw_ray_cast) {
        Debug.DrawLine(this.transform.position, this.transform.position - this.transform.forward * _obstruction_cast_length, color);
        Debug.DrawLine(this.transform.position - this.transform.up * _obstruction_cast_radius, this.transform.position + this.transform.up * _obstruction_cast_radius, color);
        Debug.DrawLine(this.transform.position - this.transform.right * _obstruction_cast_radius, this.transform.position + this.transform.right * _obstruction_cast_radius, color);
      }
    }

    public bool IsObstructed() {
      RaycastHit hit;
      if (Physics.Linecast(this.transform.position, this.transform.position - this.transform.forward * _obstruction_cast_length))
        return true;
      if (Physics.SphereCast(this.transform.position, _obstruction_cast_radius, -this.transform.forward, out hit, _obstruction_cast_length))
        return true;
      return false;
    }
  }
}
