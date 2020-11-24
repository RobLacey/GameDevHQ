using System;
using System.Reflection;
using UnityEngine;

public static class ReflectUtil
{
    private static int index;
    private static readonly NavigationType[] directions = 
    {
        NavigationType.AllDirections, 
        NavigationType.RightAndLeft
    };

    public static void SetFields<T>(T testClass)
    {
        Type type = typeof(T);
        var fieldInfo = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
        
        foreach (var info in fieldInfo)
        {
            switch (info.Name)
            {
                case "_count":
                    info.SetValue(testClass, (int)info.GetValue(testClass) + 1);
                    break;
                case "_navType":
                    info.SetValue(testClass, directions[index]);
                    index++;
                    break;
                default:
                    // throw new Exception("No field found - Possible name change or deletion");
                    break;
            }
        }
    }

    public static void GetProperty<T>(T testClass, Type attribute)
    {
        Type type = typeof(T);
        var propertyInfo = type.GetProperties(BindingFlags.Instance
                                              | BindingFlags.DeclaredOnly
                                              | BindingFlags.NonPublic);
        
        foreach (var info in propertyInfo)
        {
            if(info.GetCustomAttribute(attribute) is null) continue;
            var newType =  System.Activator.CreateInstance(info.PropertyType);
            info.SetValue(testClass, newType);
            Debug.Log(info.GetValue(testClass));
        }
    }

    public static void InvokeMethods<T>(T testClass)
    {
        Type type = typeof(T);
        var methodInfo = type.GetMethods(BindingFlags.Instance 
                                         | BindingFlags.DeclaredOnly 
                                         | BindingFlags.Public);

        foreach (var info in methodInfo)
        {
            if(info.GetParameters().Length == 0)
            {
                
                info.Invoke(testClass, null);
            }
            else
            {
                info.Invoke(testClass, new []{"Hello"});
            }
        }
        
        
    }
}