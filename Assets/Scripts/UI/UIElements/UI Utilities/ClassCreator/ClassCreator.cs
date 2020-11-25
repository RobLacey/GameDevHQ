using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassCreate
{
    private static readonly Hashtable classes = new Hashtable();
    private static List<Type> _hasParameters = new List<Type>();
    private static Type _instanceType;
    private static Type _interfaceType;
    private static ClassCreate Create = new ClassCreate();
    private static Bindings bindings = new Bindings();
    private ClassCreate() { }

    public static ClassCreate Bind<T>()
    {
        _instanceType = typeof(T);
        return Create;
    }
    public ClassCreate To<T>()
    {
        _interfaceType = typeof(T);
        classes.Add(_interfaceType, _instanceType);
        return this;
    }

    public void WithParameters() => _hasParameters.Add(_interfaceType);

    public static T Get<T>(IParameters args = null)
    {
        var typeOfNewClass = typeof(T);
        var newClass = classes[typeOfNewClass];
        var hasParams = _hasParameters.Contains(typeOfNewClass);

        return args is null ? NoParameters<T>(typeOfNewClass, newClass, hasParams) 
                            : WithParameters<T>(args, typeOfNewClass, newClass, hasParams);
    }

    private static T WithParameters<T>(IParameters args, Type typeOfNewClass, object newClass, bool hasParams)
    {
        if (!hasParams)
            throw new Exception($"Class DOESN'T HAVE Parameters : {typeOfNewClass}");
        
        try
        {
            return (T) System.Activator.CreateInstance((Type) newClass, args);
        }
        catch (Exception e)
        {
            ThrowCreationException(newClass, e);
        }

        return default;
    }

    private static T NoParameters<T>(Type typeOfNewClass, object newClass, bool hasParams)
    {
        if (hasParams)
            throw new Exception($"Constructor HAS Parameters : {typeOfNewClass}");

        try
        {
            return(T) System.Activator.CreateInstance((Type) newClass);
        }
        catch (Exception e)
        {
            ThrowCreationException(newClass, e);
        }

        return default;
    }

    private static void ThrowCreationException(object newClass, Exception e)
    {
        throw new Exception($"Possible missing Interface on class or NO Binding : {newClass} " +
                            $"{Environment.NewLine} Error is : {e}");
    }
    
    public static object Get(Type type/*, IParameters args = null*/)
    {
        var temp = classes[type];
        
        // if (_hasParameters.Contains(type))
        // {
        //     return System.Activator.CreateInstance((Type) temp, args);
        // }
        return System.Activator.CreateInstance((Type) temp);
    }
}

public interface IParameters { }

public interface IInjectClass
{
    T WithParams<T>(IParameters args);
    T NoParams<T>();
}

public abstract class InjectClassBase : IInjectClass
{
    public abstract T WithParams<T>(IParameters args);
    public abstract T NoParams<T>();
}

public class InjectClass : InjectClassBase
{
    public static InjectClass Class { get; } = new InjectClass();
    public override T WithParams<T>(IParameters args) => ClassCreate.Get<T>(args);
    public override T NoParams<T>() => ClassCreate.Get<T>();
}



public interface ITestClass
{
    void SetUpClass(TestClassArgs args);
    void CheckConstruction();
}

public class TestClassArgs : IParameters
{
    public int Count { get;}
    public NavigationType NavType { get;}
    
    public TestClassArgs(int count, NavigationType navType)
    {
        Count = count;
        NavType = navType;
    }
}


[DefectTrack("1", "20/10/20", "Rob", FixComment = "Not fully complete yet", Version = 1.1f)]
public class TestClass : ITestClass
{
    // private static ClassCreator<ICreate> newClass { get; } = new ClassCreator<ICreate>(typeof(TestClass));
    
    private int _count = 1;
    private NavigationType _navType = NavigationType.None;
    private FadeTween FadeMethod { get; set; }
    private PrintClass PrintClass { get; set; }
    PrintClass _printClass;

    public TestClass(TestClassArgs args)
    {
        _count = args.Count;
        _navType = args.NavType;
    }

    public TestClass()
    {
        
    }
    
    [DefectTrack("2", "20/10/20", "Rob", Origin = Origin.Playing)]
    public void SetUpClass(TestClassArgs args)
    {
        _count = args.Count;
        _navType = args.NavType;
    }

    public void CheckConstruction()
    {
        Debug.Log(_count);
        Debug.Log(_navType);
    }

    [DefectTrack("3", "21/10/20", "Rob", Origin = Origin.QA)]
    public void PrintCount()
    {
        Debug.Log(_count);
    }

    public void PrintNavType()
    {
        _printClass.PrintIfIHaveWorked();
    }
    
    public void PlainTest(string message)
    {
        Debug.Log(message);
    }
}

public class PrintClass
{
    public void PrintIfIHaveWorked()
    {
        Debug.Log("I AM A LOVELY MAN");
    }
}
