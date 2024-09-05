using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using System;
using TMPro;
using System;

public class DRGameManager : MonoBehaviour
{
    public CustomJsonSaveSystem Json;
    public int actualBoss;
    private void Start()
    {
        if (Json.saveData == null)
            Json.SaveGame();
        
        Json.LoadGame();
        actualBoss = Json.saveData.actualBoss;
    }
    public void Save()
    {
        Json.SaveGame();
    }

    public void DeleteGame()
    {
        Json.saveData.actualBoss = 0;
        Save();
    }
    public void ChangeBoss(int newBoss)
    {
        Json.saveData.actualBoss = newBoss;
        Save();
    }
}
