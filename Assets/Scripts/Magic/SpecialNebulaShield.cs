using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialNebulaShield : SpecialMagic
{
    NebulaShield _nebulaShield;
    float _preparation, _recovery, _cooldown;
    
    public SpecialNebulaShield(PlayerController player, Inputs inputs, NebulaShield nebulaShield, float cost, float preparation, float recovery, float cooldown)
    {
        _player = player;
        _inputs = inputs;
        _nebulaShield = nebulaShield;
        _staminaCost = cost;
        _preparation = preparation;
        _recovery = recovery;
        _cooldown = cooldown;
    }

    public override bool Activate(out float cooldown)
    {
        _player.StartCoroutine(Shielding());
        cooldown = _cooldown;
        return true;
    }

    public override bool AltActivate(out float cooldown)
    {
        return Activate(out cooldown);
    }

    public override float ReturnCost()
    {
        return _staminaCost;
    }

    IEnumerator Shielding()
    {
        var camForward = Camera.main.transform.forward.MakeHorizontal();

        _inputs.inputUpdate = _inputs.FixedCast;
        _player.transform.forward = camForward;
        //_player.anim.SetBool("IsAttacking", true);

        yield return new WaitForSeconds(_preparation);

        var nebula = Object.Instantiate(_nebulaShield, _player.transform.position + camForward * 1.5f + Vector3.up * 0.5f, Quaternion.Euler(_player.transform.eulerAngles - new Vector3(-90, 0)));
        nebula.target = _player.currentBoss;

        yield return new WaitForSeconds(_recovery);

        _inputs.inputUpdate = _inputs.Unpaused;
    }
}
