using System.Collections;
using System.IO;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CustomJsonSaveSystem : MonoBehaviour
{
    public SaveData saveData = new SaveData();
    string path;

    private void Awake()
    {
        string customDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).Replace("\\","/") + "/" + Application.companyName + "/" + Application.productName;

        if (!Directory.Exists(customDirectory)) Directory.CreateDirectory(customDirectory);

        path = customDirectory + "/SavingData";

        //path = customDirectory + "/Iceberg.Incoming";

        Debug.Log(path);
    }

    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.S)) SaveGame();
        //if (Input.GetKeyDown(KeyCode.L)) LoadGame();
    }

    public void SaveGame()
    {
        //string json = JsonUtility.ToJson(saveData);
        //
        //Debug.Log(json);

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(path, json);

        Debug.Log(json);
    }

    public bool LoadGame()
    {
        Debug.Log(path);
        if (File.Exists(path))
        {
            //string json = File.ReadAllText(path);
            JsonUtility.FromJsonOverwrite(File.ReadAllText(path), saveData);
            return true;
        }
        else return false;
        
    }

    public void ChangeBoss(int boss)
    {
        saveData.actualBoss = boss;
    }
}