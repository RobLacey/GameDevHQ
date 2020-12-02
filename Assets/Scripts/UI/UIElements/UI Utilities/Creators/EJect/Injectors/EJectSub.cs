public class EJectSub : EJectBase<ClassBindingsSub, EJectSub>
{
    public override TBind WithParams<TBind>(IParameters args) => Bind.EJectClass().Get<TBind>(args);
    public override TBind NoParams<TBind>() => Bind.EJectClass().Get<TBind>();
}