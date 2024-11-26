using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialStarCollision : SpecialMagic
{
    CollidingStars _starsPrefab, _spawnedStars;
    Transform _spawnPos;
    float _chargeDuration, _preparation, _recovery, _cooldown;
    bool _startedCasting = false, _charging = false, _thrown = false;

    public SpecialStarCollision(PlayerController player, Inputs inputs, CollidingStars stars, Transform spawnPos, float cost, float chargeDuration, float preparation, float recovery, float cooldown)
    {
        _player = player;
        _inputs = inputs;
        _starsPrefab = stars;
        _spawnPos = spawnPos;
        _staminaCost = cost;
        _chargeDuration = chargeDuration;
        _preparation = preparation;
        _recovery = recovery;
        _cooldown = cooldown;
    }

    public override bool Activate(out float cooldown)
    {
        if (!_startedCasting)
        {
            _player.StartCoroutine(Cast());
            cooldown = _chargeDuration;
            return false;
        }
        else if (_thrown)
        {
            Recast();
            cooldown = _cooldown;
            return true;
        }
        else if (!_charging)
        {
            cooldown = 0;
            _thrown = true;
            return true;
        }
        else
        {
            cooldown = 0;
            return false;
        }
    }

    public override bool AltActivate(out float cooldown)
    {
        _startedCasting = false;
        _charging = false;
        _thrown = false;
        _spawnedStars = null;
        cooldown = _cooldown;
        return false;
    }

    public override float ReturnCost()
    {
        return _startedCasting ? 0 : _staminaCost;
    }

    IEnumerator Cast()
    {
        _player.canAttack = false;
        _startedCasting = true;
        _charging = true;
        _player.anim.SetBool("isChargingStar", true);
        //_inputs.inputUpdate = _inputs.FixedCast;

        _spawnedStars = Object.Instantiate(_starsPrefab, _spawnPos.position, Quaternion.identity);
        _spawnedStars.SetUp(_player, _player.sunSpawnPoint[0]);
        _spawnedStars.StartCharge(_chargeDuration);

        float timer = 0;

        while (timer < _chargeDuration)
        {
            if (_player.OnHit.Triggered)
            {
                _spawnedStars.Die();

                _player.Specials.ActivateSpecial(SpecialsManager.Specials.StarCollision, true);

                _startedCasting = false;

                _player.anim.SetBool("isChargingStar", false);
                //_inputs.inputUpdate = _inputs.Unpaused;

                _player.canAttack = true;

                yield break;
            }

            _player.ReduceStamina(0);

            timer += Time.deltaTime;

            yield return null;
        }

        _charging = false;

        _player.anim.SetBool("isChargingStar", false);
        _player.anim.SetBool("isHoldingStars", true);

        // animacion de sostener soles

        while (!_thrown)
        {
            if (_player.OnHit.Triggered)
            {
                _spawnedStars.Die();

                _player.Specials.ActivateSpecial(SpecialsManager.Specials.StarCollision, true);

                _startedCasting = false;

                _player.anim.SetBool("isHoldingStars", false);
                //_inputs.inputUpdate = _inputs.Unpaused;

                _player.canAttack = true;

                yield break;
            }

            yield return null;
        }

        _player.LookDir = Camera.main.transform.forward.MakeHorizontal();

        _thrown = true;

        _inputs.inputUpdate = _inputs.FixedCast;

        _player.anim.SetLayerWeight(2, 0);
        _player.anim.SetLayerWeight(3, 0);
        _player.anim.SetBool("isHoldingStars", false);
        _player.anim.SetTrigger("throwStars");

        yield return new WaitForSeconds(_preparation);

        _spawnedStars.Throw(Camera.main.transform.forward.MakeHorizontal());

        yield return new WaitForSeconds(_recovery);

        _inputs.inputUpdate = _inputs.Unpaused;

        _player.anim.SetLayerWeight(2, 1);
        _player.anim.SetLayerWeight(3, 1);

        _player.canAttack = true;
    }

    void Recast()
    {
        _startedCasting = false;
        _thrown = false;
        if (_spawnedStars != null) _spawnedStars.Collide();
        _spawnedStars = null;
    }
}
