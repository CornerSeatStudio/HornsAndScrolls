using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
public static class Saving
{
    public static List<SaveData> savedGames = new List<SaveData>();
 
    public static void Save() {
        savedGames.Add(SaveData.current);
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create (Application.persistentDataPath + "/savedGames.gd");
        bf.Serialize(file, Saving.savedGames);
        file.Close();
}
    public static void Load() {
        if(File.Exists(Application.persistentDataPath + "/savedGames.gd")) {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/savedGames.gd", FileMode.Open);
            Saving.savedGames = (List<SaveData>)bf.Deserialize(file);
            file.Close();
    }
}
}
