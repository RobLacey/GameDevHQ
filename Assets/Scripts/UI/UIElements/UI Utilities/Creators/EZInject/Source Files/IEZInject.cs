namespace EZ.Inject
{
    public interface IEZInject
    {
        TBind WithParams<TBind>(IParameters args);
        TBind NoParams<TBind>();
    }
}