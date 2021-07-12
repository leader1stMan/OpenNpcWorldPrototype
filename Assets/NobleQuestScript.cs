using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.IO;
using TMPro;

public class NobleQuestScript : MonoBehaviour, IDestructible
{
    public GameObject rioteerLeader;

    public void OnDestruction(GameObject destroyer)
    {
        StartCoroutine(rioteerLeader.GetComponent<TreasonQuest>().NobleIsExecuted());
    }
}
