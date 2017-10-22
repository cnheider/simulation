using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Priority_Queue;

public static class PathFinding {

  //private const int MAX_VECTORS_IN_QUEUE = 1000000; For FastPriorityQueue implementation this is value necessary

  public class FastVector3 : FastPriorityQueueNode {
    public Vector3 _vector { get; private set; }
    public FastVector3(Vector3 vector) {
      _vector = vector;
    }

    new public int QueueIndex { get; set; }


    public override int GetHashCode() {
      return _vector.GetHashCode();
    }

    public override bool Equals(object obj) {
      if (obj == null) {
        return false;
      }

      // If parameter cannot be cast to Point return false.
      FastVector3 p = obj as FastVector3;
      return Equals(p);
    }

    public bool Equals(FastVector3 obj) {
      return _vector == obj._vector;
    }
  }

  static float Heuristic(Vector3 source, Vector3 destination) {
    return Vector3.Distance(source, destination);
  }

  static List<FastVector3> GetUnobstructedNeighbouringNodes(Vector3 current_point, float search_boundary, float grid_granularity, float sphere_cast_radius) {

    var c = current_point;
    var g = grid_granularity; // Compressed names for better readability

    Vector3[] neighbours = new Vector3[26] {
      new Vector3 (c.x + g, c.y, c.z),
      new Vector3 (c.x + g, c.y + g, c.z),
      new Vector3 (c.x + g, c.y - g, c.z),
      new Vector3 (c.x + g, c.y, c.z + g),
      new Vector3 (c.x + g, c.y, c.z - g),
      new Vector3 (c.x + g, c.y + g, c.z + g),
      new Vector3 (c.x + g, c.y + g, c.z - g),
      new Vector3 (c.x + g, c.y - g, c.z + g),
      new Vector3 (c.x + g, c.y - g, c.z - g),

      new Vector3 (c.x - g, c.y, c.z),
      new Vector3 (c.x - g, c.y + g, c.z),
      new Vector3 (c.x - g, c.y - g, c.z),
      new Vector3 (c.x - g, c.y, c.z + g),
      new Vector3 (c.x - g, c.y, c.z - g),
      new Vector3 (c.x - g, c.y + g, c.z + g),
      new Vector3 (c.x - g, c.y + g, c.z - g),
      new Vector3 (c.x - g, c.y - g, c.z + g),
      new Vector3 (c.x - g, c.y - g, c.z - g),

      new Vector3 (c.x, c.y, c.z + g),
      new Vector3 (c.x, c.y, c.z - g),
      new Vector3 (c.x, c.y + g, c.z),
      new Vector3 (c.x, c.y - g, c.z),

      new Vector3 (c.x, c.y + g, c.z + g),
      new Vector3 (c.x, c.y + g, c.z - g),
      new Vector3 (c.x, c.y - g, c.z + g),
      new Vector3 (c.x, c.y - g, c.z - g)
    };

    List<FastVector3> returnSet = new List<FastVector3>();

    foreach (Vector3 neighbour in neighbours) {
      if (IsObstructed(neighbour, c, search_boundary, sphere_cast_radius)) continue; // do not add obstructed points to returned set
      returnSet.Add(new FastVector3(neighbour));
    }

    return returnSet;
  }

  static bool IsObstructed(Vector3 point, Vector3 current, float search_boundary, float sphere_cast_radius) {

    if (point.x <= -search_boundary
      || point.x >= search_boundary
      || point.y <= -search_boundary
      || point.y >= search_boundary
      || point.z <= -search_boundary
      || point.z >= search_boundary)
      return true;

    Ray ray = new Ray(current, (point - current).normalized);
    if (Physics.SphereCast(ray, sphere_cast_radius, Vector3.Distance(current, point), LayerMask.NameToLayer("Obstruction")))
      return true;

    return false;
  }

