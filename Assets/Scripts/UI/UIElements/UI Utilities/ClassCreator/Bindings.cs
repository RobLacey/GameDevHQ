public class Bindings : Bind
{
    protected override void BindClasses()
    {
        ClassCreate.Bind<TestClass>().To<ITestClass>().WithParamaters();
        ClassCreate.Bind<FadeTween>().To<IFadeTween>();
    }
}