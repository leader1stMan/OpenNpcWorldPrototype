using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnFXDuringAnimation : StateMachineBehaviour
{
    public BloodFX bloodFX;

    //minFrame is the minimum frame for hit registry divided by the total number of animation frames
    //maxFrame is the maximum frame for hit registry divided by the total number of animation frames     

    //both set in the inspector
    public float minFrame, maxFrame;

    private bool spawned = true;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    //override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //
    //}

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        bloodFX = animator.gameObject.GetComponentInChildren<BloodFX>();
        if (spawned)
        {
            bloodFX.SpawnBlood(2000);
            spawned = false;
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        spawned = true;
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
