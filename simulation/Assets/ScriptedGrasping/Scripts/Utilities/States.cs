using System;

namespace Assets.Scripts {

  #region Enums
  public enum MotionState {
    IsAtRest,
    WasMoving,
    IsMoving
  }

  public enum PathFindingState {
    WaitingForTarget,
    WaitingForRestingEnvironment,
    Navigating,
    Approaching,
    PickingUpTarget,
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
    NotGrabbed,
    OutsideRegion,
    InsideRegion
  }

  #endregion

  public class States {

    public MotionState GetMotionState<T>(T[] objects, MotionState previous_state, float sensitivity = 0.1f) where T : MotionTracker {
      foreach (var o in objects) {
        if (o.IsInMotion(sensitivity))
          return MotionState.IsMoving;
      }

      if (previous_state != MotionState.IsMoving) {
        return MotionState.IsAtRest;
      }

      return MotionState.WasMoving;
    }

    private Action _on_state_update_callback;

    public States(Action on_state_update_callback = null) {
      if (on_state_update_callback != null)
        _on_state_update_callback = on_state_update_callback;
      else
        _on_state_update_callback = null_print;
    }

    private void null_print() {
      //Debug.Log("null");
    }

    public void TargetIsGrabbed() {
      TargetState = TargetState.Grabbed;
      GripperState = GripperState.Closed;
      PathFindingState = PathFindingState.Returning;
    }

    public void OpenClaw() {
      GripperState = GripperState.Opening;
    }

    public void WaitForRestingEnvironment() {
      PathFindingState = PathFindingState.WaitingForRestingEnvironment;
    }

    public void ReturnToStartPosition() {
      PathFindingState = PathFindingState.Returning;
    }

    public void NavigateToTarget() {
      PathFindingState = PathFindingState.Navigating;
    }

    public bool IsTargetGrabbed() {
      return TargetState == TargetState.Grabbed;
    }

    public void WaitForTarget() {
      PathFindingState = PathFindingState.WaitingForTarget;
    }

    public bool IsTargetNotGrabbed() {
      return TargetState != TargetState.Grabbed;
    }

    public bool IsGripperClosed() {
      return GripperState == GripperState.Closed;
    }

    public bool IsEnvironmentAtRest() {
      return TargetMotionState == MotionState.IsAtRest && ObstructionMotionState == MotionState.IsAtRest;
    }

    public bool WasEnvironmentMoving() {
      return ObstructionMotionState == MotionState.WasMoving || TargetMotionState == MotionState.WasMoving;
    }

    public bool IsEnvironmentMoving() {
      return ObstructionMotionState == MotionState.IsMoving || TargetMotionState == MotionState.IsMoving;
    }

    public bool WereObstructionMoving() {
      return ObstructionMotionState == MotionState.WasMoving;
    }

    public bool IsObstructionsAtRest() {
      return ObstructionMotionState == MotionState.IsAtRest;
    }

    public bool AreBothClawsTouchingTarget() {
      return Claw1State == ClawState.NotTouchingTarget && Claw2State == ClawState.NotTouchingTarget;
    }

    public void TargetIsNotGrabbed() {
      TargetState = TargetState.NotGrabbed;
    }

    public bool IsTargetTouchingAndInsideRegion() {
      return Claw1State == ClawState.TouchingTarget && Claw2State == ClawState.TouchingTarget && TargetState == TargetState.InsideRegion;
    }

    public bool IsTargetInsideRegionOrTouching() {
      return Claw1State == ClawState.TouchingTarget || Claw2State == ClawState.TouchingTarget || TargetState == TargetState.InsideRegion;
    }

    public bool IsGripperOpen() {
      return GripperState == GripperState.Open;
    }

    public void ApproachTarget() {
      PathFindingState = PathFindingState.Approaching;
    }

    public void GripperIsOpen() {
      GripperState = GripperState.Open;
    }

    public void PickUpTarget() {
      //TargetState = TargetState.InsideRegion;
      GripperState = GripperState.Closing;
      PathFindingState = PathFindingState.PickingUpTarget;
    }

    public void GripperIsClosed() {
      GripperState = GripperState.Closed;
    }

    public void Claw1IsNotTouchingTarget() {
      Claw1State = ClawState.NotTouchingTarget;
      //TargetIsNotGrabbed();
    }

    public void Claw2IsNotTouchingTarget() {
      Claw2State = ClawState.NotTouchingTarget;
      //TargetIsNotGrabbed();
    }


    public void Claw2IsTouchingTarget() {
      Claw2State = ClawState.TouchingTarget;
    }
    public void Claw1IsTouchingTarget() {
      Claw1State = ClawState.TouchingTarget;
    }

    public void TargetIsOutsideRegion() {
      TargetState = TargetState.OutsideRegion;
    }

    public void TargetIsInsideRegion() {
      TargetState = TargetState.InsideRegion;
    }
    public bool IsPickingUpTarget() {
      return PathFindingState == PathFindingState.PickingUpTarget;
    }

    public void ResetStates() {
      TargetState = TargetState.OutsideRegion;
      ObstructionMotionState = MotionState.IsAtRest;
      TargetMotionState = MotionState.IsAtRest;
      GripperState = GripperState.Open;
      PathFindingState = PathFindingState.WaitingForTarget;
      Claw1State = ClawState.NotTouchingTarget;
      Claw2State = ClawState.NotTouchingTarget;
    }

    private TargetState _current_target_state;
    private MotionState _obstruction_motion_state, _target_motion_state;
    private PathFindingState _current_path_finding_state;
    private GripperState _current_gripper_state;
    private ClawState _current_claw_1_state, _current_claw_2_state;

    public ClawState Claw1State {
      get { return _current_claw_1_state; }
      set {
        _current_claw_1_state = value;
        _on_state_update_callback();
      }
    }

    public ClawState Claw2State {
      get { return _current_claw_2_state; }
      set {
        _current_claw_2_state = value;
        _on_state_update_callback();
      }
    }

    public TargetState TargetState {
      get { return _current_target_state; }
      set {
        _current_target_state = value;
        _on_state_update_callback();
      }
    }

    public GripperState GripperState {
      get { return _current_gripper_state; }
      set {
        _current_gripper_state = value;
        _on_state_update_callback();
      }
    }

    public PathFindingState PathFindingState {
      get { return _current_path_finding_state; }
      set {
        _current_path_finding_state = value;
        _on_state_update_callback();
      }
    }

    public MotionState ObstructionMotionState {
      get { return _obstruction_motion_state; }
      set {
        _obstruction_motion_state = value;
        _on_state_update_callback();
      }
    }

    public MotionState TargetMotionState {
      get { return _target_motion_state; }
      set {
        _target_motion_state = value;
        _on_state_update_callback();
      }
    }
  }
}
