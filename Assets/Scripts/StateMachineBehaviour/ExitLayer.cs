using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitLayer : StateMachineBehaviour
{
    [SerializeField] private int layerToChange;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetLayerWeight(layerToChange, 0);
    }
}
