
namespace EZ.Inject
{
    public abstract class EZInjectBase<TBind, TInject> : IEZInject
        where TBind : new() 
        where TInject : new()
    {
        public static TInject Class { get; } = new TInject();
        protected static TBind Bind { get; } = new TBind();

        public abstract T WithParams<T>(IParameters args);
        public abstract T NoParams<T>();
    }
}