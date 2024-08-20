using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : Entity
{
    [SerializeField] protected float _distFromPivotToFloor;

    public float DistFromPivotToFloor
    {
        get { return _distFromPivotToFloor; }
    }

    public override void Die()
    {
        Destroy(gameObject);
    }
}
