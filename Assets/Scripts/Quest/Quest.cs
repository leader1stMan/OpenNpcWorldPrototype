using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="New Quest",menuName ="New Quest")]
public class Quest:ScriptableObject
{
    public bool isActive;

    public string title;
    public string description;
    public int reward;

    public QuestGoal goal;

    public void Complete()
    {
        isActive = false;
    }
}
