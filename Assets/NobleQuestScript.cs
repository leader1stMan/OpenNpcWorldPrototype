using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.IO;
using TMPro;

public class NobleQuestScript : MonoBehaviour
{
    public List<string> DialoguePaths;
    public string path = null;
    private TMP_Text text;
    public bool isFirst;

    public GameObject rioteerLeader;

    private void Start()
    {
        Rigidbody[] rig = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rigidbody in rig)
        {
            if (rigidbody != this.GetComponent<Rigidbody>())
            {
                rigidbody.GetComponent<Collider>().enabled = false;
                rigidbody.isKinematic = true;
            }
        }
        //Skinnedmesh needs to updated off screen when ragdolled. Or disappears
        SkinnedMeshRenderer[] skins = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer skinnedMeshRenderer in skins)
        {
            skinnedMeshRenderer.updateWhenOffscreen = false;
        }
        GetComponent<CapsuleCollider>().enabled = true;

        text = GetComponentInChildren<TMP_Text>();
    }

    IEnumerator Conversation(int n, bool talkToRioteer = false)
    {
        if (!talkToRioteer)
        {
            StreamReader reader;
            string line;
            List<string> lines = new List<string>();

            path = DialoguePaths[n];

            reader = new StreamReader(path);
            //Converstion is sotred in .txt file. "{}" separates first and second NPC's part 
            while ((line = reader.ReadLine()) != "{}")
            {
                lines.Add(line); //Storing the conversation by each line
            }

            text.text = null; //Ui for showing text
            for (int i = 0; i < lines.Count; i++)
            {
                if (!lines[i].StartsWith(" ")) //Displays sentece for 4 secs
                {
                    text.text = lines[i];
                    yield return new WaitForSeconds(4);
                    text.text = null;
                }
            }
        }
        else
        {
            StartCoroutine("RotateTo", rioteerLeader); //Look at talker

            StreamReader reader;
            TreasonQuest npc = rioteerLeader.GetComponent<TreasonQuest>();
            string line;
            List<string> lines = new List<string>();

            // if NPC is first, it randomly chooses conversation and assigns it to the second NPC
            // if NPC is second, it waits till conversation is assigned to him by the first NPC
            if (isFirst)
            {
                //Assigning the conversation to npc1, npc2
                path = DialoguePaths[n];
                npc.path = path;

                reader = new StreamReader(path);
                //Converstion is sotred in .txt file. "{}" separates first and second NPC's part 
                while ((line = reader.ReadLine()) != "{}")
                {
                    Debug.Log(true);
                    lines.Add(line); //Storing the conversation by each line
                }
            }
            else
            {
                yield return new WaitUntil(() => path != null); //Wait till first NPC sends the conversation he chose
                reader = new StreamReader(path);
                while (reader.ReadLine() != "{}") ;
                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }

            text.text = null; //Ui for showing text
            yield return new WaitUntil(() => isFirst); //We now don't need 'isFirst' to tell who started the conversation
            for (int i = 0; i < lines.Count; i++)
            {
                if (!lines[i].StartsWith(" ")) //Displays sentece for 4 secs
                {
                    text.text = lines[i];
                    yield return new WaitForSeconds(4);
                    text.text = null;
                }
                isFirst = false; //Turns 'isFirst' to false while pturning on it for the talker
                npc.isFirst = true; //Indicating that it's talker's time to speak
                yield return new WaitUntil(() => isFirst);
            }
            npc.isFirst = true;
        }

        EndConversation();
    }

    public void StartConversation(int n, bool isTrue)
    {
        StartCoroutine(Conversation(n, isTrue));
    }

    public void EndConversation()
    {
        GetComponent<NavMeshAgent>().isStopped = false;
        StopCoroutine("Conversation");
        StopCoroutine("RotateTo");
        path = null;
        isFirst = false;
        text.text = GetComponentInChildren<NpcData>().NpcName + "\nThe " + GetComponentInChildren<NpcData>().Job.ToString().ToLower();
    }
}
