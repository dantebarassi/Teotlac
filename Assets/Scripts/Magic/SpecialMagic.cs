using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SpecialMagic
{
    protected PlayerController _player;
    protected Inputs _inputs;
    protected float _staminaCost;

    public abstract bool Activate(out float cooldown);

    public abstract float ReturnCost();
}
