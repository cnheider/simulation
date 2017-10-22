using Assets.Scripts;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GUIControl : MonoBehaviour {

  enum Targets { Sill, Sardin, Button }
  Targets target;

  float gripper_target_distance;
  int iterations;
  int obstacle_num;
  Gripper pf;

  public Text t_gripper_target_distance;
  public Text t_iterations;
  public Text t_obstacle_num;
  public Text t_waiting;

  [Space]
  [Header("State Panel")]
  public Text gripper_state;
  public Text env_state;
  public Text pf_state;
  public Text target_state;
  public Text claw1_state;
  public Text claw2_state;

  public Slider s_distance;
  public Slider s_obstacle;

  void Start() {
    pf = GameObject.FindObjectOfType<Gripper>();
    t_gripper_target_distance.text = s_distance.value.ToString("0.00");
    t_obstacle_num.text = s_obstacle.value.ToString();
    t_waiting.text = "";
  }

  void Update() {
    gripper_state.text = pf._state.GripperState.ToString();
    env_state.text = pf._state.ObstructionMotionState.ToString();
    pf_state.text = pf._state.PathFindingState.ToString();
    target_state.text = pf._state.TargetState.ToString();
    claw1_state.text = pf._state.Claw1State.ToString();
    claw2_state.text = pf._state.Claw2State.ToString();
    if (pf._state.PathFindingState == PathFindingState.WaitingForTarget) {
      t_waiting.text = "Detecting movement\nWaiting...";
    } else {
      t_waiting.text = "";
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
