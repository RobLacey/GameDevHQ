using System;
using UnityEngine;

public class ClassBindings : BindBase
{
    protected override void BindAllObjects()
    {
        if(CheckIfAlreadyBound()) return;

        //Base
        CreateClass.Bind<EJect>().To<IEJect>();
        
        //Hub Classes
        CreateClass.Bind<UIAudioManager>().To<IAudioService>().WithParameters();
        CreateClass.Bind<UIHomeGroup>().To<IHomeGroup>().WithParameters();
        CreateClass.Bind<HistoryTracker>().To<IHistoryTrack>().WithParameters();

        //Tweens
        CreateClass.Bind<PositionTween>().To<IPositionTween>();
        CreateClass.Bind<RotateTween>().To<IRotationTween>();
        CreateClass.Bind<ScaleTween>().To<IScaleTween>();
        CreateClass.Bind<FadeTween>().To<IFadeTween>();
        CreateClass.Bind<PunchTween>().To<IPunchTween>();
        CreateClass.Bind<ShakeTween>().To<IShakeTween>();
        
        //Node Types
        CreateClass.Bind<Standard>().To<IStandard>().WithParameters();
        CreateClass.Bind<CancelOrBackButton>().To<ICancelOrBack>().WithParameters();
        CreateClass.Bind<LinkedToggles>().To<ILinkedToggles>().WithParameters();
        CreateClass.Bind<ToggleNotLinked>().To<IToggleNotLinked>().WithParameters();
        CreateClass.Bind<HoverToActivate>().To<IHoverToActivate>().WithParameters();
        
        //Branch Types
        CreateClass.Bind<HomeScreenBranch>().To<IHomeScreenBranch>().WithParameters();
        CreateClass.Bind<StandardBranch>().To<IStandardBranch>().WithParameters();
        CreateClass.Bind<ResolvePopUp>().To<IResolvePopUpBranch>().WithParameters();
        CreateClass.Bind<OptionalPopUpPopUp>().To<IOptionalPopUpBranch>().WithParameters();
        CreateClass.Bind<TimedPopUp>().To<ITimedPopUpBranch>().WithParameters();
        CreateClass.Bind<PauseMenu>().To<IPauseBranch>().WithParameters();
        
        //NodeTweens
        CreateClass.Bind<Position>().To<IPosition>().WithParameters();
        CreateClass.Bind<Scale>().To<IScale>().WithParameters();
        CreateClass.Bind<Punch>().To<IPunch>().WithParameters();
        CreateClass.Bind<Shake>().To<IShake>().WithParameters();
        
        //BucketCreator
        CreateClass.Bind<BucketCreator>().To<IBucketCreator>();
        
        //HistoryTrackerClasses
        CreateClass.Bind<HistoryListManagement>().To<IHistoryManagement>().WithParameters();
        CreateClass.Bind<MoveBackInHistory>().To<IMoveBackInHistory>().WithParameters();
        CreateClass.Bind<ManagePopUpHistory>().To<IManagePopUpHistory>().WithParameters();
        CreateClass.Bind<NewSelectionProcess>().To<INewSelectionProcess>().WithParameters();
        CreateClass.Bind<PopUpController>().To<IPopUpController>();
        
        //NodeSearch
        CreateClass.Bind<NodeSearch>().To<INodeSearch>();
        
        //ScreenData
        CreateClass.Bind<ScreenData>().To<IScreenData>().WithParameters();
        
        //Input Classes
        CreateClass.Bind<MenuAndGameSwitching>().To<IMenuAndGameSwitching>().WithParameters();
        CreateClass.Bind<ChangeControl>().To<IChangeControl>().WithParameters();
    }
}