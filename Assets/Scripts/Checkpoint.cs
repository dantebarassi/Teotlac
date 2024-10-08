using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour, IInteractable
{
    [SerializeField] GameObject _mindPalace, _ocean;
    private CustomJsonSaveSystem chekJson;
    [SerializeField] int myBoss;
    [SerializeField] GameObject myFireAnim;
    [SerializeField] Transform _firstCheckpoint;
    Vector3 _firstCheckpointPos;

    private void Start()
    {
        chekJson = GameManager.instance.Json;

        _firstCheckpointPos = _firstCheckpoint.position + new Vector3(0, 0, 3);
    }
    public void Interact(PlayerController player)
    {
        Vector3 newPos;
        bool activatePalace;
        
        if (GameManager.instance.playerInPalace)
        {
            //newPos = GameManager.instance.playerWorldPos;
            activatePalace = false;
            if (chekJson.JsonExist()) newPos = chekJson.saveData.lastCheckPoingPosition != Vector3.zero ? chekJson.saveData.lastCheckPoingPosition : _firstCheckpointPos;
            else newPos = _firstCheckpointPos;
            GameManager.instance.sunGodStone.DespawnDummy();
        }
        else
        {
            newPos = _mindPalace.transform.position;
            activatePalace = true;
            GameManager.instance.playerWorldPos = player.transform.position;
        }
        //myFireAnim.transform.position = GameManager.instance.player.transform.position;
        //myFireAnim.GetComponent<Animation>().Play();
        player.TravelToPalace();
        chekJson.LoadGame();
        if (chekJson.saveData.actualBoss != myBoss) chekJson.saveData.actualBoss = myBoss;
        if (chekJson.saveData.lastCheckPoingPosition != newPos) chekJson.saveData.lastCheckPoingPosition = GameManager.instance.playerWorldPos;
        chekJson.SaveGame();

        StartCoroutine(TeleportPlayer(player, newPos, activatePalace));
    }

    IEnumerator TeleportPlayer(PlayerController player, Vector3 position, bool palaceActive)
    {
        player.Inputs.inputUpdate = player.Inputs.Nothing;
        GameManager.instance.playerInPalace = palaceActive;

        UIManager.instance.BlackScreenFade(true);

        yield return new WaitForSeconds(1);
    
        _mindPalace.SetActive(palaceActive);
        player.transform.position = position;
        //myFireAnim.transform.position = GameManager.instance.player.transform.position;
        //myFireAnim.GetComponent<Animation>().Play();
        //_ocean.SetActive(palaceActive);
        //GameManager.instance.sunLight.gameObject.SetActive(palaceActive);

        yield return new WaitForSeconds(1);

        UIManager.instance.BlackScreenFade(false);

        yield return new WaitForSeconds(1);

        player.Inputs.inputUpdate = player.Inputs.Unpaused;
    }
}
