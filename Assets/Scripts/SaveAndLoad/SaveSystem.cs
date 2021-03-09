using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveSystem
{
    
   public static void SavePlayer(PlayerStats player)
    {
        int i = 0;
        BinaryFormatter formatter = new BinaryFormatter();
        while(File.Exists(Application.persistentDataPath+ "/SaveFile" + i + ".data"))
        {
            i++;
        }
        string path = Application.persistentDataPath + "/SaveFile"+i+".data";
        
        FileStream stream = new FileStream(path, FileMode.Create);

        PlayerData data = new PlayerData(player);
        Debug.Log("file saved at path: " + path);
        formatter.Serialize(stream, data);
        stream.Close();

    }

    public static PlayerData LoadPlayer(string saveName)
    {
        string path = Application.persistentDataPath +"/"+ saveName;
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path,FileMode.Open);

            PlayerData data = formatter.Deserialize(stream) as PlayerData;
            stream.Close();

            return data;

        }
        else
        {
            Debug.Log("Save file not found in " + path);
            return null;
        }
    }
}
