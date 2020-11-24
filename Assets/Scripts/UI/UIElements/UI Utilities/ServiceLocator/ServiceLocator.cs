using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public interface IServiceUser
{
    void SubscribeToService();
    
    //void ChangeService();
}

public class ServiceLocator
{
    private ServiceLocator() { }
    
    private static readonly Hashtable services = new Hashtable();
    private static readonly Dictionary<Type, List<IServiceUser>> waitingForServicesStore 
                            = new Dictionary<Type, List<IServiceUser>>();
    private static bool locked;
    
    public static void Bind<T>(T newService)
    {
        if(services.ContainsKey(typeof(T)) && !locked)
            services.Remove(typeof(T));
        
        if(services.ContainsKey(typeof(T)) && locked)
        {
            Debug.Log("Service already set. Unlock first to set");
            return;
        }
        
        locked = true;
        services.Add(typeof(T), newService);
        CheckForWaitingServiceUser(typeof(T));
    }
    
    public static T Get<T>(IServiceUser serviceUser)
    {
        if (!services.ContainsKey(typeof(T)))
        {
            if (!waitingForServicesStore.ContainsKey(typeof(T)))
            {
                waitingForServicesStore.Add(typeof(T), new List<IServiceUser>());
                //Debug.Log("Make new Service store for : " + typeof(T));
            }
            waitingForServicesStore[typeof(T)].Add(serviceUser);
            //Debug.Log("No Service so save method : " + user);
        }

        return (T) services[typeof(T)];
    }
    
    public static void UnlockService() => locked = false;
    
    /// <summary> Makes sure na new scene has no active services </summary>
    public static void Remove<T>()
    {
        if (services.ContainsKey(typeof(T)))
        {
            var monoBehaviourSub = services[typeof(T)] as IMonoBehaviourSub;
            // Debug.Log(typeof(T));
            monoBehaviourSub?.OnDisable();
            services.Remove(typeof(T));
        }
    }
    
    private static void CheckForWaitingServiceUser(Type type)
    {
        if(waitingForServicesStore.Count == 0 || !waitingForServicesStore.ContainsKey(type)) return;
        
        foreach (var service in waitingForServicesStore[type])
        {
           // Debug.Log("Subscribe : " + service);
            service.SubscribeToService();
        }
        waitingForServicesStore.Remove(type);
    }
}

public interface ICreate
{
    TestClass SetUpClass();
}


public class ClassCreator<T>
{
    public ClassCreator(Type newInstance)
    {
        _instance = newInstance;
        _interface = typeof(T);
        ClassLocator.AddClass(_interface, _instance);
    }
    
    private Type _instance;
    private Type _interface;
}

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

[DefectTrack("1", "20/10/20", "Rob", FixComment = "Not fully complete yet", Version = 1.1f)]
public class TestClass : ICreate
{
    // private static ClassCreator<ICreate> newClass { get; } = new ClassCreator<ICreate>(typeof(TestClass));
    
    private int _count = 1;
    private NavigationType _navType;
    [ClassInject] private FadeTween FadeMethod { get; set; }
    [ServiceInject] private PrintClass PrintClass { get; set; }
    private bool Blank { get; set; }

    
    public TestClass(int count, NavigationType nav)
    {
        _count = count;
        _navType = nav;
    }

    [DefectTrack("2", "20/10/20", "Rob", Origin = Origin.Playing)]
    public TestClass SetUpClass()
    {
        //Debug.Log(count);
        return this;
    }

    [DefectTrack("3", "21/10/20", "Rob", Origin = Origin.QA)]
    public void PrintCount()
    {
        Debug.Log(_count);
    }

    public void PrintNavType()
    {
        PrintClass.PrintIfIHaveWorked();
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


