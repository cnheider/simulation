using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class ObstacleSpawner : MonoBehaviour {

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
  [Range(0.10f, 5.00f)]
  public float x_size = 1.4f;
  [Range(0.10f, 5.00f)]
  public float y_size = 1.2f;
  [Range(0.00f, 3.00f)]
  private float y_centerPoint = 1.4f;
  [Range(0.10f, 5.00f)]
  public float z_size = 1.4f;

  [Header("Show obstacle spawn grid?")]
  public bool visualize_grid = true;

  [Space]
  [Header("Random scaling of objects (0 = uniform scale)")]
  [Range(0.000f, 0.300f)]
  public float scaling_factor = 0.000f;

  GameObject[] _obstacles;
  GameObject _cube;
  GameObject _sphere;

  void Start () {
    Setup();
  }

  void TearDown() {
    if(_cube)
      DestroyImmediate(_cube);
    if(_sphere)
      DestroyImmediate(_sphere);
    if(_obstacles != null && _obstacles.Length > 0)
      RemoveObstacles();
  }

  void Setup() {
    TearDown();
    y_centerPoint = this.transform.position.y;
    _obstacles = new GameObject[number_of_cubes + number_of_spheres];
    _cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
    _cube.SetActive(false);
    _cube.AddComponent<Obstruction>();
    _sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    _sphere.SetActive(false);
    _sphere.AddComponent<Obstruction>();
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

  void Update() {
    if (visualize_grid)
      Utilities.DrawRect(x_size,y_size,z_size, this.transform.position, Color.red);
  }

  public void SpawnObstacles(float cubeNum = 1, float sphereNum = 1) {
    RemoveObstacles();
    List<GameObject> tempList = new List<GameObject>();
    if (spawn_cubes) {
      Vector3 spawn_pos;
      for (int i = 0; i < cubeNum; i++) {
        float temp = Random.Range(-scaling_factor, scaling_factor);
        //spawn_pos = new Vector3(Random.Range(x_min, x_max), Random.Range(y_min, y_max), Random.Range(z_min, z_max));
        spawn_pos = new Vector3(Random.Range(-x_size/2, x_size / 2), Random.Range(-y_size/2 + y_centerPoint, y_size / 2 + y_centerPoint), Random.Range(-z_size/2, z_size / 2));
        GameObject cube_clone = Instantiate(_cube, spawn_pos, Quaternion.identity);
        cube_clone.transform.localScale = new Vector3(sphere_size + temp, sphere_size + temp, sphere_size + temp);
        cube_clone.SetActive(true);
        cube_clone.tag = "Obstruction";

        tempList.Add(cube_clone);
        /*if (Vector3.Distance(cube_clone.transform.position, GameObject.Find("EscapePos").transform.position) < 0.5f) {
          Destroy(cube_clone);
        }*/
      }
    }

    if (spawn_spheres) {
      Vector3 spawn_pos;
      for (int i = 0; i < sphereNum; i++) {
        float temp = Random.Range(-scaling_factor, scaling_factor);
        //spawn_pos = new Vector3(Random.Range(x_min, x_max), Random.Range(y_min, y_max), Random.Range(z_min, z_max));
        spawn_pos = new Vector3(Random.Range(-x_size/2, x_size / 2), Random.Range(-y_size/2 + y_centerPoint, y_size/2 + y_centerPoint), Random.Range(-z_size/2, z_size / 2));
        GameObject sphere_clone = Instantiate(_sphere, spawn_pos, Quaternion.identity);
        sphere_clone.transform.localScale = new Vector3(sphere_size + temp, sphere_size + temp, sphere_size + temp);
        sphere_clone.SetActive(true);
        sphere_clone.tag = "Obstruction";

        tempList.Add(sphere_clone);
        /*if (Vector3.Distance(sphere_clone.transform.position, GameObject.Find("EscapePos").transform.position) < 0.2f) {
          Destroy(sphere_clone);
        }*/
      }
    }
    tempList.CopyTo(_obstacles);
  }

  void RemoveObstacles() {
    foreach(GameObject obstacle in _obstacles) {
      DestroyImmediate(obstacle);
    }
  }

  private void OnDestroy() {
    TearDown();
  }

}
