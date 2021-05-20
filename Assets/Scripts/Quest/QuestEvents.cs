using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class QuestEvents : MonoBehaviour
{
    bool TreasonQuestFight = false;
    public TreasonQuest treasonQuest;
    
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay(Collider other)
    {

        if (other.CompareTag("TownSquare"))
        {
            if (treasonQuest.GetAgainstNobleEnding())
            {
                GameEvents.current.TreasonTriggerEnter();
            }
            if (treasonQuest.GetAgainstRioterEnding())
            {
                GameEvents.current.TreasonTriggerEnter();
            }
        }

        
    }
}
