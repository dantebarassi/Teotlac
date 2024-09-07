using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public bool joystick=false, playerInPalace, hasCheckpoint;
    [SerializeField] private Material _vignettePostProcess;
    [SerializeField] private string _vignetteAmountName = "";
    public float aaaaa;
    public Vector3 playerWorldPos = Vector3.zero;
    public Light sunLight;
    public bool playIntro;

    //--Json--//
    public CustomJsonSaveSystem Json;
    public int actualBoss;

    [SerializeField] PlayerController player;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        if (Json.saveData == null)
            Json.SaveGame();

        Json.LoadGame();
        actualBoss = Json.saveData.actualBoss;
        if (Json.saveData.lastCheckPoingPosition != null) player.transform.position = Json.saveData.lastCheckPoingPosition;
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
