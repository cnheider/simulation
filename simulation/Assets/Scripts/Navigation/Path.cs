using System.Collections.Generic;
using UnityEngine;
using System;

namespace Assets.Scripts {

  [Serializable]
  class Path {

    public Vector3 _start_position;
    public Vector3 _target_position;
    private List<Vector3> _path_list;
    private float _progress = 0f;

    private BezierCurve _bezier_curve;

    public Path(Vector3 start_position, Vector3 target_position, BezierCurve game_object, List<Vector3> path_list) {
      _start_position = start_position;
      _target_position = target_position;
      _path_list = path_list;
      _bezier_curve = game_object;
      //CurvifyPath(_path_list[_path_list.Count - 1]);
      CurvifyPath();
    }

    private void CurvifyPath() {
      for (int i = 0; i < _bezier_curve.pointCount; i++) {
        GameObject.Destroy(_bezier_curve[i].gameObject);
      }
      _bezier_curve.ClearPoints();
      for (int i = 0; i < _path_list.Count; i++) {
        _bezier_curve.AddPointAt(_path_list[i]);
      }
      //_bezier_curve.AddPointAt(target_point);
      SetHandlePosition(_bezier_curve);
    }

    private void SetHandlePosition(BezierCurve bc) {

      for (int i = 0; i < bc.pointCount; i++) {

        bc[i].handleStyle = BezierPoint.HandleStyle.Broken;

        if ((i != 0) && (i + 1 != bc.pointCount)) {
          Vector3 currPoint = bc[i].position;
          Vector3 prevPoint = bc[i - 1].position;
          Vector3 nextPoint = bc[i + 1].position;
          Vector3 direction_forward = (nextPoint - prevPoint).normalized;
          Vector3 direction_back = (prevPoint - nextPoint).normalized;
          float handle_scalar = 0.33f;
          float distance_previous = Vector3.Distance(prevPoint, currPoint);
          float distance_next = Vector3.Distance(currPoint, nextPoint);

          bc[i].globalHandle1 += (direction_back.normalized * distance_previous * handle_scalar);
          bc[i].globalHandle2 += (direction_forward.normalized * distance_next * handle_scalar);

          //if (_debug) Debug.DrawLine(bc[i].globalHandle1, bc[i].globalHandle2, Color.blue, 5);
        }
      }
    }

    public Vector3 Next(float step_size) {
      _progress += step_size;
      return _bezier_curve.GetPointAt(_progress);
    }

  }
}
