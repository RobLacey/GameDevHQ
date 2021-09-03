using EZ.Service;
using UIElements;
using UnityEngine;

public interface IHomeScreenBranch : IBranchBase { }

public class HomeScreenBranch: BranchBase, IHomeScreenBranch
{
    public HomeScreenBranch(IBranch branch) : base(branch) { }

    private ICanvasOrderData _canvasOrderData;
    private bool _justReturnedHome = false;
    
    //Properties
    private bool CannotTweenOnHome => _myBranch.TweenOnHome == DoTween.DoNothing 
                                      && (!_justReturnedHome && _myBranch.GetStayOn() == IsActive.Yes);
    private bool IsControlBar => _myBranch.IsControlBar();

    protected override void SaveIfOnHomeScreen(IOnHomeScreen args)
    {
        base.SaveIfOnHomeScreen(args);
        _justReturnedHome = true;
    }

    //Main
    public override void UseEZServiceLocator()
    {
        base.UseEZServiceLocator();
        _canvasOrderData = EZService.Locator.Get<ICanvasOrderData>(this);
    }

    public override void ObserveEvents()
    {
        base.ObserveEvents();
        HistoryEvents.Do.Subscribe<IOnStart>(SetUpOnStart);
    }

    public override void UnObserveEvents()
    {
        base.UnObserveEvents();
        HistoryEvents.Do.Unsubscribe<IOnStart>(SetUpOnStart);
    }

    public override void OnStart()
    {
        base.OnStart();
        SetCanvas(ActiveCanvas.Yes);
        SetControlBarCanvasOrder();
    }

    private void SetControlBarCanvasOrder()
    {
        if(!IsControlBar) return;
        
        var storedCondition = _myBranch.MyCanvas.enabled;
        _myBranch.MyCanvas.enabled = true;
        _myBranch.MyCanvas.overrideSorting = true;
        _myBranch.MyCanvas.sortingOrder = _canvasOrderData.ReturnControlBarCanvasOrder();
        _myBranch.MyCanvas.enabled = storedCondition;
    }

    protected override void SaveInMenu(IInMenu args)
    {
        base.SaveInMenu(args);
        SetBlockRaycast(BlockRaycast.Yes);
    }

    private void SetUpOnStart(IOnStart args) => SetBlockRaycast(BlockRaycast.Yes);

    //Main
    protected override void SetUpBranchesOnStart(ISetUpStartBranches args)
    {
        if (args.StartBranch == _myBranch)
        {
            _myBranch.DefaultStartOnThisNode.ThisNodeIsHighLighted();
        }
        else
        {
            _myBranch.DontSetBranchAsActive();
        }
        _myBranch.MoveToThisBranch();
    }

    public override void SetUpBranch(IBranch newParentController = null)
    {
        base.SetUpBranch(newParentController);
        
        if(!CanStart || !InMenu) return;
        
        if(!IsControlBar)
            _canvasOrderCalculator.SetCanvasOrder();
        
        if (CannotTweenOnHome || IsControlBar || _myBranch.CanvasIsEnabled)
            _myBranch.DoNotTween();
        
        SetCanvas(ActiveCanvas.Yes);
        
        if(!OnHomeScreen)
            InvokeOnHomeScreen(true);
        _justReturnedHome = false;
    }

    public override void EndOfBranchExit()
    {
        base.EndOfBranchExit();
        CheckChildOfControlBar();
    }

    private void CheckChildOfControlBar()
    {
        if (_myBranch.LastSelected.HasChildBranch.ScreenType == ScreenType.Overlay && !OnHomeScreen)
        {
            _historyTrack.BackToHomeScreen();
        }
    }

    public override void SetBlockRaycast(BlockRaycast active)
    {
        if(!GameIsPaused && NoResolvePopUps)
        {
            base.SetBlockRaycast(IsControlBar ? BlockRaycast.Yes: active);
        }
        else
        {
            base.SetBlockRaycast(active);
        }
    }
    
    public override void SetCanvas(ActiveCanvas active)
    {
        if(!GameIsPaused && NoResolvePopUps)
        {
            base.SetCanvas(IsControlBar ? ActiveCanvas.Yes : active);
        }
        else
        {
            base.SetCanvas(active);
        }
    }
}


