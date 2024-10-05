using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialNebulaShield : SpecialMagic
{
    NebulaShield _nebulaShieldPrefab, _spawnedNebulaShield = null;
    float _preparation, _recovery, _cooldown;
    
    public SpecialNebulaShield(PlayerController player, Inputs inputs, NebulaShield nebulaShield, float cost, float preparation, float recovery, float cooldown)
    {
        _player = player;
        _inputs = inputs;
        _nebulaShieldPrefab = nebulaShield;
        _staminaCost = cost;
        _preparation = preparation;
        _recovery = recovery;
        _cooldown = cooldown;
    }

    public override bool Activate(out float cooldown)
    {
        if (_spawnedNebulaShield == null)
        {
            _player.StartCoroutine(Cast());
            cooldown = 0;
            return true;
        }
        else
        {
            Recast();
            cooldown = _cooldown;
            return true;
        }
    }

    public override bool AltActivate(out float cooldown)
    {
        _spawnedNebulaShield = null;
        cooldown = _cooldown;
        return true;
    }

    public override float ReturnCost()
    {
        return _staminaCost;
    }

    IEnumerator Cast()
    {
        var camForward = Camera.main.transform.forward.MakeHorizontal();

        _inputs.inputUpdate = _inputs.FixedCast;
        _player.transform.forward = camForward;
        _player.anim.SetTrigger("castBarrier");

        yield return new WaitForSeconds(_preparation);

        _spawnedNebulaShield = Object.Instantiate(_nebulaShieldPrefab, _player.transform.position + camForward * 1.5f + Vector3.up * 1.5f, Quaternion.Euler(_player.transform.eulerAngles - new Vector3(-90, 0)));
        _spawnedNebulaShield.SetUp(_player);

        yield return new WaitForSeconds(_recovery);

        _inputs.inputUpdate = _inputs.Unpaused;
    }

    void Recast()
    {
        _spawnedNebulaShield.Overcharge();
        _spawnedNebulaShield = null;
    }
}
