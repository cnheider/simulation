using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GUIControl : MonoBehaviour {

  enum Targets { Sill, Sardin, Button }
  Targets target;

  float gripper_target_distance;
  int iterations;
  int obstacle_num;
  PathFinding pf;

  public Text t_gripper_target_distance;
  public Text t_iterations;
  public Text t_obstacle_num;
  public Text t_wating;
  public Text t_status;

  public Slider s_distance;
  public Slider s_obstacle;

  void Start() {
    pf = GameObject.Find("Gripper").GetComponent<PathFinding>();
    t_gripper_target_distance.text = s_distance.value.ToString("0.00");
    t_obstacle_num.text = s_obstacle.value.ToString();
    t_wating.text = "";
  }

  void Update() {
    t_status.text = pf.current_hand_status.ToString();
    if (pf.current_hand_status == PathFinding.HandStatus.Waiting && pf.target_game_object != null) {
      t_wating.text = "Detecting movement\nWaiting...";

    } else if (pf.target_game_object == null && pf.current_hand_status == PathFinding.HandStatus.Waiting) {
      t_wating.text = "No more targets.";

    } else {
      t_wating.text = "";
    }
  }

  public void DistanceSlider() {
    t_gripper_target_distance.text = s_distance.value.ToString("0.00");
  }

  public void ObstacleSlider() {
    t_obstacle_num.text = s_obstacle.value.ToString();
  }

  public void ChooseTarget() {
    switch (EventSystem.current.currentSelectedGameObject.name) {
      case "Sill":
        target = Targets.Sill;
        break;

      case "Sardin":
        target = Targets.Sardin;
        break;

      case "Button":
        target = Targets.Button;
        break;

      default:
        break;
    }
    print("Target = " + target);
  }

}
