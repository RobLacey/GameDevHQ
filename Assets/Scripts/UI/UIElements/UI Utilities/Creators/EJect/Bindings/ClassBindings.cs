public class ClassBindings : BindBase
{
    public ClassBindings()
    {
        _eJectMaster = new EJectMaster();
        BindAllObjects();
    }

    protected sealed override void BindAllObjects()
    {
        if(CheckIfAlreadyBound()) return;

        //Base
        _eJectMaster.Bind<EJect>().To<IEJect>();
        
        //Hub Classes
        _eJectMaster.Bind<UIAudioManager>().To<IAudioService>().WithParameters();
        _eJectMaster.Bind<UIHomeGroup>().To<IHomeGroup>().WithParameters();
        _eJectMaster.Bind<HistoryTracker>().To<IHistoryTrack>().WithParameters();

        //Tweens
        _eJectMaster.Bind<PositionTween>().To<IPositionTween>();
        _eJectMaster.Bind<RotateTween>().To<IRotationTween>();
        _eJectMaster.Bind<ScaleTween>().To<IScaleTween>();
        _eJectMaster.Bind<FadeTween>().To<IFadeTween>();
        _eJectMaster.Bind<PunchTween>().To<IPunchTween>();
        _eJectMaster.Bind<ShakeTween>().To<IShakeTween>();
        _eJectMaster.Bind<TweenInspector>().To<ITweenInspector>();
        
        //Node Types
        _eJectMaster.Bind<Standard>().To<IStandard>().WithParameters();
        _eJectMaster.Bind<CancelOrBackButton>().To<ICancelOrBack>().WithParameters();
        _eJectMaster.Bind<LinkedToggles>().To<ILinkedToggles>().WithParameters();
        _eJectMaster.Bind<HoverToActivate>().To<IHoverToActivate>().WithParameters();
        
        //Branch Types
        _eJectMaster.Bind<HomeScreenBranch>().To<IHomeScreenBranch>().WithParameters();
        _eJectMaster.Bind<StandardBranch>().To<IStandardBranch>().WithParameters();
        _eJectMaster.Bind<ResolvePopUp>().To<IResolvePopUpBranch>().WithParameters();
        _eJectMaster.Bind<OptionalPopUpPopUp>().To<IOptionalPopUpBranch>().WithParameters();
        _eJectMaster.Bind<TimedPopUp>().To<ITimedPopUpBranch>().WithParameters();
        _eJectMaster.Bind<PauseMenu>().To<IPauseBranch>().WithParameters();
        
        //NodeTweens
        _eJectMaster.Bind<Position>().To<IPosition>().WithParameters();
        _eJectMaster.Bind<Scale>().To<IScale>().WithParameters();
        _eJectMaster.Bind<Punch>().To<IPunch>().WithParameters();
        _eJectMaster.Bind<Shake>().To<IShake>().WithParameters();
        
        //BucketCreator
        _eJectMaster.Bind<BucketCreator>().To<IBucketCreator>();
        
        //HistoryTrackerClasses
        _eJectMaster.Bind<HistoryListManagement>().To<IHistoryManagement>().WithParameters();
        _eJectMaster.Bind<MoveBackInHistory>().To<IMoveBackInHistory>().WithParameters();
        _eJectMaster.Bind<ManagePopUpHistory>().To<IManagePopUpHistory>().WithParameters();
        _eJectMaster.Bind<NewSelectionProcess>().To<INewSelectionProcess>().WithParameters();
        _eJectMaster.Bind<PopUpController>().To<IPopUpController>();
        
        //NodeSearch
        _eJectMaster.Bind<NodeSearch>().To<INodeSearch>();
        
        //ScreenData
        _eJectMaster.Bind<ScreenData>().To<IScreenData>().WithParameters();
        
        //Input Classes
        _eJectMaster.Bind<MenuAndGameSwitching>().To<IMenuAndGameSwitching>().WithParameters();
        _eJectMaster.Bind<ChangeControl>().To<IChangeControl>().WithParameters();
    }
}