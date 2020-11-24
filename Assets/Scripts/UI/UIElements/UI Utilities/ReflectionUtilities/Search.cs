using System;
using System.Reflection;
using UnityEngine;

public static class Search
{
    private static DefectTrackAttribute _attribute;
    private static MemberInfo[] _memberInfos;

    public static void StartReflection(Type type)
    {
        GetAssemblyInfo(type);
        GetClassAttribute(type);
        GetMethodAttributes(type);
    }

    private static void GetAssemblyInfo(Type type)
    {
        Debug.Log(Assembly.GetExecutingAssembly());

        foreach (var currentTypes in Assembly.GetExecutingAssembly().GetTypes())
        {
            GetDeclaredMethods(currentTypes);
            GetMemberInfo(currentTypes);
        }
    }

    private static void GetDeclaredMethods(Type currentTypes)
    {
        if (currentTypes == typeof(TestClass))
        {
            foreach (var declaredMethod in currentTypes.GetTypeInfo().DeclaredMethods)
            {
                Debug.Log(declaredMethod);
            }
        }
    }
    
    private static void GetMemberInfo(Type currentTypes)
    {
        if (currentTypes == typeof(TestClass))
        {
            foreach (var member in currentTypes.GetMembers(BindingFlags.Instance
                                                           | BindingFlags.Static
                                                           | BindingFlags.DeclaredOnly
                                                           | BindingFlags.Public
                                                           | BindingFlags.NonPublic))
            {
                Debug.Log($"{member.Name} : {member.MemberType}");
            }
        }
    }


    private static void GetClassAttribute(Type type)
    {
        _attribute = (DefectTrackAttribute) Attribute.GetCustomAttribute(type, typeof(DefectTrackAttribute));
        if (_attribute is null)
        {
            Debug.Log("Nothing Found");
        }
        else
        {
            Debug.Log($"Found this Info : {_attribute.DefectID} : " +
                      $"{_attribute.ModificationDate} : " +
                      $"{_attribute.DeveloperID} : " +
                      $"{_attribute.Version} : " +
                      $"{_attribute.FixComment} : " +
                      $"{_attribute.Origin}");
        }
    }

    private static void GetMethodAttributes(Type type)
    {
        _memberInfos = type.GetMethods(BindingFlags.Instance
                                       | BindingFlags.DeclaredOnly
                                       | BindingFlags.Public
                                       | BindingFlags.NonPublic);

        foreach (var t in _memberInfos)
        {
            _attribute = 
                (DefectTrackAttribute) Attribute.GetCustomAttribute(t, 
                                                                    typeof(DefectTrackAttribute));
            
            if (_attribute is null)
            {
                Debug.Log("Nothing Found : " + t);
            }
            else
            {
                Debug.Log($"Found this Info on : {t.Name} :-" +
                          $"{_attribute.DefectID} : " +
                          $"{_attribute.ModificationDate} : " +
                          $"{_attribute.DeveloperID} : " +
                          $"{_attribute.Version} : " +
                          $"{_attribute.FixComment} : " +
                          $"{_attribute.Origin}");
            }
        }
    }
}