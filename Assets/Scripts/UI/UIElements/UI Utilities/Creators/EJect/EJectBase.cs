
public abstract class EJectBase<TBind, TInject> : IEJect
                                                        where TBind : new() 
                                                        where TInject : new()
{
    public static TInject Class { get; } = new TInject();
    private static TBind Bindings { get; } = new TBind();

    public abstract TBind WithParams<TBind>(IParameters args);
    public abstract TBind NoParams<TBind>();
}