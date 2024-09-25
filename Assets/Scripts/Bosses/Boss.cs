using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : SteeringAgent
{
    [SerializeField] protected float _distFromPivotToFloor;

    public float DistFromPivotToFloor
    {
        get { return _distFromPivotToFloor; }
    }
}
