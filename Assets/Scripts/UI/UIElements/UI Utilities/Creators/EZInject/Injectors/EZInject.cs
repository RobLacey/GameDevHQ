using EZ.Inject;

/// <summary>
/// Example Class. Change the ClassBindings class to your one and Rename this class
/// </summary>
public class EZInject : EZInjectBase<ClassBindings, EZInject>
{
    public override TBind WithParams<TBind>(IParameters args) => Bind.Injector().Get<TBind>(args);
    public override TBind NoParams<TBind>() => Bind.Injector().Get<TBind>();
}