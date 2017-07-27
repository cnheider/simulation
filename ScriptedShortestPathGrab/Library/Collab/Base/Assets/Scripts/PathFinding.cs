using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinding : MonoBehaviour {

  public GameObject slider;
  GameObject[] targets;
  public GameObject target_game_object;
	public GameObject hand_base, claw_1, claw_2;
	public Transform escape_position;
	public Transform[] fish_to_grab;
	public int fish_index = 0;

	[Space(1)]
	[Header("Show debug logs and lines?")]
	public bool debug = false;

  [Space(1)]
  [Header("Path finding parameters")]
  public float search_boundary = 20f;
  public float near_stopping_distance = 1f;
  public float agent_size = 0.2f;
  public float grid_granularity = 0.3f;

  [Space(1)]
	[Header("Hand Controls")]
	public int claw_rot_speed = 100;
	public float speed = 1f;
	public float turn_speed = 4f;

	[Space(1)]
	[Header("Bezier Curve Controls")]
  
	public float curv_pos_scaling = -0.005f;
	public float progress_scaling = 0.02f;
	public float approach_distance = 0.2f;
	public int point_iterations = 5;

	Vector3 target = Vector3.zero;
	Vector3[] old_obstruction_positions;
	GameObject[] obstructions;
	BezierPoint[] bez_points;
	BezierCurve bez_curve;

	enum HandStatus { GoingToTarget, ApproachingTarget, PickingUpTarget, ClosingClaws, TargetGrabbed, GoingToEndPoint, OpeningClaws, Done };
	HandStatus current_hand_status;

	private float curve_progress = 0.01f;
	private bool target_pos_updated = false;
	private bool target_grabbed = false;
	private bool obstruction_moved_last_step = false;
	private bool scene_updated_last_step = false;
	private Vector3 start_position;
	private Vector3 intermediate_target_position;
	private Vector3 target_position;
	private List<Vector3> path_list;
	private Stack<Vector3> path_stack;
	private Stack<Transform> transform_record;
	private Transform best_grip_transform;
	private Quaternion start_rotation;
	private Quaternion claw_start_rot;
	private Quaternion claw_start_rot2;

	private void Start() {
    targets = GameObject.FindGameObjectsWithTag("Target");
    FindBestTarget();
		bez_curve = GameObject.Find("Bez_curve").GetComponent<BezierCurve>();
		obstructions = GameObject.FindGameObjectsWithTag("Obstruction");
		old_obstruction_positions = new Vector3[obstructions.Length];
		UpdateTargetPosition(target_game_object.transform.position);

		start_rotation = transform.rotation;
		//claw_start_rot = claw_1.transform.parent.localRotation;
		//claw_start_rot2 = claw_2.transform.parent.localRotation;

		var childrenWithColliders = GetComponentsInChildren<Collider>(transform.gameObject);

		foreach (Collider child in childrenWithColliders) {
			ChildSensor hand_part = child.gameObject.AddComponent<ChildSensor>();
			hand_part.CollisionDelegate = OnCollisionChild;
			hand_part.TriggerDelegate = OnTriggerChild;
		}

		current_hand_status = HandStatus.GoingToTarget;
		transform_record = new Stack<Transform>();
	}

  private void FindBestTarget() {
    float best_y = - 100;
    GameObject best_target = null;
    foreach (GameObject target in targets) {
      if (target.transform.position.y > best_y) {
        best_y = target.transform.position.y;
        best_target = target;
      }
    }
    if (best_target == null) {
      print("defuq");
    } else {
      print("Best target = " + best_target.name);
    }

    target_game_object = best_target;
  }

	public void UpdateTargetPosition(Vector3 new_target_position) {
		start_position = this.transform.position;
		target_position = new_target_position;
		UpdatePath(start_position, target_position);
		bez_curve.AddPointAt(new_target_position);
		curve_progress = 0;
	}

	private void SetHandlePositionLinear(BezierCurve bc) {
		Vector3 handle_point;
		for (int i = 0; i < bc.pointCount - 1; i++) {
			handle_point = Vector3.Lerp(bc[i].position, bc[i + 1].position, 0.5f);
			bc[i].globalHandle2 = handle_point;
		}
	}

	private void SetHandlePositionCurve(BezierCurve bc) {
		Vector3 handle_point;
	  float curv_pos_percent = 0.5f;
		for (int j = 0; j < point_iterations; j++) {
			for (int i = 0; i < bc.pointCount - 1; i++) {
				handle_point = BezierCurve.GetPoint(bc[i], bc[i + 1], curv_pos_percent);
				bc[i].globalHandle2 = handle_point;
				curv_pos_percent += curv_pos_scaling;
			}
		}
	}

	private void UpdatePath(Vector3 new_start_position, Vector3 new_target_position) {
    FindBestTarget();
    path_list = AStar.FindPath(new_start_position, new_target_position, search_boundary, grid_granularity,agent_size,near_stopping_distance);
		if (path_list == null) {
			path_list = new List<Vector3>();
			Debug.Log("!!!!!!!!!!Did not find path!!!!!!!!!!!!!");
			best_grip_transform = target_game_object.GetComponent<GrabCheck>().GetUpTransform();
			path_list.Add(best_grip_transform.position);
		}
		path_list = AStar.SimplifyPath(path_list);
		path_list.Reverse();
		path_stack = new Stack<Vector3>(path_list);
		path_list.Reverse();//Reverse again after stack construction
		intermediate_target_position = PopPointFromList();

		bez_curve.ClearPoints();
		for (int i = 0; i < path_list.Count; i++) {
			bez_curve.AddPointAt(path_list[i]);
		}
		SetHandlePositionLinear(bez_curve);
		SetHandlePositionCurve(bez_curve);
	}

	private void DrawPath(Vector3 current_point_in_world_space, List<Vector3> _path, Color color) {
		Vector3 current_point = start_position;
		Gizmos.color = Color.black;
		foreach (Vector3 intermediate_goal in _path) {
			if (debug) {
				Debug.DrawRay(intermediate_goal, Vector3.forward, color, 1);
			}
			current_point = intermediate_goal;
		}
		if (debug) {
			Debug.DrawRay(target_position, Vector3.forward, color, 1);
		}
	}

	private void Update() {
		switch (current_hand_status) {
			case HandStatus.GoingToTarget:
				DidSceneUpdate();
				GetGrabPoint();
				FollowPathNoRot();
				break;
			case HandStatus.ApproachingTarget:				
				ApproachTarget(best_grip_transform);
				break;
			case HandStatus.PickingUpTarget:
				PickUpTarget();
				break;
			case HandStatus.ClosingClaws:
				CloseClaws();
				break;
			case HandStatus.TargetGrabbed:
				UpdateTargetPosition(escape_position.position);
        target_pos_updated = false;
				current_hand_status = HandStatus.GoingToEndPoint;
				break;
			case HandStatus.GoingToEndPoint:
				DidSceneUpdate();
				FollowPathNoRot();
				break;
			case HandStatus.OpeningClaws:
				OpenClaws();
				break;
			case HandStatus.Done:
				//if (fish_index < fish_to_grab.Length) {
				//	target_game_object = fish_to_grab[fish_index++].gameObject;
				//	claw_1.transform.parent.localRotation = claw_start_rot;
				//	claw_2.transform.parent.localRotation = claw_start_rot2;
				//	target_pos_updated = false;
				//	current_hand_status = HandStatus.GoingToTarget;
				//}
				break;
			default:
				Debug.Log("Not doing anything");
				break;
		}
		RecordTransform(this.transform);

		if (debug) {
      print("Current Status: " + current_hand_status);
      DrawPath(this.transform.position, path_list, Color.red);
		}
	}

	private bool DidSceneUpdate() {
		bool obstructions_are_moving = CheckIfObstructionsAreMoving();
		bool target_is_moving = target_game_object.GetComponent<GrabCheck>().GetIsMoving();
		if (!target_is_moving && !obstructions_are_moving && scene_updated_last_step && current_hand_status != HandStatus.TargetGrabbed) {
			var best_grip_position = target_game_object.GetComponent<GrabCheck>().CalculateBestGrabPoint().position;
      
      if (Vector3.Distance(best_grip_position, hand_base.transform.position) > 0.1f) {
				if (debug) {
					print("Updating path");
				}
        
        UpdateTargetPosition(best_grip_position);
			}
		} else if (!obstructions_are_moving && obstruction_moved_last_step && current_hand_status == HandStatus.GoingToEndPoint) {
			UpdateTargetPosition(escape_position.position);
		}

		scene_updated_last_step = false;
		if (obstructions_are_moving || target_is_moving) {
			scene_updated_last_step = true;
		}
		obstruction_moved_last_step = obstructions_are_moving;

		return true;
	}

	private bool CheckIfObstructionsAreMoving() {
		bool moving = false;
		var j = 0;
		var obstruction_positions = new Vector3[obstructions.Length];
		foreach (GameObject go in obstructions) {
			obstruction_positions[j++] = go.transform.position;
		}


		for (int i = 0; i < obstruction_positions.Length; i++) {
			if (obstruction_positions[i] != old_obstruction_positions[i]) {
				moving = true;
				old_obstruction_positions[i] = obstruction_positions[i];
			}
		}
		return moving;
	}

	private void GetGrabPoint() {
		if (Vector3.Distance(transform.position, path_list[path_list.Count - 1]) < .1f) {
			current_hand_status = HandStatus.ApproachingTarget;
			best_grip_transform = target_game_object.GetComponent<GrabCheck>().CalculateBestGrabPoint();
		}
	}

	private void PickUpTarget() {
    Vector3 point = best_grip_transform.Find("Point").position;
		//if (Vector3.Distance(best_grip_transform.position, hand_base.transform.position) > 0.01f) {
		if (Vector3.Distance(point, hand_base.transform.position) > 0.01f) {
        TranslateToTransform(best_grip_transform);
		} else {
			current_hand_status = HandStatus.ClosingClaws;
		}
	}
	private void CloseClaws() {

    //=============Old hand================
    //Transform pivot_1 = claw_1.transform.parent.transform;
    //Transform pivot_2 = claw_2.transform.parent.transform;

    //pivot_1.Rotate(Vector3.right * Time.fixedDeltaTime * claw_rot_speed);
    //pivot_2.Rotate(Vector3.left * Time.fixedDeltaTime * claw_rot_speed);

    Vector3 sliderPos = slider.transform.localPosition;
    if (sliderPos.y <= 0.16f) {
      sliderPos.y += .01f * Time.deltaTime;
      slider.transform.localPosition = sliderPos;

    } else if (sliderPos.y >= 0.16f) {
      current_hand_status = HandStatus.TargetGrabbed;
    } else {
      print("Mission failed, we'll get them next time.");
    }
  }

	private void OpenClaws() {
    //=============Old hand================
    //  target_game_object.transform.parent = null;
    //if (Quaternion.Angle(claw_1.transform.rotation, claw_start_rot) > 0.1f) {
    //	Transform pivot_1 = claw_1.transform.parent.transform;
    //	Transform pivot_2 = claw_2.transform.parent.transform;

    //	pivot_1.Rotate(Vector3.left * Time.fixedDeltaTime * claw_rot_speed);
    //	pivot_2.Rotate(Vector3.right * Time.fixedDeltaTime * claw_rot_speed);
    //}

    target_game_object.transform.parent = null;
    target_game_object.GetComponent<Rigidbody>().useGravity = true;
    //target_game_object.GetComponent<Rigidbody>().AddForce(0, 5, 0);
    Vector3 sliderPos = slider.transform.localPosition;
    if ((sliderPos.y >= 0.02f) || !target_grabbed) {
      sliderPos.y -= .01f * Time.deltaTime;
      slider.transform.localPosition = sliderPos;
    }
  }

	private void OnTriggerStay(Collider other) {
		if (other.transform.root.name == target_game_object.name) {
      target_grabbed = true;
			//other.attachedRigidbody.useGravity = false;
		}
	}

	private void OnTriggerExit(Collider other) {
		if (other.transform.root.name == target_game_object.name) {
      if (current_hand_status == HandStatus.OpeningClaws) {
        current_hand_status = HandStatus.Done;
      }
      target_grabbed = false;
		}
	}

	private void OnTriggerChild(GameObject child_game_object, Collider col) {
		if (child_game_object.name == "SecureChamber" && col.transform.root.name == target_game_object.name) {
			if (debug) {
				Debug.Log("Secured");
			}
			col.transform.root.parent = transform;
      col.attachedRigidbody.useGravity = false;
      current_hand_status = HandStatus.TargetGrabbed;
		}
	}

	private void OnCollisionChild(GameObject child_game_object, Collision col) {
		//print("OnCollisionChild: " + col.gameObject.tag);
		if (col.gameObject.tag == "Obstruction" && (current_hand_status == HandStatus.PickingUpTarget)) {
			current_hand_status = HandStatus.ClosingClaws;
		}
	}

	private Vector3 PopPointFromList() {
		if (path_stack.Count > 0) {
			return path_stack.Pop();
		}
		return target_position;
	}

	private void ApproachTarget(Transform target_transform) {
		float step = speed * Time.deltaTime * 0.5f;
		float turn_step = turn_speed * Time.deltaTime;
		transform.rotation = Quaternion.Slerp(transform.rotation, target_transform.rotation, turn_step);
		Vector3 approach = target_transform.position - (target_transform.forward * approach_distance);
		transform.position = Vector3.Lerp(transform.position, approach, step);
		if (Vector3.Distance(transform.position, approach) < 0.01f) {
			current_hand_status = HandStatus.PickingUpTarget;
		}

		if (debug) {
			Debug.DrawLine(transform.position, target_transform.position, Color.red);
		}
	}

	private void TranslateToTransform(Transform target_transform) {
		float step = speed * Time.deltaTime * 0.2f;
		float turn_step = turn_speed * Time.deltaTime;
		this.transform.rotation = Quaternion.Slerp(this.transform.rotation, target_transform.rotation, turn_step);
		this.transform.position = Vector3.Lerp(this.transform.position, target_transform.position, step);
		if (debug) {
			Debug.DrawLine(this.transform.position, target_transform.position, Color.red);
		}
	}

	private void FollowPath() {
    if (debug) {
      Debug.Log("Following Path");
    }
				
		if ((Vector3.Distance(transform.position, target) <= 0.1) || target == Vector3.zero) {
			target = GetCurrentTargetPosition();
		}

		float step = speed * Time.deltaTime;
;
		this.transform.rotation = Quaternion.LookRotation(target - transform.position);
    this.transform.position += transform.forward * step;
	}

  private void FollowPathOld() {
    if (debug) {
      Debug.Log("Following Path");
    }

    if ((Vector3.Distance(transform.position, target) <= 0.1) || target == Vector3.zero) {
      target = GetCurrentTargetPosition();
    }

    float step = speed * Time.deltaTime;
    float turn_step = turn_speed * Time.deltaTime;

    var to_rotation = Quaternion.LookRotation(target - transform.position);
    //this.transform.rotation = Quaternion.Slerp(transform.rotation, to_rotation, turn_step);
    this.transform.rotation = to_rotation;
    float angle_diff = Vector3.Dot(transform.rotation.eulerAngles.normalized, to_rotation.eulerAngles.normalized);

    if (angle_diff > .7f) {
      this.transform.position += transform.forward * step;
    }
  }

	private void FollowPathNoRot() {
		if (debug) {
			Debug.Log("Following Path (no rot)");
		}

		if ((Vector3.Distance(transform.position, target) <= 0.1) || !target_pos_updated) {
			target = GetCurrentTargetPosition();
			target_pos_updated = true;
		}

		float step = speed * Time.fixedDeltaTime;
		//float rot_step = 0.1f * Time.fixedDeltaTime;

		//this.transform.rotation = Quaternion.Slerp(transform.rotation, start_rotation, rot_step);
		this.transform.position = Vector3.MoveTowards(transform.position, target, step);

		if (Vector3.Distance(this.transform.position, target_position) < 0.1f) {
			current_hand_status = HandStatus.OpeningClaws;
		}
	}

	private Vector3 GetCurrentTargetPosition() {
		Vector3 target_pos;
		curve_progress += progress_scaling;
		target_pos = bez_curve.GetPointAt(curve_progress);
		if (debug) {
			Debug.DrawRay(target_pos, Vector3.right, Color.black, 1);
		}
		return target_pos;
	}

	private void RecordTransform(Transform current_transform) {
		//transform_record.Push(current_transform);
	}
}
