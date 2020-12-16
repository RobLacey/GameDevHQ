using System.Collections;

public interface IEVentBindings
{
    Hashtable Events { get; }
    void BindAllObjects();
    void CreateEvent<TType>();
}