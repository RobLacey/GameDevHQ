using System;
using System.Reflection;
using Debug = UnityEngine.Debug;

public static class ReflectUtil
{
    private static int index;
    private static readonly NavigationType[] directions = 
    {
        NavigationType.AllDirections, 
        NavigationType.RightAndLeft
    };
    
    public static void SetFields<T>(T testClass, Type attribute)
    {
        Type type = typeof(T);
        var fieldInfo = type.GetFields(BindingFlags.Instance
                                       | BindingFlags.DeclaredOnly
                                       | BindingFlags.NonPublic);
        
        SearchClass(testClass, attribute, fieldInfo);
    }

    private static void SearchClass<T>(T testClass, Type attribute, FieldInfo[] fieldInfo)
    {
        foreach (var info in fieldInfo)
        {
            if(info.GetCustomAttribute(attribute) is null) continue;
            var type = info.FieldType;
            
             var newType = ClassCreate.Get(type);
            //var newType = System.Activator.CreateInstance(info.FieldType);
            info.SetValue(testClass, newType);
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