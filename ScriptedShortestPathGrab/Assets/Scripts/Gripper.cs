using UnityEngine;
using Assets.Scripts;
using Assets.Scripts.Grasping;

public class Gripper : MonoBehaviour {
  GrabableObject _target_game_object;
  Vector3 _approach_position;
  Grasp _target_grasp;
  Path _path;

  bool _target_pos_updated = false;
  bool _target_grabbed = false;
  bool _obstructions_moved_last_step = false;
  bool _scene_updated_last_step = false;

  Vector3 _default_motor_position;

  Vector3 _intermediate_target;
  GameObject[] _targets;

  Transform[] _old_obstruction_transforms;

  Vector3 _reset_position;
  Vector3 _escape_position;

  [Space(1)]
  [Header("Game Objects")]
  public GameObject _motor;
  public GameObject _grab_region;
  public GameObject _claw_1, _claw_2;
  public States _states;

  [Space(1)]
  [Header("Path Finding Parameters")]
  public float _search_boundary = 10f;
  public float _agent_size = 0.2f;
  public float _grid_granularity = 0.3f;
	public float _speed = 0.5f;
  public float _precision = 0.01f;

  [Space(1)]
	[Header("Bezier Curve Controls")]
	public float _approach_distance = 0.5f;
	public float _curv_pos_scaling = -0.005f;
	public float _progress_scaling = 0.01f;

  [Space(1)]
  [Header("Show debug logs")]
  public bool _debug = false;

  [Space(1)]
  [Header("Draw Search Boundary")]
  public bool _draw_search_boundary = true;

  float _step_size;


  private void Start() {
    _states = new States();
    _reset_position = transform.position;
    _escape_position = transform.position;
    _default_motor_position = _motor.transform.localPosition;

    ResetStates();
    SetupEnvironment();
  }

  private void SetupEnvironment() {
    Utilities.RegisterCollisionTriggerCallbacksOnChildren(transform, OnCollisionEnterChild, OnTriggerEnterChild, OnCollisionExitChild, OnTriggerExitChild);
    var obstructions = GameObject.FindGameObjectsWithTag("Obstruction");

    Transform[] obstructions_transforms = new Transform[obstructions.Length];
    var i = 0;
    foreach (var obstruction in obstructions) {
      obstructions_transforms[i] = obstruction.transform;
      i++;
    }
    _old_obstruction_transforms = obstructions_transforms;
  }

  private void ResetStates() {
    _states.CurrentTargetState = States.TargetState.OutsideRegion;
    _states.CurrentEnvironmentState = States.EnvironmentState.IsAtRest;
    _states.CurrentGripperState = States.GripperState.Open;
    _states.CurrentPathFindingState = States.PathFindingState.Idling;
    _states.CurrentClaw1State = States.ClawState.NotTouchingTarget;
    _states.CurrentClaw2State = States.ClawState.NotTouchingTarget;
  }

  private void ResetEnvironment() {

  }

  private Pair<GrabableObject,Grasp> GetOptimalTargetAndGrasp() {
    var targets = FindObjectsOfType<GrabableObject>();
    if (targets.Length == 0) {
      return null;
    }
    float shortest_distance = float.MaxValue;
    GrabableObject optimal_target = null;
    Grasp optimal_grasp = null;
    foreach (GrabableObject target in targets) {
      var pair = target.GetOptimalGrasp(this);
      if (pair != null) {
        var target_grasp = pair.First;
        var distance = pair.Second;
        if (distance < shortest_distance) {
          shortest_distance = distance;
          optimal_grasp = target_grasp;
          optimal_target = target;
        }
      }
    }
    return new Pair<GrabableObject, Grasp>(optimal_target, optimal_grasp);
  }

  public void UpdatePath() {
    var pair = GetOptimalTargetAndGrasp();
    _target_game_object = pair.First;
    _target_grasp = pair.Second;
    _approach_position = _target_grasp.transform.position - (_target_grasp.transform.forward * _approach_distance);
    _path = FindPath(this.transform.position, _approach_position);
    _intermediate_target = _path.Next(_step_size);
  }

	private Path FindPath(Vector3 start_position, Vector3 target_position) {
    var _path_list = PathFinding.FindPathAstar(start_position, target_position, _search_boundary, _grid_granularity, _agent_size, _approach_distance);

    _path_list.Add(target_position);
    _path_list = PathFinding.SimplifyPath(_path_list);
    var path = new Path(start_position, target_position, GameObject.Find("Bezier_Curve").gameObject, _path_list);
    return path;
  }

