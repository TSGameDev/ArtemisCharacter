using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepSFX : StateMachineBehaviour
{

    // OnStateMachineEnter is called when entering a state machine via its Entry Node
    override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    {
        animator.GetComponent<AudioHandler>().inAction = true;
    }

    // OnStateMachineExit is called when exiting a state machine via its Exit Node
    override public void OnStateMachineExit(Animator animator, int stateMachinePathHash)
    {
        animator.GetComponent<AudioHandler>().inAction = false;
    }
}
