using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

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
    [SerializeField] GameObject _introCamera;
    [SerializeField] PlayableDirector _introCinematic, _bossIntroCutscene;
    public SunGodStone sunGodStone;

    public TutorialManager tutorial;
    
    //--Json--//
    public CustomJsonSaveSystem Json;
    public int actualBoss;

    [SerializeField] public PlayerController player;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        UIManager.instance.BlackScreenFade(false);

        if (!Json.LoadGame())
        {
            Debug.Log("ASDHJIASDHAOSIUD");
            Json.SaveGame();
        }
        else
        {
            Debug.Log("saveData no es null. Es:  " + JsonUtility.ToJson(Json.saveData));
            //Json.LoadGame();
            actualBoss = Json.saveData.actualBoss;
            //if (Json.saveData.lastCheckPoingPosition != Vector3.zero)
            //{
            //    _introCamera.SetActive(false);
            //    player.transform.position = Json.saveData.lastCheckPoingPosition;
            //    return;
            //}
        }

        if (!Json.saveData.finishedTutorial) tutorial.StartTutorial();
        else player.GetUp();

        //_introCinematic.Play();
        //
        //UIManager.instance.HideUI(true);
        //
        //StartCoroutine(FirstSection());
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

    public void PlayBossIntro()
    {
        UIManager.instance.HideUI(true);
        player.Cutscene(true);
        StartCoroutine(PlayingBossIntro());
    }

    IEnumerator PlayingBossIntro()
    {
        yield return new WaitForSeconds(1);

        _bossIntroCutscene.Play();

        yield return new WaitWhile(() => _bossIntroCutscene.state == PlayState.Playing);

        UIManager.instance.HideUI(false);
        player.Cutscene(false);
    }

    IEnumerator FirstSection()
    {
        player.Inputs.inputUpdate = player.Inputs.Nothing;

        yield return new WaitWhile(() => _introCinematic.state == PlayState.Playing);

        player.Inputs.inputUpdate = player.Inputs.Unpaused;
        UIManager.instance.HideUI(false);
    }
}
