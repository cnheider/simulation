using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishSpawn : MonoBehaviour {

  public Transform spawn_location;
  public GameObject fish_object;
  public int spawn_count = 10;

  public void SpawnFish() {
    float y = spawn_location.position.y;
    Vector3 newPos = new Vector3(spawn_location.position.x, y, spawn_location.position.z);
    for (int i = 0; i < spawn_count; i++) {
      newPos.y = y;
      GameObject newFish = Instantiate(fish_object, newPos, spawn_location.rotation);
      newFish.name = newFish.name + " nr. " + i;
      y += 0.2f;
    }
  }

  private void Start() {
    SpawnFish();
  }
}
