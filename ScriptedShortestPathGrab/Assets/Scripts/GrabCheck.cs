using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabCheck : MonoBehaviour {

  GameObject hand;
  public Transform center_point;
  public bool grabs_visible = true;

  List<Transform> grab_vectors;
  Vector3 cur_pos, last_pos;
  float proximity_radius = 1f;
  bool is_moving;
  int good_ori = 1;
  int bad_ori = 0;

  [Space(2)]
  [Header("Floor Distance Score")]
  public int floor_dist_score = 1;
  [Space(1)]
  [Header("Obstacles Score")]
  public int no_block = 1;
  public int obs_far = 0;
  public int obs_close = -1;
  public int obs_very_close = -2;
  [Space(1)]
  [Header("Hand Distance Score")]
  public int hand_dist_score = 1;

  public int highest_score_pub = 0;

  void Start() {
    hand = GameObject.Find("Gripper");
    AddGrabsToList();
    CalculateBestGrabPoint();
    GrabVisible(grabs_visible);
  }

  void Update() {
    MovementCheck();
    IsTargetCheck();
  }

  void AddGrabsToList() {
    grab_vectors = new List<Transform>();
    Transform[] go = gameObject.GetComponentsInChildren<Transform>();
    foreach (Transform child in go) {
      if (child.tag == "GrabTag") {
        grab_vectors.Add(child);
      }
    }
  }

  void IsTargetCheck() {
    GameObject curr_tar = GameObject.Find("Gripper").GetComponent<PathFinding>().target_game_object;
    //print("Curr_tar = " + curr_tar.name + " AND " + "GO = " + gameObject.name);
    GrabVisible(curr_tar == gameObject ? true : false);
  }

  void GrabVisible(bool is_visible) {
    foreach (Transform grab in grab_vectors) {
      grab.gameObject.SetActive(is_visible);
    }
  }

  public bool GetIsMoving() {
    return is_moving;
  }

  void MovementCheck() {
    cur_pos = transform.position;
    is_moving = (cur_pos == last_pos) ? false : true;
    last_pos = cur_pos;

    if (!is_moving) {
      CalculateBestGrabPoint();
    }
  }

  public Transform GetUpTransform() {
    var up_vec = transform.up;
    var up_pos = transform.position;
    var new_pos = up_pos + (up_vec * 2f);
    var go = new GameObject();
    go.transform.position = new_pos;
    go.transform.rotation = transform.rotation;
    return go.transform;
  }

  private void ChangeIndicatorColor(Transform parent, string color) {
    Color mark_color_green = new Color(0, 255, 0, 1);
    Color mark_color_white = new Color(255, 255, 255, 1);
    Color mark_color_orange = new Color(255, 160, 0, 1);
    Color mark_color_red = new Color(255, 0, 0, 1);
    Color mark_color_black = new Color(0, 0, 0, 1);

    MeshRenderer gameObjectRenderer;

    foreach (Transform child in parent) {
      if ((child != null) && (child.GetComponent<MeshRenderer>() != null)) {
        gameObjectRenderer = child.GetComponent<MeshRenderer>();
        Material newMaterial = new Material(Shader.Find("IgnoreZ"));
        if (color == "green") {
          newMaterial.SetColor("_Color1", mark_color_green);

        } else if (color == "red") {
          newMaterial.SetColor("_Color1", mark_color_red);

        } else if (color == "white") {
          newMaterial.SetColor("_Color1", mark_color_white);

        } else if (color == "orange") {
          newMaterial.SetColor("_Color1", mark_color_orange);

        } else if (color == "black") {
          newMaterial.SetColor("_Color1", mark_color_black);
        }
        gameObjectRenderer.material = newMaterial;
      }
    }
  }

  //Main function
  //return grip vector/transform with highest score
  public Transform CalculateBestGrabPoint() {
    List<int> score_list = new List<int>(grab_vectors.Count);

    //Making the scores for each grab_vector = 0
    for (int i = 0; i < grab_vectors.Count; i++) {
      score_list.Add(0);
    }

    //Aquiring alle the scores
    score_list = DistanceToFloorScore(score_list);
    score_list = DistanceToObstacleScore(score_list);
    score_list = DistanceToHandScore(score_list);
    //score_list = OrientationScore(score_list);

    //Finding index with highest score
    int highest_score = 0;
    int best_index = 0;
    for (int i = 0; i < score_list.Count; i++) {
      if (score_list[i] > highest_score) {
        highest_score = score_list[i];
        best_index = i;
      }
    }

    //Updating name of each grab to show their score
    for (int i = 0; i < score_list.Count; i++) {
      grab_vectors[i].name = "[i" + i + "]: " + score_list[i].ToString();
    }

    highest_score_pub = highest_score;
    if (highest_score <= -1) {
      print("(!) No suitable grab points for target object... ah man");
      return null;
    } else {
      ChangeIndicatorColor(grab_vectors[best_index], "green");
      return grab_vectors[best_index];
    }
  }

  //Distance from floor - Score
  List<int> DistanceToFloorScore(List<int> score_list) {
    RaycastHit floorHit;
    float furthest_distance = -1;
    int index = 0;
    int furthest_index = 0;

    //Giving point to the grab vector furthest away from the floor
    foreach (Transform grab_vector in grab_vectors) {

      Vector3 point = grab_vector.Find("Point").position;

      if (Physics.Raycast(center_point.position, (point - center_point.position).normalized, Vector3.Distance(center_point.position, point))) {
        score_list[index] -= 100;

      } else if (Physics.Raycast(point, Vector3.down, out floorHit, LayerMask.NameToLayer("Floor"))) {

        float current_distance = floorHit.distance;
        if (current_distance > furthest_distance) {
          furthest_distance = current_distance;
          score_list[index] += floor_dist_score;
          if (index != 0) {
            score_list[furthest_index] -= floor_dist_score;
          }
          furthest_index = index;
        }

      }
      index++;
    }
    return score_list;
  }

  //Distance from obstacles (range and if blocked) - Score
  List<int> DistanceToObstacleScore(List<int> score_list) {

    int index = 0;
    RaycastHit ray_hit;
    foreach (Transform grab_vector in grab_vectors) {
      Vector3 point = grab_vector.Find("Point").position;

      if (Physics.Raycast(grab_vector.position, -grab_vector.forward, out ray_hit, proximity_radius)) {

        bool hand_col = (ray_hit.transform.tag == "Hand") ? true : false;

        if (Vector3.Distance(ray_hit.point, grab_vector.position) > 0.3f && !hand_col) {
          score_list[index] += obs_far;
          ChangeIndicatorColor(grab_vector, "orange");

        } else if (Vector3.Distance(ray_hit.point, grab_vector.position) <= 0.3f && !hand_col) {
          score_list[index] += obs_close;
          ChangeIndicatorColor(grab_vector, "red");
        }

      } else if (Physics.Raycast(center_point.position, (point - center_point.position).normalized, Vector3.Distance(center_point.position, point))) {

        score_list[index] += obs_very_close;
        ChangeIndicatorColor(grab_vector, "black");

      } else {
        score_list[index] += no_block;
        ChangeIndicatorColor(grab_vector, "white");
      }

      index++;
    }
    return score_list;
  }

  //Distance from hand - Score
  List<int> DistanceToHandScore(List<int> score_list) {
    Vector3 hand_pos = hand.transform.position;
    int index = 0;
    int closest_index = 0;
    float closest_distance = Mathf.Infinity;
    foreach (Transform grab_vector in grab_vectors) {

      float current_distance = Vector3.Distance(grab_vector.position, hand_pos);
      if (current_distance < closest_distance) {
        closest_distance = current_distance;
        score_list[index] += hand_dist_score;
        if (index != 0) {
          score_list[closest_index] -= hand_dist_score;
        }
        closest_index = index;
      }
      index++;
    }

    return score_list;
  }

  //Orientation (normal to floor) - Score
  //----OBSOLETE----//
  List<int> OrientationScore(List<int> score_list) {

    int index = 0;
    RaycastHit ray_hit;

    foreach (Transform grab_vector in grab_vectors) {
      if (Physics.Raycast(grab_vector.position, grab_vector.right, out ray_hit, proximity_radius)) {
        if (ray_hit.transform.name == "Floor") {
          score_list[index] += good_ori;
        }
      } else if (Physics.Raycast(grab_vector.position, -grab_vector.right, out ray_hit, proximity_radius)) {
        if (ray_hit.transform.name == "Floor") {
          score_list[index] += good_ori;
        }
      } else {
        score_list[index] += bad_ori;
      }
      index++;
    }
    return score_list;
  }

}
