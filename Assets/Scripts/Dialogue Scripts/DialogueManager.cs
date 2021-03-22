using UnityEngine.Events;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using TMPro;
using System.IO;

public class DialogueManager : MonoBehaviour, IInteractWindow
{
    public Camera dialogueCamera;
    public GameObject[] ToDisable;
    public UnityEvent OnStartDialogue;
    private int index;
    public float _textSpeed = 0f;
    private string sentence;
    public bool _isdialogue = false;
    private FirstPersonAIO player;
    private PlayerActions _playeractions;
    private PlayerCombat playerCombat;
    public bool displayingdialogue = false;
    private GameObject _dialogue;
    private Dialogue dialogueScript;
    public Sentence currentSentence;
    public Sentence defaultSentence;
    private MerchantInventory shop;
    NPC npc;
    private void Start() 
    {
        player = GameObject.FindWithTag("Player").GetComponent<FirstPersonAIO>();
        _playeractions = GameObject.FindWithTag("Player").GetComponent<PlayerActions>();
        _dialogue = _playeractions.dialogue_gameobject;
        dialogueScript = _dialogue.GetComponent<Dialogue>();
        playerCombat = GameObject.FindWithTag("Player").GetComponent<PlayerCombat>();
    }
    private void Awake()
    {
        if (OnStartDialogue == null)
            OnStartDialogue = new UnityEvent();
        npc = GetComponent<NPC>();
        shop = GetComponent<MerchantInventory>();
    }

    public void say(GameObject caller) 
    {
        player.playerCanMove = false;
        _isdialogue = true;
        dialogueScript._name.text = caller.name;
        player.lockAndHideCursor = false;
        _playeractions.openedWindow = this;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        npc.agent.isStopped = true;
        npc.GetComponentInChildren<Animator>().enabled = false;
        
        npc.enabled = false;

        if (defaultSentence != null)
            currentSentence = defaultSentence;
        DialogueSystem.instance.Attach(this);
        DisplayNextSentence();
        
        OnStartDialogue.Invoke();
    }


    public void EndDialogue()
    {
        playerCombat.attackCooldown = 3f;
        DialogueSystem.instance.Detach();
        npc.GetComponentInChildren<Animator>().enabled = true;
        npc.enabled = true;
        npc.agent.isStopped = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _isdialogue = false;
        player.playerCanMove = true;
        _playeractions.openedWindow = null;
        _playeractions._indialogue = false;
        _playeractions.isInteracting = false;
        _playeractions.dialogue_gameobject.SetActive(false);
    }


    private void OnDestroy()
    {
        OnStartDialogue.RemoveAllListeners();
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

    IEnumerator Type()
    {
        displayingdialogue = true;
        dialogueScript.DialogueText.text = "";
        foreach(char letter in sentence)
        {
            dialogueScript.DialogueText.text += letter;
            yield return new WaitForSeconds(_textSpeed*Time.deltaTime);
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
}
