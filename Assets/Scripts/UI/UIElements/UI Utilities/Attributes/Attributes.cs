using System;
using System.Reflection;
using Debug = UnityEngine.Debug;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public class DefectTrackAttribute : Attribute
{
    private readonly string _defectID ;
    private DateTime _modificationDate ;
    private readonly string _developerID ;
    private Origin _defectOrigin ;
    private string _fixComment ;
    private float _version;

    public DefectTrackAttribute( string lcDefectID, string lcModificationDate, string lcDeveloperID )
    { 
        _defectID = lcDefectID ;
        _modificationDate = DateTime.Parse(lcModificationDate);
        _developerID = lcDeveloperID ; 
    }

    public string DefectID => _defectID;

    public string ModificationDate
    {
        get => _modificationDate.ToShortDateString();
    }

    public string DeveloperID => _developerID;

    public Origin Origin
    { 
        get => _defectOrigin ; 
        set => _defectOrigin = value;
    }

    public string FixComment
    { 
        get => _fixComment;
        set => _fixComment = value;
    }
    
    public float Version
    { 
        get => _version;
        set => _version = value;
    }
}

[AttributeUsage(AttributeTargets.Property)]
public class ClassInjectAttribute : Attribute
{
    public ClassInjectAttribute() { }
}

[AttributeUsage(AttributeTargets.Property)]
public class ServiceInjectAttribute : Attribute
{
    public ServiceInjectAttribute() { }
}

public enum Origin { Testing, Playing, User, QA }


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
            Debug.Log(currentTypes.GetTypeInfo().Namespace);

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
                if (declaredMethod.Name == "SpareMethod")
                {
                    Debug.Log("Method Found");
                }
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
                Debug.Log(info.Name);
                info.Invoke(testClass, new []{"Hello"});
            }
        }
        
        
    }
}