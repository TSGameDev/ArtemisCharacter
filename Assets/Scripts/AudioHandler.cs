using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioHandler : MonoBehaviour
{
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private AudioSource bowSource;

    [SerializeField] private SimpleAudioEvent stoneFootstepSFX;
    [SerializeField] private SimpleAudioEvent bowDrawnSFX;
    [SerializeField] private SimpleAudioEvent bowReleasedSFX;

    public bool inAction = false;

    public void FootStepSFX()
    {
        if(inAction) { return; }
        stoneFootstepSFX.Play(footstepSource);
    }

    public void FireSFX(int IsDrawn)
    {
        switch(IsDrawn)
        {
            case 1:
                bowDrawnSFX.Play(bowSource);
                break;
            case 2:
                bowReleasedSFX.Play(bowSource);
                break;
        }
    }
}
