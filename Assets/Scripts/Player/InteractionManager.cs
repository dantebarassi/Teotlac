using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractionManager : MonoBehaviour
{
    [SerializeField] PlayerController _player;
    IInteractable currentInteractable = null;
    bool _canInteract = false;

    public void StartInteraction()
    {
        if (!_canInteract) return;
        
        currentInteractable.Interact(_player);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IInteractable interactable))
        {
            UIManager.instance.ToggleInteractable(true);
            currentInteractable = interactable;
            _canInteract = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (currentInteractable != null)
        {
            if (other.TryGetComponent(out IInteractable interactable))
            {
                if (currentInteractable == interactable)
                {
                    UIManager.instance.ToggleInteractable(false);
                    currentInteractable = null;
                    _canInteract = false;
                }
            }
        }
    }
}
