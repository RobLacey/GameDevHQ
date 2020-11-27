public interface IEJect
{
    TBind WithParams<TBind>(IParameters args);
    TBind NoParams<TBind>();
}