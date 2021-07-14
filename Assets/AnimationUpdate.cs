using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AnimationUpdate : MonoBehaviour
{
    AnimationController controller;
    NavMeshAgent agent;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<AnimationController>();
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void LateUpdate()
    {
        //Manage animations
        if (agent.velocity.magnitude == 0)
        {
            //Idle animation if npc isn't moving
            controller.ChangeAnimation(AnimationController.IDLE, AnimatorLayers.ALL);
        }
        else
        {
            if (agent.velocity.magnitude < 2.5f)
            {
                //Walk animation if npc is moving slow
                controller.ChangeAnimation(AnimationController.WALK, AnimatorLayers.ALL);
            }
            else
            {
                //Walk animation if npc is moving fast
                controller.ChangeAnimation(AnimationController.RUN, AnimatorLayers.ALL);
            }
        }
    }
}
