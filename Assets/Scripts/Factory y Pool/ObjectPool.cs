using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ObjectPool<T>
{
    //Maneja las balas 
    public delegate T FactoryMethod();
    Action<T> _turnOn, _turnOff;

    int num;

    FactoryMethod _factory;

    List<T> _stock = new List<T>();
    
    //Action es un delegate que ya existe, recive metodos con determinadas cosas que pide ej t o int 
    public ObjectPool(FactoryMethod method, Action<T> turnOff, Action<T> turnOn, int warmup=5)
    {
        _factory = method;
        _turnOff = turnOff;
        _turnOn = turnOn;
        for (int i = 0; i < warmup; i++)
        {
            //Los voy creando y guardando 
            var x= _factory();
            _turnOff(x);
            _stock.Add(x);
        }
    }
    //Agarra de mi lista el primero, lo borra de la lista, lo prende, y se lo da 
    public T Get()
    {
        T x = default;
        // o sea solo si tiene algo
        if (_stock.Count > 0)
        {
             x = _stock[0];
            _stock.RemoveAt(0);
        }
        else
        {
            //Si no tengo instancio una bala
            x = _factory();
        }
        _turnOn(x);

        return x;
    }

    //Para volver a llenar, lo apago y lo vuelvo a agregar a la lista asi se puede volver a usar 
    public void RefillStock(T sample)
    {
        _turnOff(sample);
        _stock.Add(sample);
    }
}
