using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "AudioThing", menuName = "ScriptableObjects/Audio")]
public class AudioData : ScriptableObject {
    // Start is called before the first frame update
    
    public AudioClip[] audioClips;

    [Range(0, 1.5f)] public float volume = 1;
    [Range(0.3f, 3f)] public float pitch = 1;

    public void Play(AudioSource source) {
        if(audioClips.Length == 0) return;

        source.clip = audioClips[Random.Range(0, audioClips.Length)];
        source.volume = volume;
        source.pitch = pitch;
        source.Play();

    }

}
