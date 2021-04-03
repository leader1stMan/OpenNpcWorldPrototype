using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        int count = 15;
        RagdollSystem[] Ragdolls = GameObject.FindObjectsOfType(typeof(RagdollSystem)) as RagdollSystem[];
        foreach (RagdollSystem dolls in Ragdolls)
        {
            dolls.DollID = count;
            count = count + 1;
        }
    }

   
}
