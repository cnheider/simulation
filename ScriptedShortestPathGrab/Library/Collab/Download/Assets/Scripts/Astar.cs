using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Priority_Queue;

public static class AStar {

  static float Heuristic(Vector3 source, Vector3 destination) {
    return Vector3.Distance(source, destination);
  }

  static List<FastVector3> NeighbouringNodes(Vector3 current_point, float grid_granularity = 0.2f) {

    Vector3[] neighbours = new Vector3[26] {
      new Vector3 (current_point.x + grid_granularity, current_point.y, current_point.z),
      new Vector3 (current_point.x + grid_granularity, current_point.y + grid_granularity, current_point.z),
      new Vector3 (current_point.x + grid_granularity, current_point.y - grid_granularity, current_point.z),
      new Vector3 (current_point.x + grid_granularity, current_point.y, current_point.z + grid_granularity),
      new Vector3 (current_point.x + grid_granularity, current_point.y, current_point.z - grid_granularity),
      new Vector3 (current_point.x + grid_granularity, current_point.y + grid_granularity, current_point.z + grid_granularity),
      new Vector3 (current_point.x + grid_granularity, current_point.y + grid_granularity, current_point.z - grid_granularity),
      new Vector3 (current_point.x + grid_granularity, current_point.y - grid_granularity, current_point.z + grid_granularity),
      new Vector3 (current_point.x + grid_granularity, current_point.y - grid_granularity, current_point.z - grid_granularity),

      new Vector3 (current_point.x - grid_granularity, current_point.y, current_point.z),
      new Vector3 (current_point.x - grid_granularity, current_point.y + grid_granularity, current_point.z),
      new Vector3 (current_point.x - grid_granularity, current_point.y - grid_granularity, current_point.z),
      new Vector3 (current_point.x - grid_granularity, current_point.y, current_point.z + grid_granularity),
      new Vector3 (current_point.x - grid_granularity, current_point.y, current_point.z - grid_granularity),
      new Vector3 (current_point.x - grid_granularity, current_point.y + grid_granularity, current_point.z + grid_granularity),
      new Vector3 (current_point.x - grid_granularity, current_point.y + grid_granularity, current_point.z - grid_granularity),
      new Vector3 (current_point.x - grid_granularity, current_point.y - grid_granularity, current_point.z + grid_granularity),
      new Vector3 (current_point.x - grid_granularity, current_point.y - grid_granularity, current_point.z - grid_granularity),

      new Vector3 (current_point.x, current_point.y, current_point.z + grid_granularity),
      new Vector3 (current_point.x, current_point.y, current_point.z - grid_granularity),
      new Vector3 (current_point.x, current_point.y + grid_granularity, current_point.z),
      new Vector3 (current_point.x, current_point.y - grid_granularity, current_point.z),

      new Vector3 (current_point.x, current_point.y + grid_granularity, current_point.z + grid_granularity),
      new Vector3 (current_point.x, current_point.y + grid_granularity, current_point.z - grid_granularity),
      new Vector3 (current_point.x, current_point.y - grid_granularity, current_point.z + grid_granularity),
      new Vector3 (current_point.x, current_point.y - grid_granularity, current_point.z - grid_granularity)
    };

    List<FastVector3> returnSet = new List<FastVector3>();

    foreach (Vector3 neighbour in neighbours) {
      if (Obstructed(neighbour, current_point)) continue; // don't add obstructed points to returned set
        returnSet.Add(new FastVector3(neighbour));
    }

    return returnSet;
  }

  static bool Obstructed(Vector3 point, Vector3 current, float search_boundary_limit = 20, float sphere_cast_radius = 0.3f) {

		// check for game area borders or otherwise limit the area of path search,
		// this is important! can be a severe performance hit!

    if (point.x <= -search_boundary_limit
      || point.x >= search_boundary_limit
      || point.y <= -search_boundary_limit
      || point.y >= search_boundary_limit
      || point.z <= -search_boundary_limit
      || point.z >= search_boundary_limit)
      return true;

		Ray ray = new Ray(current, (point - current).normalized);
    //if (Physics.SphereCast(ray, sphere_cast_radius, Vector3.Distance(current, point)))//, LayerMask.NameToLayer("Obstruction")))
    if (Physics.SphereCast(ray, sphere_cast_radius, Vector3.Distance(current, point), LayerMask.NameToLayer("Obstruction")))
      return true;

		return false;
  }

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

  private const int MAX_VECTORS_IN_QUEUE = 1000000;

  public static List<Vector3> FindPath(Vector3 source, Vector3 destination, float near_stop_distance_limit = 1f) {

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
      if (distance_to_destination < near_stop_distance_limit) {
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
      List<FastVector3> neighbours = NeighbouringNodes(current_point._vector);

      //Calculate scores and add to frontier
      foreach (FastVector3 neighbour in neighbours) {

        /*foreach(FastVector3 candidate_point in frontier_set) {
          if(neighbour == candidate_point) {
            neighbour.QueueIndex = candidate_point.QueueIndex;
          }
        }*/

        if (closed_set.Contains(neighbour)) {
          //Debug.Log("Neighbour already visited");
          continue; // Skip if neighbour is already in closed set'
        }

        float temp_g_score = g_scores[current_point] + Heuristic(current_point._vector, neighbour._vector);

        if (frontier_set.Contains(neighbour)){
          if (temp_g_score > g_scores[neighbour]) {
            continue;  // Skip if neighbour g_score is already lower
          } else {
            g_scores[neighbour] = temp_g_score;
          }
        } else {
          //try {
            g_scores.Add(neighbour, temp_g_score);
          /*} catch {
            Debug.Log("catch");
            g_scores[neighbour] = temp_g_score;
          }*/
        }

        float f_score = g_scores[neighbour] + Heuristic(neighbour._vector, destination);

        if (frontier_set.Contains(neighbour)) {
          frontier_set.UpdatePriority(neighbour, f_score);
          predecessor[neighbour] = current_point;
        } else {
					//if (frontier_set.Count > MAX_VECTORS_IN_QUEUE-1) {
     //       Debug.Log("")
					//	return null;
					//}
          frontier_set.Enqueue(neighbour, f_score);
          //try {
            predecessor.Add(neighbour, current_point);
          /*} catch {
            predecessor[neighbour] = current_point;
          }*/
        }
      }
    }
    return null;
  }

  public static List<Vector3> SimplifyPath(List<Vector3> path, float sphere_cast_radius = 0.1f) {

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

      // nP can still be unchanged here,
      // so next point is the same as in unsmoothed path

      smoothPath.Add(new_point);
      int index_of_new_point = path.IndexOf(new_point);
      path.RemoveRange(index_of_new_point, path.Count - index_of_new_point); // kill everything after (including) found point
    }

    return smoothPath;

  }

}
