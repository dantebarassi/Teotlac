using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    /// <summary>
    /// Changes Y value of a vector to 0
    /// </summary>
    public static Vector3 MakeHorizontal(this Vector3 vector)
    {
        return new Vector3(vector.x, 0, vector.z);
    }

    /// <summary>
    /// Changes Y value of a quaternion by 'amount'
    /// </summary>
    public static Quaternion AddYRotation(this Quaternion quaternion, float amount)
    {
        var euler = quaternion.eulerAngles;
        return Quaternion.Euler(euler.x, euler.y + amount, euler.z);
    }

    public static bool InLineOfSightOf(this Vector3 startPos, Vector3 endPos, LayerMask obstacleLayer)
    {
        return !Physics.Linecast(startPos, endPos, obstacleLayer);
    }

    public static bool InLineOfSightOf(this Vector3 startPos, Vector3 endPos, LayerMask obstacleLayer, out RaycastHit hit)
    {
        return !Physics.Linecast(startPos, endPos, out hit, obstacleLayer);
    }

    public static Vector3 VectorVariation(this Vector3 vector, float multiplier, float xVariation = 0, float yVariation = 0, float zVariation = 0)
    {
        return new Vector3(vector.x + Random.Range(-xVariation, xVariation) * multiplier, vector.y + Random.Range(-yVariation, yVariation) * multiplier, vector.z + Random.Range(-zVariation, zVariation) * multiplier);
    }

    public static float Remap(this float value, float minIn, float maxIn, float minOut, float maxOut)
    {
        return Mathf.Lerp(minOut, maxOut, Mathf.InverseLerp(minIn, maxIn, value));
    }
}
