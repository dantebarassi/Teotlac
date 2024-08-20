using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    ObsidianPathfindManager manager { get => ObsidianPathfindManager.instance; }
    List<Node> AllNodes { get => manager.allNodes; }

    [SerializeField] private List<Node> _neighbors = new List<Node>();

    private int _cost = 1;
    public int Cost { get => _cost; }

    public void Initialize()
    {
        foreach (var item in AllNodes)
        {
            if (item == this) continue;
            if (IsNodeNeighbor(item))
            {
                _neighbors.Add(item);
            }
        }
    }

    public List<Node> Neighbors
    {
        get
        {
            return _neighbors;
        }
    }

    public bool isBlocked;

    public void SetBlock(bool block)
    {
        isBlocked = block;
    }

    public void SetCost(int cost)
    {
        _cost = Mathf.Clamp(cost, 1, 99);
        //CostText = _cost.ToString();
        //_textMesh.enabled = cost != 1;
        //if (!isBlocked) ChangeColor(_cost == 1 ? Color.white : costColor);
    }


    public bool IsNodeNeighbor(Node node)
    {

        if (Vector3.Distance(transform.position, node.transform.position) <= manager.neighborDistance)
        {
            return true;
        }
        else
        {
            return false;
        }

        //var dir = node.transform.position - transform.position;
        //if(Physics.Raycast(transform.position, dir, dir.magnitude, gm.wallLayer))
        //{
        //    return false;
        //}
        //else
        //{
        //    Physics.Raycast(transform.position, dir, out RaycastHit hit, dir.magnitude, gm.nodeLayer);
        //    hit.collider.TryGetComponent(out Node hitNode);
        //
        //    if (hitNode == node)
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}
    }
}
