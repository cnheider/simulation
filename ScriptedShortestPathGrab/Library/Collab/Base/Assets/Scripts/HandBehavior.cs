using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandBehavior : MonoBehaviour {

  GameObject claw_1, claw_2;

  public ObjectWithGrabs target;
  private Transform target_transform;
  private Vector3 target_position;
  private Vector3 target_approach_point;
  private Vector3 target_direction;

  public float speed = 1f;
  public float approach_height = 5f;
  public float approach_epsilon = 0.5f;
  public float angle_epsilon = 10f;
  public float max_dist_to_trace = 5f;
  public float max_dist_to_draw = 5f;
  public float evasion_force = 1f;
  private bool grab_proceedure_begun = false;

  void Start() {
    this.transform.position = Utils.GetRandomVector();
    if (target != null) {
      UpdateTarget();
      Debug.Log("Has target");
  } else {
      target_transform = this.transform;
      target_position = this.transform.position;
      target_approach_point = this.transform.position;
    }
	}

  void UpdateTarget() {
    //Debug.Log("TargetUpdate");
    target_transform = target.transform;
    target_position = target.GetGripPoint().position;
    target_approach_point = target.GetApproachPoint();
  }

  void Update(){
    if (target != null) {
      UpdateTarget();
      Debug.DrawLine(this.transform.position, target_approach_point, Color.cyan);
    }

    //if (Vector3.Distance(this.transform.position, this.target_transform.position) > approach_height){

    if (Vector3.Distance(this.transform.position, target_approach_point) > approach_epsilon && !grab_proceedure_begun) {
      //Debug.Log(target_approach_point);
      checkForHit2(this.transform.position, this.transform.forward);
      customMoveTowards();
    } else {
      grab_proceedure_begun = true;
      GrabProcedure();
    }
  }

  void GrabProcedure() {
    float step = speed * Time.deltaTime;

    target_direction = ((target_position - this.transform.position).normalized * 0.4f + target_direction).normalized;

    Quaternion to_rotation = Quaternion.LookRotation(target_direction);
    this.transform.rotation = Quaternion.Slerp(this.transform.rotation, to_rotation, step);
    float angle_diff = Vector3.Dot(this.transform.forward.normalized, -target.GetApproachVector().normalized);
    //Debug.Log(angle_diff);
    if (angle_diff< angle_epsilon) {
      this.transform.position += this.transform.forward * step;
    }
  }

  RaycastHit checkForHit2(Vector3 from_pos, Vector3 in_direction_of)
  {
    RaycastHit ray_hit;
    if (Physics.SphereCast(from_pos, 2f, in_direction_of, out ray_hit, max_dist_to_trace)){
      if (this.transform != ray_hit.transform && target.transform != ray_hit.transform){
        Debug.DrawLine(this.transform.position, ray_hit.point, Color.red);
        target_direction += ray_hit.normal * evasion_force; // Multiply the normal of the surface that was hit, effectively pushing away from the surface
      }
    }    else    {
      Debug.DrawRay(from_pos, in_direction_of * max_dist_to_draw, Color.green);
    }
    return ray_hit;
  }

  void StupidChecking() {
    checkForHit(this.transform.position, this.transform.forward);

    Vector3 ray_left = this.transform.position;
    Vector3 ray_right = this.transform.position;

    ray_left.x += 2f;
    ray_left.y += 2f;
    checkForHit(ray_left, this.transform.forward);
    ray_right.x -= 2f;
    ray_right.y -= 2f;
    checkForHit(ray_right, this.transform.forward);

    Vector3 yaw_left_ray = this.transform.forward;
    Vector3 yaw_right_ray = this.transform.forward;
    Vector3 roll_left_ray = this.transform.forward;
    Vector3 roll_right_ray = this.transform.forward;
    Vector3 pitch_up_ray = this.transform.forward;
    Vector3 pitch_down_ray = this.transform.forward;

    yaw_left_ray.x -= 0.5f;
    yaw_left_ray.Normalize();
    checkForHit(this.transform.position, yaw_left_ray.normalized);

    yaw_right_ray.x += 0.5f;
    yaw_right_ray.Normalize();
    checkForHit(this.transform.position, yaw_right_ray.normalized);

    roll_left_ray.y -= 0.5f;
    roll_left_ray.Normalize();
    checkForHit(this.transform.position, roll_left_ray.normalized);

    roll_right_ray.y += 0.5f;
    roll_right_ray.Normalize();
    checkForHit(this.transform.position, roll_right_ray.normalized);

    pitch_down_ray.z -= 0.5f;
    pitch_down_ray.Normalize();
    checkForHit(this.transform.position, pitch_down_ray.normalized);

    pitch_up_ray.z += 0.5f;
    pitch_up_ray.Normalize();
    checkForHit(this.transform.position, pitch_up_ray.normalized);
  }

  RaycastHit checkForHit(Vector3 from_pos, Vector3 in_direction_of)  {
    RaycastHit ray_hit;
    if (Physics.Raycast(from_pos, in_direction_of, out ray_hit, max_dist_to_trace)){
      if (this.transform != ray_hit.transform && target.transform != ray_hit.transform){
        Debug.DrawLine(this.transform.position, ray_hit.point, Color.red);
        target_direction += ray_hit.normal * evasion_force; // Multiply the normal of the surface that was hit, effectively pushing away from the surface
      }
    }else{
      Debug.DrawRay(from_pos, in_direction_of * max_dist_to_draw, Color.green);
    }
    return ray_hit;
  }

  void customMoveTowards(){
    float step = speed * Time.deltaTime;

    target_direction = ((target_approach_point - this.transform.position).normalized *0.4f + target_direction).normalized;

    Quaternion to_rotation = Quaternion.LookRotation(target_direction);
    this.transform.rotation = Quaternion.Slerp(this.transform.rotation, to_rotation, step);

    this.transform.position += this.transform.forward * step;
  }

  void simpleMoveTowards(){
    float step = speed * Time.deltaTime;
    if (Vector3.Distance(transform.position, target_transform.position) > approach_height)
    {
      transform.position = Vector3.MoveTowards(transform.position, target_transform.position, step);
      transform.LookAt(target_transform);
    }
  }
}
