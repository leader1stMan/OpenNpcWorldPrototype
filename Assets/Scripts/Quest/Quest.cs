using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quest : MonoBehaviour
{
    public string title;
    public string description;

    public List<Item> itemRewards;
    public int coinsReward;

    public QuestState state;

    public static Quest instance;

    private void Awake()
    {
        instance = this;
    }
}

public enum QuestState { Unknown, Ongoing, Complete};
