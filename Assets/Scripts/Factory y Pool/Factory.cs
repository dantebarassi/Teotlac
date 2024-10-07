using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Factory<T> where T : MonoBehaviour
{
    //Crea las balas 
    public Factory(T prefab)
    {
        _prefab = prefab;
    }
    private T _prefab;
    public T GetObject()
    {
        return GameObject.Instantiate(_prefab);
    }
}
