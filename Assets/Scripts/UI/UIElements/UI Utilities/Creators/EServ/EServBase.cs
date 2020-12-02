
public interface IEServBase
{
    void AddNew<T>(T service);
    T Get<T>(IEServUser ieServUser);
    IEServBase Unlock();
}


public abstract class EServBase<TLocator> : IEServBase where TLocator : new()
{
    public static TLocator Locator { get; } = new TLocator();
    protected abstract EServMaster Service { get; }

    public abstract void AddNew<T>(T service);
    public abstract T Get<T>(IEServUser ieServUser);
    public abstract IEServBase Unlock();
}