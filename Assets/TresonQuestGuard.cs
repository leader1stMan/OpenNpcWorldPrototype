using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class TresonQuestGuard : MonoBehaviour
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

        int random = Random.Range(0, 999);
        switch (state)
        {
            case TreasonQuest.QuestState.AttackNoble:
                questState = TreasonQuest.QuestState.AttackNoble;
                GetComponent<CombatBase>().attackPoint = treasonQuestNpc.GetComponent<TreasonQuest>().CenterofTown;
                GetComponent<CombatBase>().EnableCombat();
                break;

            case TreasonQuest.QuestState.GuardBossFight:
                if (!GetComponent<CharacterStats>().isDead)
                {
                    GetComponent<CombatBase>().enabled = false;
                    if (GetComponent<NavMeshAgent>().enabled)
                        GetComponent<NavMeshAgent>().isStopped = true;

                    GetComponentInChildren<AnimationController>().ChangeAnimation(AnimationController.IDLE, AnimatorLayers.UP);
                    GetComponentInChildren<AnimationController>().ChangeAnimation(AnimationController.IDLE, AnimatorLayers.DOWN);

                    if (random == 0)
                    {
                        if (!isSpeaking)
                        {
                            isSpeaking = true;
                            StartCoroutine(Speak());
                        }
                    }
                }
                break;

            case TreasonQuest.QuestState.AttackRiot:
                questState = TreasonQuest.QuestState.AttackNoble;
                GetComponent<CombatBase>().attackPoint = treasonQuestNpc.GetComponent<TreasonQuest>().CenterofTown;
                GetComponent<CombatBase>().EnableCombat();
                break;

            case TreasonQuest.QuestState.ReturnToNoble:
                if (!GetComponent<CharacterStats>().isDead)
                {
                    GetComponent<CombatBase>().enabled = false;
                    if (GetComponent<NavMeshAgent>().enabled)
                        GetComponent<NavMeshAgent>().isStopped = true;

                    GetComponentInChildren<AnimationController>().ChangeAnimation(AnimationController.IDLE, AnimatorLayers.UP);
                    GetComponentInChildren<AnimationController>().ChangeAnimation(AnimationController.IDLE, AnimatorLayers.DOWN);

                    if (random == 0)
                    {
                        if (!isSpeaking)
                        {
                            isSpeaking = true;
                            StartCoroutine(Speak(1));
                        }
                    }
                }
                break;
        }
    }

    IEnumerator Speak(int state = 0)
    {
        TMP_Text tMP_Text = GetComponentInChildren<TMP_Text>();
        tMP_Text.text = null; //Ui for showing text

        int random = Random.Range(0, 2);
        switch (state)
        {
            case 0:
                switch (random)
                {
                    case 0:
                        tMP_Text.text = "Please have mercy!";
                        break;
                    case 1:
                        tMP_Text.text = "I didn't want to do this either!";
                        break;
                    case 2:
                        tMP_Text.text = "It is my wish that you treat me with honor.";
                        break;
                }
                break;
            case 1:
                switch (random)
                {
                    case 0:
                        tMP_Text.text = "Runaway you scumbags!";
                        break;
                    case 1:
                        tMP_Text.text = "The battle is lost!";
                        break;
                    case 2:
                        tMP_Text.text = "Freedom!";
                        break;
                }
                break;
        }
        yield return new WaitForSeconds(4);

        isSpeaking = false;
        tMP_Text.text = GetComponentInChildren<NpcData>().NpcName + "\nThe " + GetComponentInChildren<NpcData>().Job.ToString().ToLower();
    }
}
