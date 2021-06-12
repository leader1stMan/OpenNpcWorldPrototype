using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class OnDeathRagdoll : MonoBehaviour, IDestructible
{
    AnimationController controller;
    NavMeshAgent agent;
    Rigidbody[] rig;
    SkinnedMeshRenderer[] skin;

    void Start()
    {
        skin = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer skinned in skin)
        {
            skinned.updateWhenOffscreen = false; //has to be enabled when ragdoll is in. Otherwise the character sometimes does not render
        }

        rig = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rigidbody in rig)
        {
            if (rigidbody != this.GetComponent<Rigidbody>())
            {
                rigidbody.GetComponent<Collider>().enabled = false;
                rigidbody.isKinematic = true;
            }
        }
    }

    public void OnDestruction(GameObject destroyer)
    {
        //Activate ragdoll
        controller.enabled = false;
        agent.isStopped = true;
        GetComponent<CapsuleCollider>().enabled = false;
        GetComponent<Rigidbody>().isKinematic = false;

        foreach (Rigidbody rigidbody in rig)
        {
            if (rigidbody != this.GetComponent<Rigidbody>())
            {
                rigidbody.GetComponent<Collider>().enabled = true;
                rigidbody.isKinematic = false;
            }
        }

        foreach (SkinnedMeshRenderer skinned in skin)
        {
            skinned.updateWhenOffscreen = true; //Stops character from disrendering
        }

    }
}
