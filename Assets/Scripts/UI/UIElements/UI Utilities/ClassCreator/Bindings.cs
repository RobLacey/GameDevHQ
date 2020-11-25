
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
        //Base
        ClassCreate.Bind<TestClass>().To<ITestClass>().WithParameters();
        ClassCreate.Bind<InjectClass>().To<IInjectClass>();
        
        //Tweens
        ClassCreate.Bind<PositionTween>().To<IPositionTween>();
        ClassCreate.Bind<RotateTween>().To<IRotationTween>();
        ClassCreate.Bind<ScaleTween>().To<IScaleTween>();
        ClassCreate.Bind<FadeTween>().To<IFadeTween>();
        ClassCreate.Bind<PunchTween>().To<IPunchTween>();
        ClassCreate.Bind<ShakeTween>().To<IShakeTween>();
        
        //Node Types
        ClassCreate.Bind<Standard>().To<IStandard>().WithParameters();
        ClassCreate.Bind<CancelOrBackButton>().To<ICancelOrBack>().WithParameters();
        ClassCreate.Bind<LinkedToggles>().To<ILinkedToggles>().WithParameters();
        ClassCreate.Bind<ToggleNotLinked>().To<IToggleNotLinked>().WithParameters();
        ClassCreate.Bind<HoverToActivate>().To<IHoverToActivate>().WithParameters();
        
        //Branch Types
        ClassCreate.Bind<HomeScreenBranch>().To<IHomeScreenBranch>().WithParameters();
        ClassCreate.Bind<StandardBranch>().To<IStandardBranch>().WithParameters();
        ClassCreate.Bind<ResolvePopUp>().To<IResolvePopUpBranch>().WithParameters();
        ClassCreate.Bind<OptionalPopUpPopUp>().To<IOptionalPopUpBranch>().WithParameters();
        ClassCreate.Bind<TimedPopUp>().To<ITimedPopUpBranch>().WithParameters();
        ClassCreate.Bind<PauseMenu>().To<IPauseBranch>().WithParameters();
        
        //NodeTweens
        ClassCreate.Bind<Position>().To<IPosition>().WithParameters();
        ClassCreate.Bind<Scale>().To<IScale>().WithParameters();
        ClassCreate.Bind<Punch>().To<IPunch>().WithParameters();
        ClassCreate.Bind<Shake>().To<IShake>().WithParameters();
        
        //BucketCreator
        ClassCreate.Bind<BucketCreator>().To<IBucketCreator>();
        
        //HistoryTrackerClasses
        ClassCreate.Bind<HistoryListManagement>().To<IHistoryManagement>().WithParameters();
        ClassCreate.Bind<MoveBackInHistory>().To<IMoveBackInHistory>().WithParameters();
        ClassCreate.Bind<ManagePopUpHistory>().To<IManagePopUpHistory>().WithParameters();
        ClassCreate.Bind<NewSelectionProcess>().To<INewSelectionProcess>().WithParameters();
        ClassCreate.Bind<PopUpController>().To<IPopUpController>();
        
        //NodeSearch
        ClassCreate.Bind<NodeSearch>().To<INodeSearch>();
        
        //ScreenData
        ClassCreate.Bind<ScreenData>().To<IScreenData>().WithParameters();
        
        //Input Classes
        ClassCreate.Bind<MenuAndGameSwitching>().To<IMenuAndGameSwitching>().WithParameters();
        ClassCreate.Bind<ChangeControl>().To<IChangeControl>().WithParameters();
        
        //Hub Classes
        ClassCreate.Bind<UIAudioManager>().To<IAudioService>().WithParameters();
        ClassCreate.Bind<UIHomeGroup>().To<IHomeGroup>().WithParameters();
        ClassCreate.Bind<HistoryTracker>().To<IHistoryTrack>().WithParameters();
    }
}