  private void Update() {
    _step_size = _speed * Time.deltaTime;

    if(_draw_search_boundary) Utilities.DrawBoxFromCenter(this.transform.position, _search_boundary, Color.magenta);

    _states.CurrentEnvironmentState = GetEnvironmentState(_states.CurrentEnvironmentState);

    if (_debug) {
      Debug.Log("CurrentTargetState: " + _states.CurrentTargetState);
      Debug.Log("CurrentPathFindingState: " + _states.CurrentPathFindingState);
      Debug.Log("CurrentGripperState: " + _states.CurrentGripperState);
      Debug.Log("CurrentEnvironmentState: " + _states.CurrentEnvironmentState);
      Debug.Log("CurrentClaw1State: " + _states.CurrentClaw1State);
      Debug.Log("CurrentClaw2State: " + _states.CurrentClaw2State);
    }

    switch (_states.CurrentPathFindingState) {
      case States.PathFindingState.Idling:
        switch (_states.CurrentEnvironmentState) {
          case States.EnvironmentState.Moving:
            break;
          case States.EnvironmentState.WasMoving:
            UpdatePath();
            _states.CurrentPathFindingState = States.PathFindingState.Navigating;
            break;
          case States.EnvironmentState.IsAtRest:
            break;
        }
        break;

      case States.PathFindingState.Navigating:
        switch (_states.CurrentEnvironmentState) {
          case States.EnvironmentState.Moving:
            _states.CurrentPathFindingState = States.PathFindingState.Idling;
            break;
        }
        FollowPath(_step_size);
        _states.CurrentGripperState = States.GripperState.Opening;
        CheckIfGripperIsOpen();
        MaybeBeginApproachProcedure();
        break;

      case States.PathFindingState.Approaching:
        ApproachTarget(_step_size);
        break;

      case States.PathFindingState.Returning:
        switch (_states.CurrentEnvironmentState) {
          case States.EnvironmentState.Moving:
            break;
          case States.EnvironmentState.WasMoving:
            _path = FindPath(this.transform.position, _reset_position);
            break;
          case States.EnvironmentState.IsAtRest:
            break;
        }
        _path = FindPath(this.transform.position, _reset_position);
        FollowPath(_step_size, false);
        MaybeBeginReleaseProcedure();
        break;
    }

    switch (_states.CurrentGripperState) {
      case States.GripperState.Opening:
        OpenClaws(_step_size);
        break;

      case States.GripperState.Closing:
        CloseClaws(_step_size);
        break;
    }

    if (_states.CurrentGripperState == States.GripperState.Closed && _states.CurrentTargetState != States.TargetState.InsideRegion) {
      _states.CurrentPathFindingState = States.PathFindingState.Navigating;
    }
  }
  /*
  public void respawn_obstructions(States.GripperState state) {
    ObstacleSpawner obstacles_spawner = FindObjectOfType<ObstacleSpawner>();
    obstacles_spawner.SpawnObstacles(obstacles_spawner.number_of_cubes, obstacles_spawner.number_of_spheres);
  }*/

  private States.EnvironmentState GetEnvironmentState(States.EnvironmentState previous_state) {
    var obstructions = FindObjectsOfType<Obstruction>();
    foreach (var obstruction in obstructions) {
      if (obstruction.IsMoving())
        return States.EnvironmentState.Moving;
    }

    var targets = FindObjectsOfType<GrabableObject>();
    foreach(var target in targets) {
      if(target.IsMoving())
        return States.EnvironmentState.Moving;
    }

    if(previous_state == States.EnvironmentState.WasMoving) {
      return States.EnvironmentState.IsAtRest;
    }

    return States.EnvironmentState.WasMoving;
	}

  private void MaybeBeginReleaseProcedure() {
    if (Vector3.Distance(this.transform.position, _path._target_position) < _precision ) {
      Destroy(_target_game_object);
      _states.CurrentPathFindingState = States.PathFindingState.Navigating;
    }
  }

  private void MaybeBeginApproachProcedure() {
		if (Vector3.Distance(this.transform.position, _path._target_position) < _approach_distance && Quaternion.Angle(this.transform.rotation, _target_grasp.transform.rotation) < _precision && _states.CurrentGripperState == States.GripperState.Open) {
      _states.CurrentPathFindingState = States.PathFindingState.Approaching;
    }
	}

  private void CheckIfGripperIsOpen() {
    if (Vector3.Distance(_motor.transform.localPosition, _default_motor_position) < _precision) {
      _states.CurrentGripperState = States.GripperState.Open;
    } 
  }

