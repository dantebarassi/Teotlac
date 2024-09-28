using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunGodStone : MonoBehaviour, IInteractable
{
    [SerializeField] Color _emissiveColor;
    [SerializeField] float _emissiveIntensity;
    [SerializeField] Light _light;

    Material _material;
    //bool _toggledOn = false;

    private void Start()
    {
        _material = GetComponent<Material>();
    }

    public void Interact(PlayerController player)
    {
        StartCoroutine(ToggleLight(true));
    }

    IEnumerator ToggleLight(bool on)
    {
        float timer = 0;

        if (on)
        {
            while (timer < 1)
            {
                timer += Time.deltaTime;

                _material.SetColor("_EmissiveColor", _emissiveColor * Mathf.Lerp(0, _emissiveIntensity, timer));

                yield return null;
            }
        }
        else
        {
            while (timer < 1)
            {
                timer += Time.deltaTime;

                _material.SetColor("_EmissiveColor", _emissiveColor * Mathf.Lerp(_emissiveIntensity, 0, timer));

                yield return null;
            }
        }
    }
}
