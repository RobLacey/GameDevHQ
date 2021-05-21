using EZ.Inject;
using UnityEngine;

public class InjectExampleClass : EZInjectBase<ClassBindingsExample, InjectExampleClass>
{
    public override TBind WithParams<TBind>(IParameters args) => Bind.Injector().Get<TBind>(args);
    public override TBind NoParams<TBind>() => Bind.Injector().Get<TBind>();
}