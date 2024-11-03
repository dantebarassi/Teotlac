using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericPoolableObject : MonoBehaviour
{
    //ObjectPool<GenericPoolableObject> _objectPool;
    //
    //public void Initialize(ObjectPool<GenericPoolableObject> op)
    //{
    //    _objectPool = op;
    //}

    public static void TurnOff(GenericPoolableObject x)
    {
        x.gameObject.SetActive(false);
    }

    public static void TurnOn(GenericPoolableObject x)
    {
        x.gameObject.SetActive(true);
    }
}
