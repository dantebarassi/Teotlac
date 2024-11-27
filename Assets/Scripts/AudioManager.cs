using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{
    public enum FloorMaterials
    {
        Stone,
        Grass
    }

    public static AudioManager instance;

    [Header("Player")]
    [SerializeField] AudioClip _xd;
    [SerializeField] AudioClip _xd2;
    [SerializeField] AudioClip[] _playerGrassFootsteps, _playerStoneFootsteps;

    private void Awake()
    {
        if (instance != null) Destroy(gameObject);
        else instance = this;
    }

    public AudioClip PlayerFootsteps(string material)
    {
        Enum.TryParse<FloorMaterials>(material, out var result);

        switch (result)
        {
            case FloorMaterials.Stone:
                return _playerStoneFootsteps[UnityEngine.Random.Range(0, _playerStoneFootsteps.Length)];
            case FloorMaterials.Grass:
                return _playerGrassFootsteps[UnityEngine.Random.Range(0, _playerGrassFootsteps.Length)];
            default:
                return _playerStoneFootsteps[UnityEngine.Random.Range(0, _playerStoneFootsteps.Length)];
        }
    }
}
