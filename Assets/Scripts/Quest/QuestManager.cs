using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public List<Quest> quests;

    private void Awake()
    {
        quests = new List<Quest>()
        {
            TreasonQuest.instance
        };
    }
}
