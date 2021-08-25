using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Audio : ScriptableObject
{
    public abstract void Play(AudioSource audioSource);
}
