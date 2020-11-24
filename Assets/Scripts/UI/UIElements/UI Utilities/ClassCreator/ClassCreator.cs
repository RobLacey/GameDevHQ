using System;
using System.Collections;
using UnityEngine;

public static class ClassLocator
{
    private static readonly Hashtable classes = new Hashtable();
    
    public static void AddClass(Type newInterface, Type instance)
    {
        Debug.Log(instance + " : " + newInterface);
        classes.Add(newInterface, instance);
    }
    
    public static T GetClass<T>()
    {
        var temp = classes[typeof(T)];
        return (T) System.Activator.CreateInstance((Type) temp);
    }
}