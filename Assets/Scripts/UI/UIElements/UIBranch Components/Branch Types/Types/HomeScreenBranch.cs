
using UnityEngine;

public interface IHomeScreenBranch : IBranchBase { }

public class HomeScreenBranch: BranchBase, IHomeScreenBranch
{
    public HomeScreenBranch(IBranch branch) : base(branch)
    {
        SetCanvas(ActiveCanvas.Yes);
    }
    
    //Properties
    private bool CannotTweenOnHome => _myBranch.TweenOnHome == DoTween.DoNothing;
    private bool IsControlBar => _myBranch.IsControlBar();

    public override void ObserveEvents()
    {
        base.ObserveEvents();
        EVent.Do.Subscribe<ISetStartingCanvasOrder>(SetControlBarCanvasOrder);
    }

    private void SetControlBarCanvasOrder(ISetStartingCanvasOrder args)
    {
        if(!IsControlBar) return;
        
        var storedCondition = _myBranch.MyCanvas.enabled;
        _myBranch.MyCanvas.enabled = true;
        _myBranch.MyCanvas.overrideSorting = true;
        _myBranch.MyCanvas.sortingOrder = args.ReturnControlBarCanvasOrder();
        _myBranch.MyCanvas.enabled = storedCondition;
    }

    //Main
    protected override void SaveInMenu(IInMenu args)
    {
        base.SaveInMenu(args);
        SetBlockRaycast(BlockRaycast.Yes);
    }

    protected override void SaveOnStart(IOnStart args) 
    {
        base.SaveOnStart(args);
        SetBlockRaycast(BlockRaycast.Yes);
    }

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
        
        if(!_canStart || !_inMenu) return;
        
        if(!IsControlBar)
            _canvasOrderCalculator.SetCanvasOrder();
        
        SetCanvas(ActiveCanvas.Yes);
        
        if (CannotTweenOnHome && !OnHomeScreen)
            _myBranch.DoNotTween();
        
        if (OnHomeScreen && _myBranch.GetStayOn() == IsActive.Yes)
            _myBranch.DoNotTween();
        
        if(!OnHomeScreen)
            InvokeOnHomeScreen(true);
    }

    public override void SetBlockRaycast(BlockRaycast active)
    {
        if(_activeResolvePopUps) return;
        if(!_gameIsPaused)
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
        if(!_gameIsPaused)
        {
            base.SetCanvas(IsControlBar ? ActiveCanvas.Yes : active);
        }
        else
        {
            base.SetCanvas(active);
        }
    }

    protected override void ActivateStoredPosition()
    {
        if (MyScreenType != ScreenType.FullScreen && _myBranch.IsControlBar())
        {
            InvokeOnHomeScreen(true);
            return;
        }
        base.ActivateStoredPosition();
    }
}


