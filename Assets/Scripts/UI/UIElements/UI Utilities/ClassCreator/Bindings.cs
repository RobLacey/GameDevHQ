
using System;

public abstract class Bind
{
    protected Bind()
    {
        BindClasses();
    }

    protected abstract void BindClasses();
}

public class Bindings : Bind
{
    protected override void BindClasses()
    {
        ClassCreate.Bind<TestClass>().To<ITestClass>().WithParamaters();
        //Tweens
        ClassCreate.Bind<PositionTween>().To<IPositionTween>();
        ClassCreate.Bind<RotateTween>().To<IRotationTween>();
        ClassCreate.Bind<ScaleTweener>().To<IScaleTween>();
        ClassCreate.Bind<FadeTween>().To<IFadeTween>();
        ClassCreate.Bind<PunchTweener>().To<IPunchTween>();
        ClassCreate.Bind<ShakeTweener>().To<IShakeTween>();
        
        //Node Types
        ClassCreate.Bind<Standard>().To<IStandard>().WithParamaters();
        ClassCreate.Bind<CancelOrBackButton>().To<ICancelOrBack>().WithParamaters();
        ClassCreate.Bind<LinkedToggles>().To<ILinkedToggles>().WithParamaters();
        ClassCreate.Bind<ToggleNotLinked>().To<IToggleNotLinked>().WithParamaters();
        ClassCreate.Bind<HoverToActivate>().To<IHoverToActivate>().WithParamaters();
        
        //Branch Types
        ClassCreate.Bind<HomeScreenBranch>().To<IHomeScreenBranch>().WithParamaters();
        ClassCreate.Bind<StandardBranch>().To<IStandardBranch>().WithParamaters();
        ClassCreate.Bind<ResolvePopUp>().To<IResolvePopUpBranch>().WithParamaters();
        ClassCreate.Bind<OptionalPopUpPopUp>().To<IOptionalPopUpBranch>().WithParamaters();
        ClassCreate.Bind<TimedPopUp>().To<ITimedPopUpBranch>().WithParamaters();
        ClassCreate.Bind<PauseMenu>().To<IPauseBranch>().WithParamaters();
        
        //NodeTweens
        ClassCreate.Bind<Position>().To<IPosition>().WithParamaters();
        ClassCreate.Bind<Scale>().To<IScale>().WithParamaters();
        ClassCreate.Bind<Punch>().To<IPunch>().WithParamaters();
        ClassCreate.Bind<Shake>().To<IShake>().WithParamaters();
        
        //HistoryTrackerClasses
        throw new Exception("Am Here");
    }
}