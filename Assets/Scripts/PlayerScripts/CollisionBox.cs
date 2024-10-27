using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CollisionBox
{
    public Vector3 boxPosition;
    public Vector3 boxSize;

    public Vector3 RelativeBoxPosition(Transform _transform)
    {
        Vector3 relativeX = boxPosition.x * _transform.right;
        Vector3 relativeY = boxPosition.y * _transform.up;
        Vector3 relativeZ = boxPosition.z * _transform.forward;

        return relativeX + relativeY + relativeZ;
    }
}