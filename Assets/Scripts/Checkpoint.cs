using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour, IInteractable
{
    [SerializeField] GameObject _mindPalace, _ocean;

    public void Interact(PlayerController player)
    {
        Vector3 newPos;
        bool activatePalace;

        if (GameManager.instance.playerInPalace)
        {
            newPos = GameManager.instance.playerWorldPos;
            activatePalace = false;
        }
        else
        {
            newPos = _mindPalace.transform.position;
            activatePalace = true;
            GameManager.instance.playerWorldPos = player.transform.position;
        }

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
        //_ocean.SetActive(palaceActive);
        //GameManager.instance.sunLight.gameObject.SetActive(palaceActive);

        yield return new WaitForSeconds(1);

        UIManager.instance.BlackScreenFade(false);

        yield return new WaitForSeconds(1);

        player.Inputs.inputUpdate = player.Inputs.Unpaused;
    }
}
