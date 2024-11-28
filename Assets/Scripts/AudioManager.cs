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
    public AudioClip roll;
    public AudioClip jump;
    [SerializeField] AudioClip[] _playerGrassFootsteps, _playerStoneFootsteps;

    [Header("Magic")]
    [SerializeField] AudioClip[] _comboAttacks;
    public AudioClip comboHit;

    [Header("Itztlacoliuhqui")]
    public AudioClip basicAttackShardSpawn;
    public AudioClip stomp, shardHit, limbHit;

    [Header("Environment")]
    [SerializeField] AudioClip[] _structureBreak;

    int _lastFootstepIndex = -1, _lastComboIndex = -1, _lastStructureIndex = -1;

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
                return GetRandomFromArray(_playerStoneFootsteps, ref _lastFootstepIndex);
            case FloorMaterials.Grass:
                return GetRandomFromArray(_playerGrassFootsteps, ref _lastFootstepIndex);
            default:
                return GetRandomFromArray(_playerStoneFootsteps, ref _lastFootstepIndex);
        }
    }

    public AudioClip PlayerCombo()
    {
        return GetRandomFromArray(_comboAttacks, ref _lastComboIndex);
    }

    public AudioClip StructureBreak()
    {
        return GetRandomFromArray(_structureBreak, ref _lastStructureIndex);
    }

    AudioClip GetRandomFromArray(AudioClip[] array, ref int lastPicked)
    {
        int index;
        do
        {
            index = UnityEngine.Random.Range(0, array.Length);
        } while (index == lastPicked);

        lastPicked = index;

        return array[lastPicked];
    }
}
