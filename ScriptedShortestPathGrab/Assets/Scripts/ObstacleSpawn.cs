using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawn : MonoBehaviour {

  [Header("Cube")]
  public bool spawn_cubes = true;
  public float cube_size = 0.2f;
  [Range(1, 20)]
  public int number_of_cubes = 1;

  [Header("Sphere")]
  public bool spawn_spheres = true;
  public float sphere_size = 0.2f;
  [Range(1, 20)]
  public int number_of_spheres = 1;

  [Header("Spawn random number of objects?")]
  public bool random_obj_num = false;

  [Space]
  [Header("Bounderies")]
  public float x_max = 0.7f; 
  public float x_min = -0.7f;

  public float y_max = 2;
  public float y_min = 0.6f;

  public float z_max = 0.7f;
  public float z_min = -0.7f;

  [Space]
  [Header("Random scaling of objects (0 = uniform scale)")]
  [Range(0.000f, 0.300f)]
  public float scaling_factor = 0.000f;

  GameObject[] objects;
  GameObject cube;
  GameObject sphere;

  void Start () {
    objects = new GameObject[number_of_cubes + number_of_spheres];
    cube = GameObject.CreatePrimitive(PrimitiveType.Cube); cube.SetActive(false);
    sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere); sphere.SetActive(false);
    if (scaling_factor > 0.3f || scaling_factor < -0.3f) {
      scaling_factor = 0.3f;
    }

    if (!spawn_cubes && !spawn_spheres) {
      spawn_cubes = true;
    }

    if (random_obj_num) {
      SpawnObstacles(Random.Range(1, number_of_cubes), Random.Range(1, number_of_spheres));

    } else {
      SpawnObstacles(number_of_cubes, number_of_spheres);
    }
  }

  public void SpawnObstacles(float cubeNum = 1, float sphereNum = 1) {
    RemoveObstacles();
    List<GameObject> tempList = new List<GameObject>();
    if (spawn_cubes) {
      Vector3 spawn_pos;
      for (int i = 0; i < cubeNum; i++) {
        float temp = Random.Range(-scaling_factor, scaling_factor);
        spawn_pos = new Vector3(Random.Range(x_min, x_max), Random.Range(y_min, y_max), Random.Range(z_min, z_max));
        GameObject cube_clone = Instantiate(cube, spawn_pos, Quaternion.identity);
        cube_clone.transform.localScale = new Vector3(sphere_size + temp, sphere_size + temp, sphere_size + temp);
        cube_clone.SetActive(true);
        cube_clone.tag = "Obstruction";

        tempList.Add(cube_clone);
        if (Vector3.Distance(cube_clone.transform.position, GameObject.Find("EscapePos").transform.position) < 0.2f) {
          Destroy(cube_clone);
        }
      }
    }

    if (spawn_spheres) {
      Vector3 spawn_pos;
      for (int i = 0; i < sphereNum; i++) {
        float temp = Random.Range(-scaling_factor, scaling_factor);
        spawn_pos = new Vector3(Random.Range(x_min, x_max), Random.Range(y_min, y_max), Random.Range(z_min, z_max));
        GameObject sphere_clone = Instantiate(sphere, spawn_pos, Quaternion.identity);
        sphere_clone.transform.localScale = new Vector3(sphere_size + temp, sphere_size + temp, sphere_size + temp);
        sphere_clone.SetActive(true);
        sphere_clone.tag = "Obstruction";

        tempList.Add(sphere_clone);
        if (Vector3.Distance(sphere_clone.transform.position, GameObject.Find("EscapePos").transform.position) < 0.2f) {
          Destroy(sphere_clone);
        }
      }
    }
    tempList.CopyTo(objects);
  }

  void RemoveObstacles() {
    for (int i = 0; i < objects.Length; i++) {
      Destroy(objects[i]);
    }
  }

}
