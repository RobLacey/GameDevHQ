using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public static class ExtensionsMethods
{
    public static Transform Random2DRotation(this Transform _rotation)
    {
        _rotation.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360F));
        return _rotation;
    }
    public static Transform SetRotationX(this Transform _rotation, float value)
    {
        _rotation.rotation = Quaternion.Euler(value, 0, 0);
        return _rotation;
    }
    public static Transform SetRotationY(this Transform _rotation, float value)
    {
        _rotation.rotation = Quaternion.Euler(0, value, 0);
        return _rotation;
    }
    public static Transform SetRotationZ(this Transform _rotation, float value)
    {
        _rotation.rotation = Quaternion.Euler(0, 0, value);
        return _rotation;
    }
    public static Transform SetRotation(this Transform _rotation, float valueX, float valueY, float valueZ)
    {
        _rotation.rotation = Quaternion.Euler(valueX, valueY, valueZ);
        return _rotation;
    }

    public static bool StillOnScreen(this Transform myPosition, GlobalVariables bounds)
    {
        if (myPosition.position.y > bounds.TopBounds || myPosition.position.y < bounds.BottomBounds)
        {
            return false;
        }

        if (myPosition.position.x < bounds.LeftBounds || myPosition.position.x > bounds.RightBounds)
        {
            return false;
        }
        return true;
    }
}
