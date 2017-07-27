using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GUIControl : MonoBehaviour {

  enum Targets { Sill, Sardin, Button }
  Targets target;

  float gripper_target_distance;
  int iterations;
  int obstacle_num;

  public Text t_gripper_target_distance;
  public Text t_iterations;
  public Text t_obstacle_num;
  public Text t_wating;

  public Slider s_distance;
  public Slider s_obstacle;

  void Start() {
    t_gripper_target_distance.text = s_distance.value.ToString("0.00");
    t_obstacle_num.text = s_obstacle.value.ToString();
    t_wating.text = "";
  }

  void Update() {
    if (GameObject.Find("Gripper").GetComponent<PathFinding>().current_hand_status == PathFinding.HandStatus.Waiting) {
      t_wating.text = "Detecting movement\nWaiting...";
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