  public static List<Vector3> FindPathAstar(Vector3 source, Vector3 destination, float search_boundary = 20f, float grid_granularity = 1f, float agent_size = 1f, float near_stopping_distance = 3f) {

    HashSet<FastVector3> closed_set = new HashSet<FastVector3>();
    //FastPriorityQueue<FastVector3> frontier_set = new FastPriorityQueue<FastVector3>(MAX_VECTORS_IN_QUEUE);
    SimplePriorityQueue<FastVector3> frontier_set = new SimplePriorityQueue<FastVector3>();

    var fast_source = new FastVector3(source);
    Dictionary<FastVector3, FastVector3> predecessor = new Dictionary<FastVector3, FastVector3>();
    Dictionary<FastVector3, float> g_scores = new Dictionary<FastVector3, float>();

    g_scores.Add(fast_source, 0);
    frontier_set.Enqueue(fast_source, Heuristic(source, destination)); // Priority is distance, lowest distance highest priority
    while (frontier_set.Count > 0) {
      FastVector3 current_point = frontier_set.Dequeue();
      closed_set.Add(current_point);

      //Stopping condition and reconstruct path
      float distance_to_destination = Heuristic(current_point._vector, destination);
      if (distance_to_destination < near_stopping_distance) {
        FastVector3 current_trace_back_point = current_point;
        List<Vector3> path = new List<Vector3>() { current_trace_back_point._vector };
        while (predecessor.ContainsKey(current_trace_back_point)) {
          current_trace_back_point = predecessor[current_trace_back_point];
          path.Add(current_trace_back_point._vector);
        }
        path.Reverse();
        return path;
      }

      //Get neighboring points
      List<FastVector3> neighbours = GetUnobstructedNeighbouringNodes(current_point._vector, search_boundary, grid_granularity, agent_size);

      //Calculate scores and add to frontier
      foreach (FastVector3 neighbour in neighbours) {

        /*foreach(FastVector3 candidate_point in frontier_set) {  For FastPriorityQueue implementation this is check necessary
          if(neighbour == candidate_point) {
            neighbour.QueueIndex = candidate_point.QueueIndex;
          }
        }*/

        if (closed_set.Contains(neighbour)) {
          //Debug.Log("Neighbour already visited");
          continue; // Skip if neighbour is already in closed set'
        }

        float temp_g_score = g_scores[current_point] + Heuristic(current_point._vector, neighbour._vector);

        if (frontier_set.Contains(neighbour)) {
          if (temp_g_score > g_scores[neighbour]) {
            continue;  // Skip if neighbour g_score is already lower
          } else {
            g_scores[neighbour] = temp_g_score;
          }
        } else {
          g_scores.Add(neighbour, temp_g_score);
        }

        float f_score = g_scores[neighbour] + Heuristic(neighbour._vector, destination);

        if (frontier_set.Contains(neighbour)) {
          frontier_set.UpdatePriority(neighbour, f_score);
          predecessor[neighbour] = current_point;
        } else {
          /*if (frontier_set.Count > MAX_VECTORS_IN_QUEUE-1) { For FastPriorityQueue implementation this is check necessary
						return null;
					}*/
          frontier_set.Enqueue(neighbour, f_score);
          predecessor.Add(neighbour, current_point);
        }
      }
    }
    return null;
  }

  public static List<Vector3> SimplifyPath(List<Vector3> path, float sphere_cast_radius = 1f) {

    List<Vector3> smoothPath = new List<Vector3>();
    smoothPath.Add(path[0]);
    path.RemoveAt(0);
    path.Reverse(); // reverse to walk from last point

    while (path.Count > 0) {
      Vector3 last_point = smoothPath[smoothPath.Count - 1]; // will be drawing from last point in smoothed path
      Vector3 new_point = path[path.Count - 1]; // next unsmoothed path point is the last in reversed array
      foreach (Vector3 point in path) {
        Ray ray = new Ray(last_point, (point - last_point).normalized);
        if (Physics.SphereCast(ray, sphere_cast_radius, Vector3.Distance(point, last_point))) continue;
        new_point = point;
        break;
      }

      // new_point can still be unchanged here, so next point is the same as in unsmoothed path

      smoothPath.Add(new_point);
      int index_of_new_point = path.IndexOf(new_point);
      path.RemoveRange(index_of_new_point, path.Count - index_of_new_point); // kill everything after (including) found point
    }

    return smoothPath;

  }

}
