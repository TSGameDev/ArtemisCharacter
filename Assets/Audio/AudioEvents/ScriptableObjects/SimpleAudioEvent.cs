using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Simple Audio Event", menuName = "Assets/Audio/Simple Audio Event")]
public class SimpleAudioEvent : Audio
{
    [SerializeField] private AudioClip[] clips;
    [SerializeField] private float minPitch;
    [SerializeField] private float maxPitch;
    [SerializeField] private float minVolume;
    [SerializeField] private float maxVolume;

    public override void Play(AudioSource audioSource)
    {
        float newpitch = Random.Range(minPitch, maxPitch);
        float newvolume = Random.Range(minVolume, maxVolume);
        int newclip = Random.Range(0, clips.Length);

        audioSource.volume = newvolume;
        audioSource.pitch = newpitch;
        audioSource.clip = clips[newclip];

        if(audioSource.isPlaying == true) { audioSource.Stop(); }

        audioSource.Play();
    }
}
