using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Footsteps : MonoBehaviour
{
    // Start is called before the first frame update
    private AudioClip[] clips;

    private AudioSource audioSource;
    private void step(){
        AudioClip clip= GetRandomClip();
        audioSource.PlayOneShot(clip);
    }
    private AudioClip GetRandomAudio(){
        return clips[UnityEngine.Random.Range(0,clips.Length)];
    }
}
