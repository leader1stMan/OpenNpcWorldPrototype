using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.IO;
using TMPro;

public class NobleQuestScript : MonoBehaviour
{
    public List<string> DialoguePaths;
    public string path = null;
    private TMP_Text text;
    public bool isFirst;

    public GameObject rioteerLeader;
    
    private bool nobleDead = false;

    private void Start()
    {
        Rigidbody[] rig = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rigidbody in rig)
        {
            if (rigidbody != this.GetComponent<Rigidbody>())
            {
                rigidbody.GetComponent<Collider>().enabled = false;
                rigidbody.isKinematic = true;
            }
        }
        //Skinnedmesh needs to updated off screen when ragdolled. Or disappears
        SkinnedMeshRenderer[] skins = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer skinnedMeshRenderer in skins)
        {
            skinnedMeshRenderer.updateWhenOffscreen = false;
        }
        GetComponent<CapsuleCollider>().enabled = true;

        text = GetComponentInChildren<TMP_Text>();
    }

    private void Update()
    {
        if (GetComponent<CharacterStats>().isDead && nobleDead == false)
        {
            nobleDead = true;
            StartCoroutine(rioteerLeader.GetComponent<TreasonQuest>().NobleIsExecuted());
        }
    }
}
