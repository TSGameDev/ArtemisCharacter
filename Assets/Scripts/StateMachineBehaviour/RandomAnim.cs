using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomAnim : StateMachineBehaviour
{
    [Tooltip("The number of states that can be randomly selected.")]
    [SerializeField] private int numberOfStates;
    [Tooltip("The name of the int anim variable that is used as the condition for the transitions.")]
    [SerializeField] private string intStateCounterName;
    private int stateCounterHash;

    //OnStateMachineEnter is called when entering a state machine via its Entry Node
    override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    {
        stateCounterHash = Animator.StringToHash(intStateCounterName);
        int RanNum = Random.Range(0, numberOfStates);
        animator.SetInteger(stateCounterHash, RanNum);
    }
}
