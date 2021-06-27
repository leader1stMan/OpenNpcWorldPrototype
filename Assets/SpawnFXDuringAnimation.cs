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

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    //override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //
    //}

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        bloodFX = animator.gameObject.GetComponentInChildren<BloodFX>();

        if (bloodFX == null)
        {
            Debug.LogError("Add a BloodFX component to your object being animated");
            return;
        }
            else if (stateInfo.normalizedTime < minFrame || stateInfo.normalizedTime > maxFrame)
            {
                Debug.LogError("Failed to spawn blood");

                return;
            }


        else if (stateInfo.normalizedTime >= minFrame && stateInfo.normalizedTime <= maxFrame)
        {
            bloodFX.SpawnBlood(2000);
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

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
