using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialsManager : MonoBehaviour
{
    public enum Specials
    {
        Sunstrike,
        Supernova,
        Firewall,
        NebulaShield,
        ObsidianTrap,
        RockToss
    }
    Dictionary<Specials, (SpecialMagic, Sprite)> _allSpecials = new();
    SpecialMagic[] _equippedSpecials = new SpecialMagic[2];

    [SerializeField] AudioSource _audioSpawner;

    [Header("Sunstrike")]
    [SerializeField] Sprite _sunstrikeIcon;
    [SerializeField] GameObject _sunstrikeFirstRay;
    [SerializeField] GameObject _sunstrikeSecondRay;
    [SerializeField] AudioClip _sunstrikeSound;
    [SerializeField] float _sunstrikeCost, _sunstrikeDamage, _sunstrikeRadius, _sunstrikePreparation, _sunstrikeDelay, _sunstrikeLinger, _sunstrikeCooldown;

    [Header("Supernova")]
    [SerializeField] Sprite _supernovaIcon;
    [SerializeField] GameObject _supernova;
    [SerializeField] float _supernovaCost, _supernovaRadius, _supernovaDamage, _supernovaPreparation, _supernovaDuration, _supernovaRecovery, _supernovaCooldown;

    [Header("Firewall")]
    [SerializeField] Sprite _firewallIcon;
    [SerializeField] GameObject _firewall;
    [SerializeField] float _firewallCost, _firewallPreparation, _firewallRecovery, _firewallCooldown;

    [Header("Nebula Shield")]
    [SerializeField] Sprite _nebulaShieldIcon;
    [SerializeField] NebulaShield _nebulaShield;
    [SerializeField] float _nebulaShieldCost, _nebulaShieldPreparation, _nebulaShieldRecovery, _nebulaShieldCooldown;

    [Header("Obsidian Trap")]
    [SerializeField] Sprite _obsidianTrapIcon;
    [SerializeField] ObsidianTrap _obsidianTrap;
    [SerializeField] float _obsidianTrapCost, _obsidianTrapShardDamage, _obsidianTrapShardSpeed, _obsidianTrapPreparation, _obsidianTrapRecovery, _obsidianTrapCooldown;

    [Header("Rock Toss")]
    [SerializeField] Sprite _rockTossIcon;
    [SerializeField] Rock _rock;
    [SerializeField] Transform _rockTossPos;
    [SerializeField] float _rockTossCost, _rockTossDamage, _rockTossStrength, _rockTossAngle, _rockTossPreparation, _rockTossRecovery, _rockTossCooldown;

    PlayerController _player;
    Inputs _inputs;

    float[] _slotsCooldowns = new float[2];
    float[] _slotsCurrentCooldowns = new float[2];

    private void Start()
    {
        _player = GetComponent<PlayerController>();
        _inputs = _player.Inputs;

        var sunstrike = new SpecialSunstrike(_player, _inputs, _sunstrikeFirstRay, _sunstrikeSecondRay, _audioSpawner, _sunstrikeSound, _sunstrikeCost, _sunstrikeDamage, _sunstrikeRadius, _sunstrikePreparation, _sunstrikeDelay, _sunstrikeLinger, _sunstrikeCooldown);
        var supernova = new SpecialSupernova(_player, _inputs, _supernova, _supernovaCost, _supernovaRadius, _supernovaDamage, _supernovaPreparation, _supernovaDuration, _supernovaRecovery, _supernovaCooldown);
        var firewall = new SpecialFirewall(_player, _inputs, _firewall, _firewallCost, _firewallPreparation, _firewallRecovery, _firewallCooldown);
        var nebulaShield = new SpecialNebulaShield(_player, _inputs, _nebulaShield, _nebulaShieldCost, _nebulaShieldPreparation, _nebulaShieldRecovery, _nebulaShieldCooldown);
        var obsTrap = new SpecialObsidianTrap(_player, _inputs, _obsidianTrap, _obsidianTrapCost, _obsidianTrapShardDamage, _obsidianTrapShardSpeed, _obsidianTrapPreparation, _obsidianTrapRecovery, _obsidianTrapCooldown);
        var rockToss = new SpecialRockToss(_player, _inputs, _rock, _rockTossPos, _rockTossCost, _rockTossDamage, _rockTossStrength, _rockTossAngle, _rockTossPreparation, _rockTossRecovery, _rockTossCooldown);

        _allSpecials.Add(Specials.Sunstrike, (sunstrike, _sunstrikeIcon));
        _allSpecials.Add(Specials.Supernova, (supernova, _supernovaIcon));
        _allSpecials.Add(Specials.Firewall, (firewall, _firewallIcon));
        _allSpecials.Add(Specials.NebulaShield, (nebulaShield, _nebulaShieldIcon));
        _allSpecials.Add(Specials.ObsidianTrap, (obsTrap, _obsidianTrapIcon));
        _allSpecials.Add(Specials.RockToss, (rockToss, _rockTossIcon));

        EquipSpecial(Specials.Sunstrike, 0);
        EquipSpecial(Specials.NebulaShield, 1);
    }

    private void Update()
    {
        if (_slotsCurrentCooldowns[0] > 0)
        {
            Debug.Log("slot 0 cd updating");
            _slotsCurrentCooldowns[0] -= Time.deltaTime;
            UIManager.instance.UpdateSpecialCooldown(0, Mathf.InverseLerp(0, _slotsCooldowns[0], _slotsCurrentCooldowns[0]));
        }
        if (_slotsCurrentCooldowns[1] > 0)
        {
            Debug.Log("slot 1 cd updating");
            _slotsCurrentCooldowns[1] -= Time.deltaTime;
            UIManager.instance.UpdateSpecialCooldown(1, Mathf.InverseLerp(0, _slotsCooldowns[1], _slotsCurrentCooldowns[1]));
        }
    }

    public float GetCost(int slot)
    {
        return _equippedSpecials[slot].staminaCost;
    }

    public bool IsOffCooldown(int slot)
    {
        return _slotsCurrentCooldowns[slot] <= 0;
    }

    public void ActivateSpecial(int slot)
    {
        _slotsCooldowns[slot] = _equippedSpecials[slot].Activate();
        _slotsCurrentCooldowns[slot] = _slotsCooldowns[slot];
    }

    public void EquipSpecial(Specials special, int slot)
    {
        if (slot < 0 || slot >= _equippedSpecials.Length) return;

        _equippedSpecials[slot] = _allSpecials[special].Item1;
        UIManager.instance.UpdateSpecialIcon(slot, _allSpecials[special].Item2);
    }
}
