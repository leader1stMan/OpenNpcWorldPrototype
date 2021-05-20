using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameEvents : MonoBehaviour
{
    public static GameEvents current;
    private void Awake()
    {
        current = this;
    }

    public event Action onNpcTurnAgressive;

    public void NpcTurnAgressive()
    {
        if (onNpcTurnAgressive != null)
        {
            onNpcTurnAgressive();
        }
    }

    public event Action onTreasonQuestNpcKill;

    public void TreasonQuestNpcKill()
    {
        if(onTreasonQuestNpcKill != null)
        {
            onTreasonQuestNpcKill();
        }
    }

    public event Action onTreasonTriggerEnter;
    public void TreasonTriggerEnter()
    {
        if (onTreasonTriggerEnter != null)
        {
            onTreasonTriggerEnter();
        }
    }
}
