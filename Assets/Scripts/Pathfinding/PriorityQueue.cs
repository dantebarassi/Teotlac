using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriorityQueue<T>
{
    Dictionary<T, float> _allElements = new Dictionary<T, float>();

    public int Count { get => _allElements.Count; }

    public void Enqueue(T elem, float cost)
    {
        _allElements[elem] = cost;
    }

    public T Dequeue()
    {
        T elem = default;
        int count = 0;

        foreach (var keyValuePair in _allElements)
        {
            if (count == 0)
            {
                elem = keyValuePair.Key;
                count++;
                continue;
            }

            if (_allElements[elem] > keyValuePair.Value)
                elem = keyValuePair.Key;

            //elem = elem.Equals(default) || _allElements[elem] > keyValuePair.Value ? keyValuePair.Key : elem;
        }
        _allElements.Remove(elem);
        return elem;
    }

}
