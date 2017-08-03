using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts {
  public class States {
    private Action _on_state_update_callback;

    public enum EnvironmentState {
      IsAtRest,
      WasMoving,
      Moving
    }

    public enum PathFindingState {
      Idling,
      Waiting,
      Navigating,
      Approaching,
      Returning
    }

    public enum GripperState {
      Closed,
      Open,
      Closing,
      Opening
    };

    public enum ClawState {
      TouchingTarget,
      NotTouchingTarget
    }

    public enum TargetState {
      Grabbed,
      OutsideRegion,
      InsideRegion
    }

    public States(Action on_state_update_callback = null) {
      if (on_state_update_callback != null)
        _on_state_update_callback = on_state_update_callback;
      else
        _on_state_update_callback = null_print;
    }

    private void null_print() {
      //Debug.Log("null");
    }

    private TargetState _current_target_state;
    private EnvironmentState _current_environment_state;
    private PathFindingState _current_path_finding_state;
    private GripperState _current_gripper_state;
    private GripperState _previous_gripper_state;
    private ClawState _current_claw_1_state, _current_claw_2_state;

    public ClawState CurrentClaw1State {
      get { return _current_claw_1_state; }
      set { _current_claw_1_state = value;
        _on_state_update_callback();
      }
    }

    public ClawState CurrentClaw2State {
      get { return _current_claw_2_state; }
      set { _current_claw_2_state = value;
        _on_state_update_callback();
      }
    }

    public TargetState CurrentTargetState {
      get { return _current_target_state; }
      set { _current_target_state = value;
        _on_state_update_callback();
      }
    }

    public GripperState CurrentGripperState {
      get { return _current_gripper_state; }
      set {
        _current_gripper_state = value;
        _on_state_update_callback();
      }
    }

    public PathFindingState CurrentPathFindingState {
      get { return _current_path_finding_state; }
      set {
        _current_path_finding_state = value;
        _on_state_update_callback();
      }
    }

    public EnvironmentState CurrentEnvironmentState {
      get { return _current_environment_state; }
      set {
        _current_environment_state = value;
        _on_state_update_callback();
      }
    }

    public GripperState PreviousGripperState { get; set; }

  }
}
