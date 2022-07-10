using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[Serializable]
public class GenerativeClipTag : ISerializationCallbackReceiver {
  public string tag = "123";
  public float minValue;
  public float maxValue;
  public AnimationCurve valueMap;

  public void OnAfterDeserialize() { }

  public void OnBeforeSerialize() {
    if (valueMap?.length == 0) {
      valueMap = AnimationCurve.Linear(0, 0, 1, 1);
    }
  }
}

[CreateAssetMenu(fileName = "GenerativeAudioClip", menuName = "Dropecho/Layered Sounds/GenerativeAudioClip")]
public class GenerativeAudioClip : ScriptableObject {
  public AudioClip clip;
  public List<GenerativeClipTag> tags;
  public int beatLength;
}