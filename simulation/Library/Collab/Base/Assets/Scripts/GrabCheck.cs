using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabCheck : MonoBehaviour {

    List<Transform> grab_vectors;
    List<bool> obscured_surfaces;
    Transform[] valid_grab_points;
    Transform best_grab_point;
    bool is_moving;
    float proximity_radius;
    float distance;
    Vector3 cur_pos;
    Vector3 last_pos;

    void Start () {
      proximity_radius = 10;
      grab_vectors = new List<Transform>();
      obscured_surfaces = new List<bool>();
      foreach (Transform childTransform in this.transform){
          grab_vectors.Add(childTransform);
      }
      CalcValid();
    }

    public bool GetIsMoving()
    {
        return is_moving;
    }

    void Update () {
        cur_pos = this.transform.position;
        if (cur_pos == last_pos) { is_moving = false; } else { is_moving = true; }
        last_pos = cur_pos;
        //CalcValid();
      Debug.Log("CUBE: " + transform.position);

  }

  public Vector3 GetBestGrip(Vector3 hand_position)
    {
        Transform candidate = this.transform;
        float shortest_distance = Vector3.Distance(hand_position, candidate.position);
        int i = 0;
    Debug.Log(grab_vectors.Count);
    Debug.Log(obscured_surfaces.Count);
        foreach(bool obstructed in obscured_surfaces)
        {
            if (!obstructed)
            {
                var distance = Vector3.Distance(hand_position, grab_vectors[i].position);
                if (distance < shortest_distance)
                {
                    candidate = grab_vectors[i];
                    shortest_distance = distance;
                }
            }
            ++i;
        }

        return candidate.position;
    }

    public void CalcValid() {
    obscured_surfaces = new List<bool>();
        foreach(Transform grab_vector in grab_vectors)
        {
          if (Physics.Raycast(transform.position, -grab_vector.forward, proximity_radius)) {
            obscured_surfaces.Add(true);
          } else {
            obscured_surfaces.Add(false);
          }
        }
    }

    float CalcDistance(Transform point) {
        Transform hand = transform.Find("Hand").transform;
        distance = Mathf.Sqrt(Mathf.Pow((hand.position.x - point.transform.position.x), 2) +
                              Mathf.Pow((hand.position.y - point.transform.position.y), 2) +
                              Mathf.Pow((hand.position.z - point.transform.position.z), 2));
        return distance;
    }
}
