using System.Collections;

public class EVentBindingsSub : IEVentBindings
{
    public EVentBindingsSub()
    {
        BindAllObjects();
    }

    public Hashtable Events { get; } = new Hashtable();

    public void BindAllObjects()
    {
        
    }

    public void CreateEvent<TType>()
    {
        
    }
}