using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EJectMaster
{
    private readonly Hashtable classes = new Hashtable();
    private readonly List<Type> classesWithParameters = new List<Type>();
    private Type instanceType;
    private Type interfaceType;
    
    public EJectMaster Bind<T>()
    {
        instanceType = typeof(T);
        return this;
    }

    public EJectMaster To<T>()
    {
        interfaceType = typeof(T);
        if(classes.ContainsKey(typeof(T)))
            ThrowParameterException("Class Already Has Key : ", typeof(T));

        classes.Add(interfaceType, instanceType);
        return this;
    }

    public void WithParameters() => classesWithParameters.Add(interfaceType);

    public T Get<T>(IParameters args = null)
    {
        var typeOfNewClass = typeof(T);
        var newClass = classes[typeOfNewClass];
        var hasParams = classesWithParameters.Contains(typeOfNewClass);
        
        return args is null
            ? NoParameters<T>(typeOfNewClass, newClass, hasParams)
            : WithParameters<T>(args, typeOfNewClass, newClass, hasParams);
    }

    private T WithParameters<T>(IParameters args, Type typeOfNewClass, object newClass, bool hasParams)
    {
        if (!hasParams)
            ThrowParameterException($"Class DOESN'T HAVE Parameters or no Binding", typeOfNewClass);
        // try
        // {
            return (T) System.Activator.CreateInstance((Type) newClass, args);
        // }
        // catch (Exception e)
        // {
        //     ThrowParameterException("classes constructor has NO Parameters " +
        //                             "but is set up in Bindings as requiring them", typeOfNewClass);
        //     return default;
        // }
    }

    private T NoParameters<T>(Type typeOfNewClass, object newClass, bool hasParams)
    {
        if (hasParams)
            ThrowParameterException("Constructor HAS Parameters", typeOfNewClass);

        return (T) System.Activator.CreateInstance((Type) newClass);
    }

    private void ThrowParameterException(string message, Type typeOfNewClass)
    {
        throw new Exception($"{message} : {typeOfNewClass}");
    }
}