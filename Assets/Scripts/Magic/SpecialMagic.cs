using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SpecialMagic
{
    protected PlayerController _player;
    protected Inputs _inputs;
    public float staminaCost;

    public abstract float Activate();
}
