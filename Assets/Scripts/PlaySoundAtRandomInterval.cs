using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySoundAtRandomInterval : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;
    void Start()
    {
        int randomStartTime = Random.Range(0, audioSource.clip.samples - 1);
        audioSource.timeSamples = randomStartTime;
        audioSource.Play();
    }

}
