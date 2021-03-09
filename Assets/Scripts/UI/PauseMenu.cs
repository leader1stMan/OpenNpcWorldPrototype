using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public GameObject btnParent;
    public GameObject button;
    public GameObject PlayerStat;
    public GameObject PausePanel;
    public GameObject LoadGameList;
    private int index;
    // Start is called before the first frame update
    void Start()
    {
        PopulateSavedGames();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PopulateSavedGames()
    {
        foreach (Transform child in btnParent.transform)
        {
            Destroy(child.gameObject);
            Debug.Log("destroyed" + child.name);
        }
        if (Directory.Exists(Application.persistentDataPath))
        {
            string worldsFolder = Application.persistentDataPath;

            DirectoryInfo d = new DirectoryInfo(worldsFolder);
            int i = 0;
            foreach (var file in d.GetFiles("*.data"))
            {
                PlayerData data= SaveSystem.LoadPlayer(file.Name);
                var newBtn = Instantiate(button, transform);
                newBtn.transform.SetParent(btnParent.transform, false);
                Button b = newBtn.GetComponent<Button>();
                b.GetComponentInChildren<Text>().text ="Level "+ data.level.ToString();
                var a = data;
                AddListener(b, a);
                i++;
            }
        }
        else
        {
            //File.Create(Application.persistentDataPath);
            return;
        }
    }
    void AddListener(Button b, PlayerData name)
    {
        b.onClick.AddListener(() => LoadSaveGame(name));
        //index = value;
    }
    public void LoadSaveGame(PlayerData data)
    {
        PlayerStat.GetComponent<PlayerStats>().LoadPlayer(data);
    }
    public void LoadButton()
    {
        LoadGameList.SetActive(true);
    }
    public void ResumeButton()
    {
        LoadGameList.SetActive(false);
        PausePanel.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
