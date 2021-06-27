﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class Rioteer : MonoBehaviour
{
    private GameObject treasonQuestNpc;
    private TreasonQuest.QuestState questState;
    private bool isSpeaking = false;

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

        if (treasonQuestNpc.GetComponent<TreasonQuest>().state == TreasonQuest.QuestState.WithGaunavin)
        {
            StartCoroutine(LookAtNpc());
        }
    }

    private void Update()
    {
        if (GetComponent<CharacterStats>().isDead)
        {
            TreasonQuest treasonQuest = GameObject.FindObjectOfType<TreasonQuest>();
            treasonQuest.NumberofRioteersDead();
            treasonQuest.SpawnRioteerAgain();
            this.enabled = false;
        }
        OnQuestState();
    }

    IEnumerator LookAtNpc()
    {
        treasonQuestNpc = GameObject.FindObjectOfType<TreasonQuest>().gameObject;
        Quaternion lookRotation;
        do
        {
            Vector3 direction = (treasonQuestNpc.transform.position - transform.position).normalized;
            lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime / (Quaternion.Angle(transform.rotation, lookRotation) / GetComponent<NavMeshAgent>().angularSpeed));
            yield return new WaitForEndOfFrame();
        } while (treasonQuestNpc.GetComponent<TreasonQuest>().state != TreasonQuest.QuestState.AttackNoble);

        CombatBase combat = GetComponent<CombatBase>().EnableCombat();
        combat.attackPoint = treasonQuestNpc.GetComponent<TreasonQuest>().nobleHouse;
    }

    void OnQuestState()
    {
        TreasonQuest.QuestState state = treasonQuestNpc.GetComponent<TreasonQuest>().state;
        if (state == questState)
            return;

        int random = Random.Range(0, 999);
        switch (state)
        {
            case TreasonQuest.QuestState.GuardBossFight:
                GetComponent<CombatBase>().enabled = false;
                if (GetComponent<NavMeshAgent>().enabled)
                    GetComponent<NavMeshAgent>().isStopped = true;

                GetComponentInChildren<AnimationController>().ChangeAnimation(AnimationController.IDLE, AnimatorLayers.ALL);

                if (random == 0)
                {
                    if (!isSpeaking)
                    {
                        isSpeaking = true;
                        StartCoroutine(Speak());
                    }
                }
                break;

            case TreasonQuest.QuestState.AttackRiot:
                CombatBase combat = GetComponent<CombatBase>().EnableCombat();
                combat.attackPoint = treasonQuestNpc.GetComponent<TreasonQuest>().nobleHouse;

                if (random == 0)
                {
                    if (!isSpeaking)
                    {
                        isSpeaking = true;
                        StartCoroutine(Speak());
                    }
                }
                break;

            case TreasonQuest.QuestState.ReturnToNoble:
                GetComponent<CombatBase>().enabled = false;
                StartCoroutine(GetComponent<NPC>().Run(FindObjectOfType<TreasonQuest>().CenterofTown.gameObject));
                break;
        }
    }

    IEnumerator Speak()
    {
        TMP_Text tMP_Text = GetComponentInChildren<TMP_Text>();
        tMP_Text.text = null; //Ui for showing text

        int random = Random.Range(0, 2);
        switch (random)
        {
            case 0:
                tMP_Text.text = "Hooray!";
                break;
            case 1:
                tMP_Text.text = "Kill the nobel!";
                break;
            case 2:
                tMP_Text.text = "Freedom!";
                break;
        }
        yield return new WaitForSeconds(4);
        
        isSpeaking = false;
        tMP_Text.text = GetComponentInChildren<NpcData>().NpcName + "\nThe " + GetComponentInChildren<NpcData>().Job.ToString().ToLower();
    }
}