using UnityEngine;
using System.Collections.Generic;

public class DistanceInput : MonoBehaviour {
  public BeatBasedClipPlayer player;
  public GameObject other;

  public float currentDistance;
  public float currentLevel;

  public string tag;
  public float minDistance;
  public float maxDistance;

  GenerativeClipTag playerLevel = null;

  void Start() {
    // TODO: this is wonky
    if (player) {
      playerLevel = player.inputLevels.Find(x => x.tag == tag);
    }
  }

  void Update() {
    if (other != null) {
      currentLevel = Mathf.InverseLerp(maxDistance, minDistance, Vector3.Distance(transform.position, other.transform.position));
      if (playerLevel != null) {
        playerLevel.minValue = currentLevel;
      }
    }
  }
}