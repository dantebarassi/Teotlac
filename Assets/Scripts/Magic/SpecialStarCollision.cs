using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialStarCollision : SpecialMagic
{
    CollidingStars _starsPrefab, _spawnedStars;
    float _preparation, _recovery, _cooldown;
    bool _startedCasting = false, _thrown = false;

    public SpecialStarCollision(PlayerController player, Inputs inputs, CollidingStars stars, float cost, float preparation, float recovery, float cooldown)
    {
        _player = player;
        _inputs = inputs;
        _starsPrefab = stars;
        _staminaCost = cost;
        _preparation = preparation;
        _recovery = recovery;
        _cooldown = cooldown;
    }

    public override bool Activate(out float cooldown)
    {
        if (!_startedCasting)
        {
            _player.StartCoroutine(Cast());
            cooldown = 0;
            return true;
        }
        else if(_thrown)
        {
            Recast();
            cooldown = _cooldown;
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
        _thrown = false;
        _spawnedStars = null;
        cooldown = _cooldown;
        return false;
    }

    public override float ReturnCost()
    {
        return _thrown ? 0 : _staminaCost;
    }

    IEnumerator Cast()
    {
        _startedCasting = true;
        _player.anim.SetBool("isChargingStar", true);
        _inputs.inputUpdate = _inputs.FixedCast;

        float timer = 0;

        while (timer < _preparation)
        {
            if (_player.StopChannels)
            {
                _player.Specials.ActivateSpecial(SpecialsManager.Specials.StarCollision, true);

                _startedCasting = false;

                _player.anim.SetBool("isChargingStar", false);
                _inputs.inputUpdate = _inputs.Unpaused;

                yield break;
            }

            timer += Time.deltaTime;

            yield return null;
        }

        _spawnedStars = Object.Instantiate(_starsPrefab, _player.sunSpawnPoint[0].position, Quaternion.identity);
        _spawnedStars.player = _player;
        _spawnedStars.transform.forward = Camera.main.transform.forward.MakeHorizontal();

        _thrown = true;

        yield return new WaitForSeconds(_recovery);

        _player.anim.SetBool("isChargingStar", false);
        _inputs.inputUpdate = _inputs.Unpaused;
    }

    void Recast()
    {
        _startedCasting = false;
        _thrown = false;
        if (_spawnedStars != null) _spawnedStars.Collide();
        _spawnedStars = null;
    }
}
