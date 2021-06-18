using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class DialogueSystem : MonoBehaviour
{
    public static DialogueSystem instance;

    public DialogueManager currentManager { get; private set; }

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    public void Attach(DialogueManager dialogueManager)
    {
        currentManager = dialogueManager;
        dialogueManager.OnStartDialogue.AddListener(DialogueListener);
        currentManager.dialogueCamera.enabled = true;
        foreach (GameObject g in currentManager.ToDisable)
            g.SetActive(false);
    }

    private void DialogueListener()
    {
        
    }

    public void Detach()
    {
        if (currentManager != null)
        {
            currentManager.OnStartDialogue.RemoveListener(DialogueListener);
            currentManager.dialogueCamera.enabled = false;
            foreach (GameObject g in currentManager.ToDisable)
                g.SetActive(true);
        }
        currentManager = null;
    }
}