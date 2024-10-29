using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger
{
    bool _triggered = false;

    public bool Triggered
    {
        get
        {
            return _triggered;
        }
    }

    public void Triggers(MonoBehaviour behaviour)
    {
        if (_triggered == true) return;

        _triggered = true;
        behaviour.StartCoroutine(Deactivate());
    }

    IEnumerator Deactivate()
    {
        yield return null;

        yield return null;

        _triggered = false;
    }
}
