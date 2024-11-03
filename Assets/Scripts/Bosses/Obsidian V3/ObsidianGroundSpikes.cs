using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class ObsidianGroundSpikes : MonoBehaviour
{
    [SerializeField] VisualEffect _spikesVFX;

    public void SetUpVFX(float size, float lifetime)
    {
        _spikesVFX.SetFloat("Radiu 2", size);
        _spikesVFX.SetFloat("SizeZ", size);
        _spikesVFX.SetFloat("Lifetime", lifetime);
    }

    public void Initialize(Vector3 pos, Quaternion rotation)
    {
        transform.position = pos;
        transform.rotation = rotation;

        _spikesVFX.Play();
    }

    public static void TurnOff(ObsidianGroundSpikes x)
    {
        x.gameObject.SetActive(false);
    }

    public static void TurnOn(ObsidianGroundSpikes x)
    {
        x.gameObject.SetActive(true);
    }
}
