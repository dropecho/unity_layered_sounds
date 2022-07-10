using UnityEngine;
using System.Collections.Generic;

public class BeatBasedClipPlayer : MonoBehaviour {
  public List<GenerativeClipTag> inputLevels;
  public int beat = 1;
  public float bpm = 100.0f;
  public List<GenerativeAudioClip> clips = new List<GenerativeAudioClip>();

  private double nextBeatTime;
  private double nextEventTime;
  private List<AudioSource> audioSources = new List<AudioSource>();


  void Start() {
    for (int i = 0; i < clips.Count; i++) {
      GameObject child = new GameObject(clips[i].name);
      child.transform.SetParent(gameObject.transform);
      var source = child.AddComponent<AudioSource>();
      source.clip = clips[i].clip;
      source.playOnAwake = false;
      source.loop = true;
      source.volume = 0;
      audioSources.Add(source);
    }

    nextEventTime = AudioSettings.dspTime + 1f;
    nextBeatTime = nextEventTime;
  }

  void Update() {
    OnUpdate();

    if (nextBeatTime <= AudioSettings.dspTime) {
      OnBeat(AudioSettings.dspTime - nextBeatTime);
    }
  }

  void OnUpdate() {
    for (var i = 0; i < clips.Count; i++) {

      var volume = GetVolumeForClip(clips[i]);

      if (audioSources[i].isPlaying == true) {
        if (volume > 0) {
          audioSources[i].volume = Mathf.Lerp(audioSources[i].volume, volume, Time.deltaTime * 2f);
        } else {
          audioSources[i].volume = Mathf.Lerp(audioSources[i].volume, 0, Time.deltaTime * 2f);
        }
      } else {
        audioSources[i].volume = 0;
      }
    }
  }

  void OnBeat(double timeBehind) {
    nextBeatTime += 60.0f / bpm;

    for (var i = 0; i < clips.Count; i++) {
      var shouldPlay = GetVolumeForClip(clips[i]) > 0;

      if (shouldPlay) {
        if (audioSources[i].isPlaying == false) {
          var clipBeat = (beat) % clips[i].beatLength;
          Debug.Log($"{audioSources[i]} coming in on beat {clipBeat + 1} with {timeBehind}ms @ {beat}");

          audioSources[i].time = (float)(((clipBeat) * (60f / bpm)) + timeBehind);
          audioSources[i].PlayScheduled(AudioSettings.dspTime);
        }
      } else if (audioSources[i].isPlaying && shouldPlay == false && audioSources[i].volume < 0.01f) {
        Debug.Log($"{audioSources[i]} stopping at beat {beat}");
        audioSources[i].Stop();
      }
    }
    beat++;
  }

  float GetVolumeForClip(GenerativeAudioClip clip) {
    foreach (var input in inputLevels) {
      foreach (var tag in clip.tags) {
        var tagsMatch = input.tag == tag.tag;
        var valueExceeds = input.minValue >= tag.minValue && input.minValue <= tag.maxValue;

        if (tagsMatch && valueExceeds) {
          return tag.valueMap.Evaluate(input.minValue);
        }
      }
    }

    return 0;
  }
}