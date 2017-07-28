using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinding : MonoBehaviour {

  public GameObject slider;
  public GameObject target_game_object;
	public GameObject hand_base;
	public Transform escape_transform;

	[Space(1)]
	[Header("Show debug logs and lines?")]
	public bool _debug = false;
  public bool showSearchBoundaries = true;

  [Space(1)]
  [Header("Path finding parameters")]
  public float search_boundary = 10f;
  public float near_stopping_distance = 1f;
  public float agent_size = 0.2f;
  public float grid_granularity = 0.3f;

  [Space(1)]
	[Header("Hand Controls")]
	public float speed = 1f;
	public float turn_speed = 4f;

	[Space(1)]
	[Header("Bezier Curve Controls")]
	public float approach_distance = 0.2f;
	public float curv_pos_scaling = -0.005f;
	public float progress_scaling = 0.01f;

	Vector3 target = Vector3.zero;
	Vector3[] old_obstruction_positions;
  GameObject[] targets;
	GameObject[] obstructions;
	BezierCurve bez_curve;

	public enum HandStatus { GoingToTarget, ApproachingTarget, PickingUpTarget, ClosingClaws, TargetGrabbed, GoingToEndPoint, OpeningClaws, Waiting, Reset, Done };
	public HandStatus current_hand_status, prev_hand_status;

	private float curve_progress = 0.01f;
  private bool isReset = false;
  private bool delivering = false;
	private bool target_pos_updated = false;
	private bool target_grabbed = false;
	private bool obstruction_moved_last_step = false;
	private bool scene_updated_last_step = false;
	private Vector3 start_position;
	public Vector3 target_position;
  private Vector3 reset_pos;
  private List<Vector3> path_list;
	private Transform best_grip_transform;

	private void Start() {
    escape_transform.position = transform.position;
    reset_pos = GameObject.Find("ResetPos").transform.position;
		bez_curve = GameObject.Find("Bez_curve").GetComponent<BezierCurve>();
		obstructions = GameObject.FindGameObjectsWithTag("Obstruction");
		old_obstruction_positions = new Vector3[obstructions.Length];
    UpdateTargetPosition(target_game_object.transform.position);
    FindBestTarget();

    var childrenWithColliders = GetComponentsInChildren<Collider>(transform.gameObject);

		foreach (Collider child in childrenWithColliders) {
			ChildSensor hand_part = child.gameObject.AddComponent<ChildSensor>();
			hand_part.CollisionDelegate = OnCollisionChild;
			hand_part.TriggerDelegate = OnTriggerChild;
		}

		current_hand_status = HandStatus.GoingToTarget;
	}

  private void showBounderies() {
    float x = transform.position.x;
    float y = transform.position.y;
    float z = transform.position.z;
    float dur = 0.05f;
    //Bottom lines
    Debug.DrawLine(new Vector3(-search_boundary + x, -search_boundary + y, -search_boundary + z), new Vector3(search_boundary + x, -search_boundary + y, -search_boundary + z), Color.magenta, dur);
    Debug.DrawLine(new Vector3(-search_boundary + x, -search_boundary + y, -search_boundary + z), new Vector3(-search_boundary + x, -search_boundary + y, search_boundary + z), Color.magenta, dur);
    Debug.DrawLine(new Vector3(search_boundary + x, -search_boundary + y, search_boundary + z), new Vector3(-search_boundary + x, -search_boundary + y, search_boundary + z), Color.magenta, dur);
    Debug.DrawLine(new Vector3(search_boundary + x, -search_boundary + y, search_boundary + z), new Vector3(search_boundary + x, -search_boundary + y, -search_boundary + z), Color.magenta, dur);
    //Vertical lines
    Debug.DrawLine(new Vector3(-search_boundary + x, search_boundary + y, -search_boundary + z), new Vector3(search_boundary + x, search_boundary + y, -search_boundary + z), Color.magenta, dur);
    Debug.DrawLine(new Vector3(-search_boundary + x, search_boundary + y, -search_boundary + z), new Vector3(-search_boundary + x, search_boundary + y, search_boundary + z), Color.magenta, dur);
    Debug.DrawLine(new Vector3(search_boundary + x, search_boundary + y, search_boundary + z), new Vector3(-search_boundary + x, search_boundary + y, search_boundary + z), Color.magenta, dur);
    Debug.DrawLine(new Vector3(search_boundary + x, search_boundary + y, search_boundary + z), new Vector3(search_boundary + x, search_boundary + y, -search_boundary + z), Color.magenta, dur);
    //Top lines
    Debug.DrawLine(new Vector3(-search_boundary + x, -search_boundary + y, -search_boundary + z), new Vector3(-search_boundary + x, search_boundary + y, -search_boundary + z), Color.magenta, dur);
    Debug.DrawLine(new Vector3(-search_boundary + x, -search_boundary + y, search_boundary + z), new Vector3(-search_boundary + x, search_boundary + y, search_boundary + z), Color.magenta, dur);
    Debug.DrawLine(new Vector3(search_boundary + x, -search_boundary + y, -search_boundary + z), new Vector3(search_boundary + x, search_boundary + y, -search_boundary + z), Color.magenta, dur);
    Debug.DrawLine(new Vector3(search_boundary + x, -search_boundary + y, search_boundary + z), new Vector3(search_boundary + x, search_boundary + y, search_boundary + z), Color.magenta, dur);
  }

  private void FindBestTarget() {
    targets = GameObject.FindGameObjectsWithTag("Target");
    //if (targets.Length == 0) {
    //  current_hand_status = HandStatus.Waiting;
    //}
    float best_y = -100;
    float highest_score = -100;
    GameObject best_target = null;
    foreach (GameObject target in targets) {

      //if (target.GetComponent<GrabCheck>().highest_score_pub > highest_score) {
      //  highest_score = target.GetComponent<GrabCheck>().highest_score_pub;
      //  best_target = target;
      //  print(target.name + "'s score is: " + highest_score);
      //}

      //Transform center = target.transform.Find("CenterPoint");
      Transform center = target.transform.FindDeepChild("CenterPoint");
      if (center.transform.position.y > best_y) {
        best_y = center.transform.position.y;
        best_target = target;
      }
    }
    if (best_target != null) {
      target_game_object = best_target;
    } else {
      target_game_object = best_target;
      current_hand_status = HandStatus.Waiting;
      print("Target = null");
    }
  }

  public void UpdateTargetPosition(Vector3 new_target_position) {
    start_position = transform.position;
		target_position = new_target_position;
		UpdatePath(start_position, target_position);
    CurvifyPath(new_target_position);
    curve_progress = 0;
	}

  private void SetHandlePosition(BezierCurve bc) {

    for (int i = 0; i < bc.pointCount; i++) {

      bc[i].handleStyle = BezierPoint.HandleStyle.Broken;

      if ((i != 0) && (i+1 != bc.pointCount)) {
        Vector3 currPoint = bc[i].position;
        Vector3 prevPoint = bc[i - 1].position;
        Vector3 nextPoint = bc[i + 1].position;
        Vector3 directionFrd = (nextPoint - prevPoint).normalized;
        Vector3 directionBck = (prevPoint - nextPoint).normalized;
        float handleScalar = 0.33f;
        float distPrev = Vector3.Distance(prevPoint, currPoint);
        float distNext = Vector3.Distance(currPoint, nextPoint);

        bc[i].globalHandle1 += (directionBck.normalized * distPrev * handleScalar);
        bc[i].globalHandle2 += (directionFrd.normalized * distNext * handleScalar);

        if (_debug) Debug.DrawLine(bc[i].globalHandle1, bc[i].globalHandle2, Color.blue, 5);
      }
    }
  }

  private void CurvifyPath(Vector3 targetPoint) {
    for (int i = 0; i < bez_curve.pointCount; i++) {
      Destroy(bez_curve[i].gameObject);
    }
    bez_curve.ClearPoints();
    for (int i = 0; i < path_list.Count; i++) {
      bez_curve.AddPointAt(path_list[i]);
    }
    bez_curve.AddPointAt(targetPoint);
    SetHandlePosition(bez_curve);
  }

	private void UpdatePath(Vector3 new_start_position, Vector3 new_target_position) {

    path_list = AStar.FindPath(new_start_position, new_target_position, search_boundary, grid_granularity,agent_size,near_stopping_distance);
		if (path_list == null) {
			path_list = new List<Vector3>();
			print("!!!!!!!!!!Did not find path!!!!!!!!!!!!!");
			best_grip_transform = target_game_object.GetComponent<GrabCheck>().GetUpTransform();
			path_list.Add(best_grip_transform.position);
		}
		path_list = AStar.SimplifyPath(path_list);
  }

	private void Update() {
    if (showSearchBoundaries)
      showBounderies();
    FindBestTarget();
    switch (current_hand_status) {
			case HandStatus.GoingToTarget:
        prev_hand_status = current_hand_status;
        if (DidSceneUpdate()) current_hand_status = HandStatus.Waiting;
				FollowPathNoRot();
        GetGrabPoint();
				break;

      case HandStatus.ApproachingTarget:
        prev_hand_status = current_hand_status;
        if (DidSceneUpdate()) current_hand_status = HandStatus.Waiting;
        ApproachTarget(best_grip_transform);
				break;

			case HandStatus.PickingUpTarget:
				PickUpTarget();
				break;

			case HandStatus.ClosingClaws:
				CloseClaws();
				break;

			case HandStatus.TargetGrabbed:
        target_pos_updated = false;
        UpdateTargetPosition(escape_transform.position);
        current_hand_status = HandStatus.GoingToEndPoint;
				break;

			case HandStatus.GoingToEndPoint:
        FollowPathNoRot();
        delivering = true;
				break;

			case HandStatus.OpeningClaws:
				OpenClaws();
				break;

			case HandStatus.Done:
        target_pos_updated = false;
        if (delivering) { //To make sure no targets gets untagged by mistake
          print("Untagging: " + target_game_object.name);
          target_game_object.tag = "Untagged";
          Destroy(target_game_object, 2);
          delivering = false;
        }
        FindBestTarget();
        ObstacleSpawn obs = GameObject.Find("OBSTACLE SPAWNER").GetComponent<ObstacleSpawn>();
        obs.SpawnObstacles(obs.number_of_cubes, obs.number_of_spheres);

        current_hand_status = HandStatus.GoingToTarget;
        break;

      case HandStatus.Waiting:
        if (target_game_object != null) {
          current_hand_status = DidSceneUpdate() ? HandStatus.Waiting : prev_hand_status;          
        }

        break;

      case HandStatus.Reset:
        ResetGripper();        
        break;

			default:
				print("Not doing anything (default case)");
				break;
		}

		if (_debug) {
      print("Current Status: " + current_hand_status);
		}
	}

  private void ResetGripper() {
    Vector3 sliderPos = slider.transform.localPosition;
    if (sliderPos.y >= 0.02f) {
      sliderPos.y -= 0.04f * Time.deltaTime;
      slider.transform.localPosition = sliderPos;
    } else {
      progress_scaling = 0.2f;
    }

    if (transform.position != reset_pos) {
      if (!isReset) {
        target_pos_updated = false;
        UpdateTargetPosition(reset_pos);
        isReset = true;
      }
    
      FollowPathNoRot();

    } else if ((transform.position == reset_pos) && (sliderPos.y <= 0.02f)) {
      progress_scaling = 0.01f;
      FindBestTarget();
      target_pos_updated = false;
      if (target_game_object != null)
        UpdateTargetPosition(target_game_object.transform.position);
      isReset = false;
      current_hand_status = HandStatus.GoingToTarget;
    }
  }

  private bool DidSceneUpdate() {
		bool obstructions_are_moving = CheckIfObstructionsAreMoving();
		bool target_is_moving = target_game_object.GetComponent<GrabCheck>().GetIsMoving();
		if (!target_is_moving && !obstructions_are_moving && scene_updated_last_step && current_hand_status != HandStatus.TargetGrabbed) {
			var best_grip_position = target_game_object.GetComponent<GrabCheck>().CalculateBestGrabPoint().position;
      
      if (Vector3.Distance(best_grip_position, hand_base.transform.position) > 0.1f && current_hand_status != HandStatus.GoingToEndPoint) {
				if (_debug) print("Updating path");
        
        UpdateTargetPosition(best_grip_position);
			}
		} else if (!obstructions_are_moving && obstruction_moved_last_step && current_hand_status == HandStatus.GoingToEndPoint) {
			UpdateTargetPosition(escape_transform.position);
		}

		scene_updated_last_step = false;
		if (obstructions_are_moving || target_is_moving) {
			scene_updated_last_step = true;
		}
		obstruction_moved_last_step = obstructions_are_moving;
    if (target_is_moving || obstructions_are_moving) {
      return true;
    } else {
      return false;
    }
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
    if (path_list != null) {
		  if (Vector3.Distance(transform.position, path_list[path_list.Count - 1]) < .1f) {
			  current_hand_status = HandStatus.ApproachingTarget;
			  best_grip_transform = target_game_object.GetComponent<GrabCheck>().CalculateBestGrabPoint();
		  }
    }
	}

	private void PickUpTarget() {
    Vector3 point = best_grip_transform.Find("Point").position;
		if (Vector3.Distance(point, hand_base.transform.position) > 0.02f) {
        TranslateToTransform(best_grip_transform);
		} else {
			current_hand_status = HandStatus.ClosingClaws;
		}
	}

	private void CloseClaws() {
    Vector3 sliderPos = slider.transform.localPosition;
    if (sliderPos.y <= 0.16f) {
      sliderPos.y += .025f * Time.deltaTime;
      slider.transform.localPosition = sliderPos;

    } else if (sliderPos.y >= 0.16f && !target_grabbed) {
      current_hand_status = HandStatus.Reset;
      print("Mission failed, we'll get them next time.");

    } else if (sliderPos.y >= 0.16f && target_grabbed) {
      current_hand_status = HandStatus.TargetGrabbed;
    }
  }

	private void OpenClaws() {
    if (target_game_object != null) {
      target_game_object.transform.parent = null;
      if (target_game_object.GetComponent<Rigidbody>() != null) {
        target_game_object.GetComponent<Rigidbody>().useGravity = true;
        target_game_object.GetComponent<Rigidbody>().angularDrag = 0;
      }
    }
    Vector3 sliderPos = slider.transform.localPosition;
    if (sliderPos.y > 0.02f) {
      sliderPos.y -= .025f * Time.deltaTime;
      slider.transform.localPosition = sliderPos;
    }
    if (current_hand_status != HandStatus.Reset) {
      if (!target_grabbed || (sliderPos.y <= 0.02f)) {
        current_hand_status = HandStatus.Done;

      }
    }
  }

  //Inside gripper grasp (Stay)
  private void OnTriggerStay(Collider other) {
		if (other.transform.root.name == target_game_object.name) {
      target_grabbed = true;
			//other.attachedRigidbody.useGravity = false;
		}
	}

  //Inside gripper grasp (Exit)
	private void OnTriggerExit(Collider other) {
		if (other.transform.root.name == target_game_object.name) {
      if (current_hand_status == HandStatus.OpeningClaws) {
        current_hand_status = HandStatus.Done;
      } else if (current_hand_status == HandStatus.GoingToEndPoint) {
        current_hand_status = HandStatus.Reset;
        print("Exit..");
      }
		}
    target_grabbed = false;
	}

  //Secure Chamber collider
	private void OnTriggerChild(GameObject child_game_object, Collider col) {
    if (child_game_object.name == "SecureChamber" && col.transform.root.name == target_game_object.name && current_hand_status != HandStatus.PickingUpTarget) {
			if (_debug) Debug.Log("Secured");
			col.transform.root.parent = transform;
      col.attachedRigidbody.useGravity = false;
      current_hand_status = HandStatus.TargetGrabbed;
    }
	}

  //Claw finger collision
	private void OnCollisionChild(GameObject child_game_object, Collision col) {
		if (col.gameObject.tag == "Obstruction" && (current_hand_status == HandStatus.PickingUpTarget)) {
			current_hand_status = HandStatus.ClosingClaws;
		}
	}

	private void ApproachTarget(Transform target_transform) {
		float step = speed * Time.deltaTime;
		float turn_step = turn_speed * Time.deltaTime;
		transform.rotation = Quaternion.Slerp(transform.rotation, target_transform.rotation, turn_step);
		Vector3 approach = target_transform.position - (target_transform.forward * approach_distance);
    transform.position = Vector3.MoveTowards(transform.position, approach, step);
		if (Vector3.Distance(transform.position, approach) < 0.01f) {
			current_hand_status = HandStatus.PickingUpTarget;
		}
		if (_debug) Debug.DrawLine(transform.position, target_transform.position, Color.red);
	}

	private void TranslateToTransform(Transform target_transform) {
		float step = speed * Time.deltaTime * 0.2f;
		float turn_step = turn_speed * Time.deltaTime;
		transform.rotation = Quaternion.Slerp(transform.rotation, target_transform.rotation, turn_step);
    transform.position = Vector3.MoveTowards(transform.position, target_transform.position, step);
		if (_debug) Debug.DrawLine(transform.position, target_transform.position, Color.red);
	}

	private void FollowPath() {
    if (_debug) Debug.Log("Following Path");
				
		if ((Vector3.Distance(transform.position, target) <= 0.1) || target == Vector3.zero) {
      target = GetCurrentTargetPosition();
		}

		float step = speed * Time.deltaTime;
;
		transform.rotation = Quaternion.LookRotation(target - transform.position);
    transform.position += transform.forward * step;
	}

	private void FollowPathNoRot() {
		if (_debug)	Debug.Log("Following Path (no rot)");

		if ((Vector3.Distance(transform.position, target) <= 0.1) || !target_pos_updated) {
      target = GetCurrentTargetPosition();
			target_pos_updated = true;
		}

		float step = speed * Time.deltaTime;

		transform.position = Vector3.MoveTowards(transform.position, target, step);
    
    if ((Vector3.Distance(transform.position, target_position) < 0.1f) && current_hand_status == HandStatus.GoingToEndPoint) {
			current_hand_status = HandStatus.OpeningClaws;
    } 
	}

	private Vector3 GetCurrentTargetPosition() {
		Vector3 target_pos_on_curve;
    if (curve_progress <= 1)
		  curve_progress += progress_scaling;

    target_pos_on_curve = bez_curve.GetPointAt(curve_progress);
    
		if (_debug) Debug.DrawRay(target_pos_on_curve, Vector3.right, Color.green, 1);

    return target_pos_on_curve;
	}



}


public static class TransformDeepChildExtension {
  //Breadth-first search
  public static Transform FindDeepChild(this Transform aParent, string aName) {
    var result = aParent.Find(aName);
    if (result != null)
      return result;
    foreach (Transform child in aParent) {
      result = child.FindDeepChild(aName);
      if (result != null)
        return result;
    }
    return null;
  }
}