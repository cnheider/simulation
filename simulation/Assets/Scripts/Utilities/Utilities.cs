using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts {
  public static class Utilities {
    public static void DrawBoxFromCenter(Vector3 p, float r, Color c) { // p is pos.yition of the center, r is "radius" and c is the color of the box
      //Bottom lines
      Debug.DrawLine(new Vector3(-r + p.x, -r + p.y, -r + p.z), new Vector3(r + p.x, -r + p.y, -r + p.z), c);
      Debug.DrawLine(new Vector3(-r + p.x, -r + p.y, -r + p.z), new Vector3(-r + p.x, -r + p.y, r + p.z), c);
      Debug.DrawLine(new Vector3(r + p.x, -r + p.y, r + p.z), new Vector3(-r + p.x, -r + p.y, r + p.z), c);
      Debug.DrawLine(new Vector3(r + p.x, -r + p.y, r + p.z), new Vector3(r + p.x, -r + p.y, -r + p.z), c);

      //Vertical lines
      Debug.DrawLine(new Vector3(-r + p.x, r + p.y, -r + p.z), new Vector3(r + p.x, r + p.y, -r + p.z), c);
      Debug.DrawLine(new Vector3(-r + p.x, r + p.y, -r + p.z), new Vector3(-r + p.x, r + p.y, r + p.z), c);
      Debug.DrawLine(new Vector3(r + p.x, r + p.y, r + p.z), new Vector3(-r + p.x, r + p.y, r + p.z), c);
      Debug.DrawLine(new Vector3(r + p.x, r + p.y, r + p.z), new Vector3(r + p.x, r + p.y, -r + p.z), c);

      //Top lines
      Debug.DrawLine(new Vector3(-r + p.x, -r + p.y, -r + p.z), new Vector3(-r + p.x, r + p.y, -r + p.z), c);
      Debug.DrawLine(new Vector3(-r + p.x, -r + p.y, r + p.z), new Vector3(-r + p.x, r + p.y, r + p.z), c);
      Debug.DrawLine(new Vector3(r + p.x, -r + p.y, -r + p.z), new Vector3(r + p.x, r + p.y, -r + p.z), c);
      Debug.DrawLine(new Vector3(r + p.x, -r + p.y, r + p.z), new Vector3(r + p.x, r + p.y, r + p.z), c);
    }

    public static void RegisterCollisionTriggerCallbacksOnChildren(Transform transform, ChildSensor.OnChildCollisionEnterDelegate OnCollisionEnterChild, ChildSensor.OnChildTriggerEnterDelegate OnTriggerEnterChild, ChildSensor.OnChildCollisionExitDelegate OnCollisionExitChild, ChildSensor.OnChildTriggerExitDelegate OnTriggerExitChild, bool debug = false) {
      var childrenWithColliders = transform.GetComponentsInChildren<Collider>(transform.gameObject);

      foreach (Collider child in childrenWithColliders) {
        ChildSensor child_sensor = child.gameObject.AddComponent<ChildSensor>();
        child_sensor.OnCollisionEnterDelegate = OnCollisionEnterChild;
        child_sensor.OnTriggerEnterDelegate = OnTriggerEnterChild;
        child_sensor.OnCollisionExitDelegate = OnCollisionExitChild;
        child_sensor.OnTriggerExitDelegate = OnTriggerExitChild;
        //Debug.Log(transform.name + " has " + child_sensor.name + " registered");
      }
    }

    public static void DrawRect(float x_size, float y_size, float z_size, Vector3 pos, Color color) {
      float x = x_size / 2;
      float y = y_size / 2;
      float z = z_size / 2;

      //Vertical lines
      Debug.DrawLine(new Vector3(-x + pos.x, -y + pos.y, -z + pos.z), new Vector3(-x + pos.x, y + pos.y, -z + pos.z), color);
      Debug.DrawLine(new Vector3(x + pos.x, -y + pos.y, -z + pos.z), new Vector3(x + pos.x, y + pos.y, -z + pos.z), color);
      Debug.DrawLine(new Vector3(-x + pos.x, -y + pos.y, z + pos.z), new Vector3(-x + pos.x, y + pos.y, z + pos.z), color);
      Debug.DrawLine(new Vector3(x + pos.x, -y + pos.y, z + pos.z), new Vector3(x + pos.x, y + pos.y, z + pos.z), color);

      //Horizontal top
      Debug.DrawLine(new Vector3(-x + pos.x, y + pos.y, -z + pos.z), new Vector3(x + pos.x, y + pos.y, -z + pos.z), color);
      Debug.DrawLine(new Vector3(-x + pos.x, y + pos.y, z + pos.z), new Vector3(x + pos.x, y + pos.y, z + pos.z), color);
      Debug.DrawLine(new Vector3(-x + pos.x, y + pos.y, -z + pos.z), new Vector3(-x + pos.x, y + pos.y, z + pos.z), color);
      Debug.DrawLine(new Vector3(x + pos.x, y + pos.y, -z + pos.z), new Vector3(x + pos.x, y + pos.y, z + pos.z), color);

      //Horizontal bottom
      Debug.DrawLine(new Vector3(-x + pos.x, -y + pos.y, -z + pos.z), new Vector3(x + pos.x, -y + pos.y, -z + pos.z), color);
      Debug.DrawLine(new Vector3(-x + pos.x, -y + pos.y, z + pos.z), new Vector3(x + pos.x, -y + pos.y, z + pos.z), color);
      Debug.DrawLine(new Vector3(-x + pos.x, -y + pos.y, -z + pos.z), new Vector3(-x + pos.x, -y + pos.y, z + pos.z), color);
      Debug.DrawLine(new Vector3(x + pos.x, -y + pos.y, -z + pos.z), new Vector3(x + pos.x, -y + pos.y, z + pos.z), color);

    }


    public static bool DidTransformsChange(Transform[] old_transforms, Transform[] newly_acquired_transforms) {
      if (old_transforms.Length != newly_acquired_transforms.Length) {
        return true;
      }

      var i = 0;
      foreach (Transform old in old_transforms) {
        if (old.position != newly_acquired_transforms[i].position || old.rotation != newly_acquired_transforms[i].rotation)
          return true;
        i++;
      }
      return false;
    }

    public static Bounds GetTotalMeshFilterBounds(Transform objectTransform) {
      var meshFilter = objectTransform.GetComponent<MeshFilter>();

      var result = meshFilter != null ? meshFilter.mesh.bounds : new Bounds();

      foreach (Transform transform in objectTransform) {
        var bounds = GetTotalMeshFilterBounds(transform);
        result.Encapsulate(bounds.min);
        result.Encapsulate(bounds.max);
      }

      /*var bounds1 = GetTotalColliderBounds(objectTransform);
      result.Encapsulate(bounds1.min);
      result.Encapsulate(bounds1.max);
      */
/*
      foreach (Transform transform in objectTransform) {
        var bounds = GetTotalColliderBounds(transform);
        result.Encapsulate(bounds.min);
        result.Encapsulate(bounds.max);
      }
      */
      var scaledMin = result.min;
      scaledMin.Scale(objectTransform.localScale);
      result.min = scaledMin;
      var scaledMax = result.max;
      scaledMax.Scale(objectTransform.localScale);
      result.max = scaledMax;
      return result;
    }

    public static Bounds GetTotalColliderBounds(Transform objectTransform) {
      var meshFilter = objectTransform.GetComponent<Collider>();

      var result = meshFilter != null ? meshFilter.bounds : new Bounds();

      foreach (Transform transform in objectTransform) {
        var bounds = GetTotalColliderBounds(transform);
        result.Encapsulate(bounds.min);
        result.Encapsulate(bounds.max);
      }

      var scaledMin = result.min;
      scaledMin.Scale(objectTransform.localScale);
      result.min = scaledMin;
      var scaledMax = result.max;
      scaledMax.Scale(objectTransform.localScale);
      result.max = scaledMax;
      return result;
    }

    public static Bounds GetMaxBounds(GameObject g) {
      var b = new Bounds(g.transform.position, Vector3.zero);
      foreach (Renderer r in g.GetComponentsInChildren<Renderer>()) {
        b.Encapsulate(r.bounds);
      }
      return b;
    }

  }


public class Pair<T1, T2> {
    public T1 First { get; private set; }
    public T2 Second { get; private set; }
    internal Pair(T1 first, T2 second) {
      First = first;
      Second = second;
    }
  }

  public static class Pair {
    public static Pair<T1, T2> New<T1, T2>(T1 first, T2 second) {
      var tuple = new Pair<T1, T2>(first, second);
      return tuple;
    }
  }
}
