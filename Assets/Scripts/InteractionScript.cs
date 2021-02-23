using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.IO;
using TMPro;

public class InteractionScript : MonoBehaviour
{
    public int priority;
    public bool isTalking;
    public bool isFirst;
    public int Dialogue;
    private bool isStarted;
    private int called;

    private TMP_Text text;

    void Start()
    {
        text = GetComponentInChildren<TMP_Text>();
    }

    void Update()
    {
        if (isTalking)
        {
            if (!isStarted)
            {
                isStarted = true;
                StartCoroutine("Conversation");
            }
        }
        else
        {
            isStarted = false;
            StopCoroutine("Conversation");
        }
    }

    IEnumerator Conversation()
    {
        string path;

        switch (Random.Range(1, 2))
        {
            case 1:
                if (isFirst)
                {
                    path = "Assets/NPC dialogues/Dialogue 1a.txt";
                }
                else
                {
                    path = "Assets/NPC dialogues/Dialogue 1b.txt";
                }
                break;
            case 2:
                if (isFirst)
                {
                    path = "Assets/NPC dialogues/Dialogue 2a.txt";
                }
                else
                {
                    path = "Assets/NPC dialogues/Dialogue 2b.txt";
                }
                break;
            default:
                path = null;
                break;
        }

        if (!isFirst)
        {
            text.text = null;
            yield return new WaitForSeconds(4);
        }

        string line;
        StreamReader reader = new StreamReader(path);
        while ((line = reader.ReadLine()) != null)
        {
            text.text = line;
            yield return new WaitForSeconds(4);
            text.text = null;
            yield return new WaitForSeconds(4);
        }

        if (isFirst)
        {
            yield return new WaitForSeconds(4);
        }

        //Debug.Log("Conversation ended by" + gameObject.name);
        isFirst = false;
        isTalking = false;

        text.text = GetComponentInChildren<NpcData>().NpcName + "\nThe " + GetComponentInChildren<NpcData>().Job.ToString().ToLower();
        yield return null;
    }

    void OnTriggerStay(Collider other)
    {
        if (!isTalking)
        {
            if (other.CompareTag("Npc"))
            {
                GameObject obj = other.gameObject;
                InteractionScript script = obj.GetComponentInParent<InteractionScript>();
                if (!script.isTalking)
                {
                    if (priority > script.priority)
                    {
                        if (Random.Range(0, 1000) == 1)
                        {
                            isTalking = true;
                            isFirst = true;
                            script.isTalking = true;
                            script.isFirst = false;
                            //Debug.Log("Conversation started by " + gameObject.name);
                        }
                    }
                }
            }
        }
    }
}
