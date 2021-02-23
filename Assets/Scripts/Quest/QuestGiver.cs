
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestGiver : MonoBehaviour
{
    public Quest quest;
    public PlayerActions player;

    public GameObject questWindow;
    public Text title;
    public Text description;
    public Text reward;


    public GameObject button;
    public GameObject AcceptBtn;
    public GameObject btnParent;
    private Object[] quests;
    private int index;
    public void Start()
    {
        QuestScrollViewPopulator();
    }

    private void QuestScrollViewPopulator()
    {
        int i = 0;
        quests = Resources.LoadAll("Quests");
        foreach (var q in quests)
        {
            Quest y = (Quest)q;
            var newBtn = Instantiate(button, transform);
            newBtn.transform.SetParent(btnParent.transform,false);
            Button b = newBtn.GetComponent<Button>();
            b.GetComponentInChildren<Text>().text = y.title;
            var a = i;
            AddListener(b, a );
            i++;
        }
    }
    void AddListener(Button b, int value)
    {
        b.onClick.AddListener(() => OpenQuestWindow(value));
        index = value;
    }
    public void OpenQuestWindow(int index)
    {
        Quest y = (Quest)quests[index];
        questWindow.SetActive(true);
        title.text = y.title;
        description.text = y.description;
        reward.text = y.reward.ToString();
        Button a = AcceptBtn.GetComponent<Button>();
        a.onClick.AddListener(() => AcceptQuest(y));
    }
    public void AcceptQuest(Quest q)
    {
        
        quest = q;
        questWindow.SetActive(false);
        quest.isActive = true;
        player.quest = quest;
    }
}
