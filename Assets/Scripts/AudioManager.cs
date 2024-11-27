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
    [SerializeField] AudioClip[] _playerGrassFootsteps, _playerStoneFootsteps, _comboAttacks;

    int _lastFootstepIndex = -1, _lastComboIndex = -1;

    private void Awake()
    {
        if (instance != null) Destroy(gameObject);
        else instance = this;
    }

    public AudioClip PlayerFootsteps(string material)
    {
        Enum.TryParse<FloorMaterials>(material, out var result);

        int index;

        switch (result)
        {
            case FloorMaterials.Stone:
                do
                {
                    index = UnityEngine.Random.Range(0, _playerStoneFootsteps.Length);
                } while (index == _lastFootstepIndex);

                _lastFootstepIndex = index;

                return _playerStoneFootsteps[index];
            case FloorMaterials.Grass:
                do
                {
                    index = UnityEngine.Random.Range(0, _playerStoneFootsteps.Length);
                } while (index == _lastFootstepIndex);

                _lastFootstepIndex = index;
                return _playerGrassFootsteps[index];
            default:
                do
                {
                    index = UnityEngine.Random.Range(0, _playerStoneFootsteps.Length);
                } while (index == _lastFootstepIndex);

                _lastFootstepIndex = index;
                return _playerStoneFootsteps[index];
        }
    }

    public AudioClip PlayerCombo()
    {
        int index;

        do
        {
            index = UnityEngine.Random.Range(0, _playerStoneFootsteps.Length);
        } while (index == _lastComboIndex);

        _lastComboIndex = index;

        return _comboAttacks[index];
    }
}
