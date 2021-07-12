using UnityEngine.Events;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.AI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using TMPro;
using System.IO;

public class DialogueManager : MonoBehaviour, IInteractWindow, IDestructible
{
    public Camera dialogueCamera;
    public GameObject[] ToDisable;
    private int index;
    public float _textSpeed = 0f;
    private string sentence;
    public bool _isdialogue = false;
    private FirstPersonAIO firstPersonAIO;
    private PlayerActions _playeractions;
    private CharacterStats stats;
    public bool displayingdialogue = false;
    private GameObject _dialogue;
    private Dialogue dialogueScript;
    public Sentence currentSentence;
    public Sentence defaultSentence;
    private MerchantInventory shop;
    NPC npc;

    private void Start() 
    {
        GameObject player = GameObject.FindWithTag("Player");
        firstPersonAIO = player.GetComponent<FirstPersonAIO>();
        _playeractions = player.GetComponent<PlayerActions>();
        stats = player.GetComponent<CharacterStats>();
        _dialogue = _playeractions.dialogue_gameobject;
        dialogueScript = _dialogue.GetComponent<Dialogue>();
        npc = GetComponent<NPC>();
        shop = GetComponent<MerchantInventory>();
    }

    public void say(GameObject caller) 
    {
        firstPersonAIO.playerCanMove = false;
        _isdialogue = true;
        dialogueScript._name.text = caller.name;
        firstPersonAIO.lockAndHideCursor = false;
        _playeractions.openedWindow = this;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        npc.GetComponent<NavMeshAgent>().isStopped = true;
        npc.GetComponentInChildren<Animator>().enabled = false;
        
        npc.enabled = false;

        if (defaultSentence != null)
            currentSentence = defaultSentence;
        DialogueSystem.instance.Attach(this);
        DisplayNextSentence();
    }


    public void EndDialogue()
    {
        stats.attackCooldown = 3f;
        DialogueSystem.instance.Detach();
        npc.GetComponentInChildren<Animator>().enabled = true;

        var combats = npc.GetComponents<CombatBase>();
        bool enable = true;
        foreach (var combat in combats)
        {
            if (combat.enabled)
                enable = false;
        }
        npc.enabled = enable;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _isdialogue = false;
        firstPersonAIO.playerCanMove = true;
        _playeractions.openedWindow = null;
        _playeractions._indialogue = false;
        _playeractions.isInteracting = false;
        _playeractions.dialogue_gameobject.SetActive(false);
    }

    public void OptionsActive()
    {
        dialogueScript.DialogueText.gameObject.SetActive(false);
        dialogueScript._name.gameObject.SetActive(false);

        var options = _dialogue.GetComponentsInChildren<Button>(true);

        index = 0;
        foreach (Button a in options)
        {
            if (index >= currentSentence.GetPaths())
                break;
            a.gameObject.SetActive(true);
            a.interactable = true;
            AddButtonListener(a, index);
            a.GetComponentInChildren<Text>().text = currentSentence.GetSentence(index).text;
            index++;
        }
    }
    public void DisplayNextSentence()
    {
        if (currentSentence.answer != null && currentSentence.answer.Length > 0)
        {
            sentence = currentSentence.answer;
            if (currentSentence.goal != null)
            {
                currentSentence.goal.completeSentence(currentSentence);
            }
            if (currentSentence.CallShop)
            {
                OpenShop();
            }
            Typee();
        }
        else
        {
            if (currentSentence.nextSentence != null)
            {
                if (currentSentence.goal != null)
                {
                    currentSentence.goal.completeSentence(currentSentence); 
                }
                if (currentSentence.CallShop)
                {
                    OpenShop();
                }
                currentSentence = currentSentence.nextSentence;

                DisplayNextSentence();
            }
        }
    }

    void Typee()
    {
        displayingdialogue = true;
        dialogueScript.DialogueText.text = "";
        foreach (char letter in sentence)
        {
            dialogueScript.DialogueText.text += letter;
        }
        displayingdialogue = false;

    }

    private void Choices(int index)
    {
        var options = _dialogue.GetComponentsInChildren<Button>(true);

        foreach (Button a in options)
        {
            a.onClick.RemoveAllListeners();
            a.gameObject.SetActive(false);
            a.interactable = false;
        }
        currentSentence = currentSentence.choices[index];
        dialogueScript.DialogueText.gameObject.SetActive(true);
        dialogueScript._name.gameObject.SetActive(true);
        displayingdialogue = false;
        DisplayNextSentence();
    }

    private void AddButtonListener(Button a, int index)
    {
        a.onClick.AddListener(() =>
        {
            Choices(index);
        }
        );
    }

    public void OnClose()
    {
        EndDialogue();
    }

    void OpenShop()
    {
        EndDialogue();
        shop.OnOpen();
        _playeractions.openedWindow = shop;
        _playeractions.isInteracting = true;
    }

    public void OnDestruction(GameObject destroyer)
    {
        EndDialogue();
    }
}
