using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunGodStone : MonoBehaviour, IInteractable
{
    [SerializeField] float _emissiveIntensity;
    [SerializeField] Light _light;
    [Range(0, 8)]
    [SerializeField] float _lightIntensity;
    [SerializeField] float _transitionDuration;

    Material _material;
    //bool _toggledOn = false;

    private void Start()
    {
        _material = GetComponent<Renderer>().material;
    }

    public void Interact(PlayerController player)
    {
        StartCoroutine(ToggleLight(true, _transitionDuration));
    }

    IEnumerator ToggleLight(bool on, float duration)
    {
        float timer = 0, lerpT;
        
        if (on)
        {
            while (timer < duration)
            {
                timer += Time.deltaTime;

                lerpT = timer / duration;

                _material.SetFloat("_Emission_Intensity", Mathf.Lerp(0, _emissiveIntensity, lerpT));
                _light.intensity = Mathf.Lerp(0, _lightIntensity, lerpT);

                yield return null;
            }
        }
        else
        {
            while (timer < duration)
            {
                timer += Time.deltaTime;

                lerpT = timer / duration;

                _material.SetFloat("_Emission_Intensity", Mathf.Lerp(_emissiveIntensity, 0, lerpT));
                _light.intensity = Mathf.Lerp(_lightIntensity, 0, lerpT);

                yield return null;
            }
        }
    }
}
