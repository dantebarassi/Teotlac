using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour, IInteractable
{
    GameObject _mindPalace;

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
        }

        StartCoroutine(TeleportPlayer(player, newPos, activatePalace));
    }

    IEnumerator TeleportPlayer(PlayerController player, Vector3 position, bool palaceActive)
    {
        player.Inputs.inputUpdate = player.Inputs.Nothing;

        UIManager.instance.BlackScreenFade(true);

        yield return new WaitForSeconds(1);

        _mindPalace.SetActive(palaceActive);
        player.transform.position = position;

        UIManager.instance.BlackScreenFade(false);

        yield return new WaitForSeconds(1);

        player.Inputs.inputUpdate = player.Inputs.Unpaused;
    }
}
