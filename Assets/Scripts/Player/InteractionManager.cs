using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractionManager : MonoBehaviour
{
    [SerializeField] PlayerController _player;
    IInteractable currentInteractable = null;
    bool _canInteract = false;
    public Image FButton;

    public void StartInteraction()
    {
        if (!_canInteract) return;

        currentInteractable.Interact(_player);
    }

    private void OnTriggerEnter(Collider other)
    {
        FButton.gameObject.SetActive(true);
        if (other.TryGetComponent(out IInteractable interactable))
        {
            currentInteractable = interactable;
            _canInteract = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        FButton.gameObject.SetActive(false);
        if (currentInteractable != null)
        {
            if (other.TryGetComponent(out IInteractable interactable))
            {
                if (currentInteractable == interactable)
                {
                    currentInteractable = null;
                    _canInteract = false;
                }
            }
        }
    }
}
