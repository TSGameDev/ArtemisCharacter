using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartLayer : StateMachineBehaviour
{
    [SerializeField] private int layerToChange;

    // OnStateMachineEnter is called when entering a state machine via its Entry Node
    override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    {
        animator.SetLayerWeight(layerToChange, 1);
    }
}
