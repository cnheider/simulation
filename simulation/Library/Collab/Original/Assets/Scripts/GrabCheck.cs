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
        if (!is_moving)
        {
            CalcValid();
        }
        //Debug.Log("CUBE: " + transform.position);
  }

  public Vector3 GetBestGripVector(Vector3 hand_position)
    {
        Transform candidate = null;
        float shortest_distance = Mathf.Infinity;
        int i = 0;
        //Debug.Log(grab_vectors.Count);
        //Debug.Log(obscured_surfaces.Count);
        foreach(bool obstructed in obscured_surfaces)
        {
            if (!obstructed)
            {

                var distance = Vector3.Distance(hand_position, grab_vectors[i].position);
                if (distance < shortest_distance || candidate == null)
                {
                    //GameObject grab = whatever;
                    Color mark_color = new Color(255, 0, 0, 1);
                    MeshRenderer gameObjectRenderer = grab_vectors[i].GetComponent<MeshRenderer>();
                    Material newMaterial = new Material(Shader.Find("IgnoreZ"));
                    newMaterial.color = mark_color;
                    gameObjectRenderer.material = newMaterial;


                    candidate = grab_vectors[i];
                    shortest_distance = distance;
                }
            }
            ++i;
        }
        if (candidate == null)
        {
            return Vector3.zero;
        }
        else
        {
            ChangeIndicatorColor(candidate, "green");
            return candidate.position;
        }
    }

  public Transform GetBestGripTransform(Vector3 hand_position) {
    Transform candidate = null;
    float shortest_distance = Mathf.Infinity;
    int i = 0;
    //Debug.Log(grab_vectors.Count);
    //Debug.Log(obscured_surfaces.Count);
    foreach (bool obstructed in obscured_surfaces) {
      if (!obstructed) {

        var distance = Vector3.Distance(hand_position, grab_vectors[i].position);
        if (distance < shortest_distance || candidate == null) {
          //GameObject grab = whatever;
          Color mark_color = new Color(255, 0, 0, 1);
          MeshRenderer gameObjectRenderer = grab_vectors[i].GetComponent<MeshRenderer>();
          Material newMaterial = new Material(Shader.Find("IgnoreZ"));
          newMaterial.color = mark_color;
          gameObjectRenderer.material = newMaterial;


          candidate = grab_vectors[i];
          shortest_distance = distance;
        }
      }
      ++i;
    }
        ChangeIndicatorColor(candidate, "green");
        return candidate;

  }

    void ChangeIndicatorColor(Transform parent, string color)
    {
        Color mark_color_green = new Color(0, 255, 0, 1);
        Color mark_color_red = new Color(255, 0, 0, 1);
        Color mark_color_white = new Color(255, 255, 255, 1);
        MeshRenderer gameObjectRenderer;

        foreach (Transform child in parent)
        {
            gameObjectRenderer = child.GetComponent<MeshRenderer>();
            Material newMaterial = new Material(Shader.Find("IgnoreZ"));
            if (color == "green")
            {
                newMaterial.SetColor("_Color1", mark_color_green);
            }
            else if (color == "red")
            {
                newMaterial.SetColor("_Color1", mark_color_red);
            }
            else if (color == "white")
            {
                newMaterial.SetColor("_Color1", mark_color_white);
            }
            gameObjectRenderer.material = newMaterial;
        }
    }

    public void CalcValid() {
    obscured_surfaces = new List<bool>();
        foreach(Transform grab_vector in grab_vectors)
        {
          if (Physics.Raycast(transform.position, -grab_vector.forward, proximity_radius)) {
                ChangeIndicatorColor(grab_vector, "red");
                obscured_surfaces.Add(true);
          } else {
            ChangeIndicatorColor(grab_vector, "white");
            obscured_surfaces.Add(false);
          }
        }
    }
}