	private void CloseClaws(float step_size) {
   Vector3 current_motor_position = _motor.transform.localPosition;
    current_motor_position.y += step_size/8;
    _motor.transform.localPosition = current_motor_position;
  }

	private void OpenClaws(float step_size) {
    _motor.transform.localPosition = Vector3.MoveTowards(_motor.transform.localPosition, _default_motor_position, step_size);
  }

	private void OnTriggerEnterChild(GameObject child_game_object, Collider other_game_object) {
    if (child_game_object.name == _grab_region.name && other_game_object.transform.name == _target_game_object.name) {
      other_game_object.transform.root.parent = transform;
      _states.CurrentTargetState = States.TargetState.InsideRegion;
      _states.CurrentGripperState = States.GripperState.Closing;
      _states.CurrentPathFindingState = States.PathFindingState.Waiting;
    }

    if (child_game_object.tag == "Stopper"  && other_game_object.transform.tag == "Stopper") {
      _states.CurrentGripperState = States.GripperState.Closed;
    }
  }

  private void OnCollisionExitChild(GameObject child_game_object, Collision collision) {
    if (collision.gameObject.GetComponent<Obstruction>() != null) {
      //_states.CurrentGripperState = States.GripperState.Opening;
    }

    if (child_game_object.name == _claw_1.name && collision.gameObject.name == _target_game_object.name) {
      _states.CurrentClaw1State = States.ClawState.NotTouchingTarget;
      _target_game_object.transform.parent = null;
    }

    if (child_game_object.name == _claw_2.name && collision.gameObject.name == _target_game_object.name) {
      _states.CurrentClaw2State = States.ClawState.NotTouchingTarget;
      _target_game_object.transform.parent = null;
    }

    if (_states.CurrentClaw1State == States.ClawState.NotTouchingTarget && _states.CurrentClaw2State == States.ClawState.NotTouchingTarget) {
      _target_game_object.transform.parent = null;
    }
  }

  private void OnTriggerExitChild(GameObject child_game_object, Collider other_game_object) {
    if (child_game_object.name == _grab_region.name && other_game_object.transform.name == _target_game_object.name) {
      _target_game_object.transform.parent = null;
      _states.CurrentTargetState = States.TargetState.OutsideRegion;
      _states.CurrentPathFindingState = States.PathFindingState.Idling;
    }
  }

	private void OnCollisionEnterChild(GameObject child_game_object, Collision collision) {
		if (collision.gameObject.GetComponent<Obstruction>() != null) {
      _states.CurrentGripperState = States.GripperState.Closing;
      _states.CurrentPathFindingState = States.PathFindingState.Waiting;
    }

    if(child_game_object.name == _claw_1.name && collision.gameObject.name == _target_game_object.name) {
      _states.CurrentClaw1State = States.ClawState.TouchingTarget;
    }

    if (child_game_object.name == _claw_2.name && collision.gameObject.name == _target_game_object.name) {
      _states.CurrentClaw2State = States.ClawState.TouchingTarget;
    }

    if(_states.CurrentClaw1State == States.ClawState.TouchingTarget && _states.CurrentClaw2State == States.ClawState.TouchingTarget && _states.CurrentTargetState == States.TargetState.InsideRegion) {
      _states.CurrentTargetState = States.TargetState.Grabbed;
      _states.CurrentGripperState = States.GripperState.Closed;
      _states.CurrentPathFindingState = States.PathFindingState.Returning;
    }
  }

	private void ApproachTarget(float step_size) {
    transform.position = Vector3.MoveTowards(this.transform.position, _target_grasp.transform.position, step_size);
    //transform.rotation = Quaternion.RotateTowards(this.transform.rotation, _target_grasp.transform.rotation, step_size * 50);

    //if (_debug) Debug.DrawLine(this.transform.position, _target_grasp.transform.position, Color.red);
	}

	private void FollowPath(float step_size, bool rotate = true) {
    if (_debug) {
      Debug.Log("Following Path");
      if (rotate)
        Debug.Log("With rotation");
      else
        Debug.Log("No rotation");
    }

    if ((Vector3.Distance(this.transform.position, _intermediate_target) <= _precision)) {
      _intermediate_target = _path.Next(step_size);
		}

    if (_debug) Debug.DrawRay(_intermediate_target, this.transform.forward, Color.green);

    if (rotate) transform.rotation = Quaternion.RotateTowards(this.transform.rotation, _target_grasp.transform.rotation, step_size * 50);
    transform.position = Vector3.MoveTowards(this.transform.position, _intermediate_target,  step_size);
  }
}
