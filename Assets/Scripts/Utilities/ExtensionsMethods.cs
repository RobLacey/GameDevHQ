using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public static class ExtensionsMethods
{

    //Set and objects rotation by each axis, as a vector3 or by a random amount
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

    //Vector X lerp

    public static Transform VectorLerp(this Transform t, float valueX, float valueY, float timer)
    {
        t.position = new Vector3(Mathf.Lerp(valueX, valueY, timer),
                                                     t.position.y,
                                                     t.position.z);

        return t;
    }

    public static Transform VectorLerp(this Transform t, Vector3 targetPos, float _timer)
    {
        t.position = Vector3.Lerp(t.position, targetPos, _timer);

        return t;
    }

    //Is object still on screen check
    //TODO uses bounds but need to change to use viewport so works with anything

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

    //Tween library

    public static float EaseINOut (this float perc)
    {
        perc = perc * perc * (3f - 2f * perc);
        return perc;
    }

    public static float EaseIN (this float perc)
    {
        perc = 1f - Mathf.Cos(perc * Mathf.PI * 0.5f);
        return perc;
    }

    public static float EaseOut (this float perc)
    {
        perc = Mathf.Sin(perc * Mathf.PI * 0.5f);
        return perc;
    }

    //Direction

    public static Vector3 Direction (this Transform from, Transform to)
    {
        return to.position - from.position;
    }

    public static Vector3 Direction (this Transform from, Vector3 to)
    {
        return to - from.position;
    }

    //GetChildren

    public static GameObject[] FillWithChildren (this GameObject[] array, Transform parent)
    {
        int index = 0;
        array = new GameObject[parent.childCount];
        foreach (Transform child in parent)
        {
            array[index] = child.gameObject;
            index++;
        }
        return array;

    }
}
