﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TresonQuestGuard : MonoBehaviour
{
    private GameObject treasonQuestNpc;
    private TreasonQuest.QuestState questState;

    // Start is called before the first frame update
    void Start()
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

        treasonQuestNpc = GameObject.FindObjectOfType<TreasonQuest>().gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (GetComponent<CharacterStats>().isDead)
        {
            TreasonQuest treasonQuest = GameObject.FindObjectOfType<TreasonQuest>();
            treasonQuest.NumberofGuardsDead();
            treasonQuest.SpawnGuardAgain();
            this.enabled = false;
        }

        OnQuestState();
    }

    void OnQuestState()
    {
        TreasonQuest.QuestState state = treasonQuestNpc.GetComponent<TreasonQuest>().state;
        if (state == questState)
            return;

        switch (state)
        {
            case TreasonQuest.QuestState.AttackingNobleHouse:
                questState = TreasonQuest.QuestState.AttackingNobleHouse;
                GetComponent<CombatBase>().attackPoint = treasonQuestNpc.GetComponent<TreasonQuest>().CenterofTown;
                GetComponent<CombatBase>().EnableCombat();
                break;
            case TreasonQuest.QuestState.GuardBossFight:
                GetComponent<CombatBase>().enabled = false;
                GetComponent<NavMeshAgent>().isStopped = true;
                break;
        }
    }
}
