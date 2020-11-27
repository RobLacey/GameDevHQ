public class EJect : EJectBase<ClassBindings, EJect>
{
    public override TBind WithParams<TBind>(IParameters args) => CreateClass.Get<TBind>(args);
    public override TBind NoParams<TBind>() => CreateClass.Get<TBind>();
}