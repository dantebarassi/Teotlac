using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Firewall : EnvironmentHazard
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Projectile bullet))
        {
            if (bullet.gameObject.layer == 11)
            {
                bullet.Die();
            }
        }
    }
}
