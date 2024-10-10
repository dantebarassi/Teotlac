using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObsidianShard : Projectile
{
    ObjectPool<ObsidianShard> _objectPool;

    public void Initialize(ObjectPool<ObsidianShard> pool, float spd, float dmg)
    {
        _objectPool = pool;
        speed = spd;
        damage = dmg;
    }

    public static void TurnOff(ObsidianShard x)
    {
        x.gameObject.SetActive(false);
    }

    public static void TurnOn(ObsidianShard x)
    {
        x._timer = x._duration; 

        x.gameObject.SetActive(true);
    }

    public override void Die()
    {
        _objectPool.RefillStock(this);
    }
}
