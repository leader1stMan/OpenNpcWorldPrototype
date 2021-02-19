using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SentenceGoal
{
    static private string id;
    static private string color;
    static private DialogueManager dialogue;
    static int red=0;
    static int green=0;
    static int pink=0;
    public static void sentence(string []id_goal) //When dialogue reached, goal complete
    {
        test();
        foreach(string ids in id_goal)
        {            
            if(ids == id)
            {
                Debug.Log("Dialogue: " + id_goal + " complete");
            }
        }
    }

    static void test()
    {
        id = dialogue.sentence1.GetId();
        if(dialogue.sentence1.GetHasColor())
        {
            if((int) dialogue.sentence1.GetColor() == 0) //Green
            {
                green++;
            }
            else if((int) dialogue.sentence1.GetColor() == 1)
            {
                pink++;
            }
            else
            {
                red++;
            }
        }
    }
}
