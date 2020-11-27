using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateClass
{
    private static readonly Hashtable classes = new Hashtable();
    private static readonly List<Type> classesWithParameters = new List<Type>();
    private static Type instanceType;
    private static Type interfaceType;
    private static readonly CreateClass create = new CreateClass();
    
    public static CreateClass Bind<T>()
    {
        instanceType = typeof(T);
        return create;
    }

    public CreateClass To<T>()
    {
        interfaceType = typeof(T);
        classes.Add(interfaceType, instanceType);
        return this;
    }

    public void WithParameters() => classesWithParameters.Add(interfaceType);

    public static T Get<T>(IParameters args = null)
    {
        var typeOfNewClass = typeof(T);
        var newClass = classes[typeOfNewClass];
        var hasParams = classesWithParameters.Contains(typeOfNewClass);
        
        return args is null
            ? NoParameters<T>(typeOfNewClass, newClass, hasParams)
            : WithParameters<T>(args, typeOfNewClass, newClass, hasParams);
    }

    private static T WithParameters<T>(IParameters args, Type typeOfNewClass, object newClass, bool hasParams)
    {
        if (!hasParams)
            ThrowParameterException("Class DOESN'T HAVE Parameters or no Binding", typeOfNewClass);

        return (T) System.Activator.CreateInstance((Type) newClass, args);
    }

    private static T NoParameters<T>(Type typeOfNewClass, object newClass, bool hasParams)
    {
        if (hasParams)
            ThrowParameterException("Constructor HAS Parameters", typeOfNewClass);

        return (T) System.Activator.CreateInstance((Type) newClass);
    }

    private static void ThrowParameterException(string message, Type typeOfNewClass)
    {
        throw new Exception($"{message} : {typeOfNewClass}");
    }
}