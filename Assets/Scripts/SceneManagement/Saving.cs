using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
public static class Saving
{ 
    public static void Save(SaveManager player) {
        //Debug.Log(Application.persistentDataPath);
        BinaryFormatter bf = new BinaryFormatter();
        FileStream stream = File.Create (Application.persistentDataPath + "/saveFile.knk");
        SaveData data = new SaveData(player);
        bf.Serialize(stream, data);
        stream.Close();
}
    public static SaveData Load() {
        string path = Application.persistentDataPath + "/saveFile.knk";
        if(File.Exists(path)) {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream stream = File.Open(path, FileMode.Open);
            SaveData data = bf.Deserialize(stream) as SaveData;
            stream.Close();

            return data;
        } else {
            Debug.LogWarning("no file exists");
            return null;
        }
}
}
