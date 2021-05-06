using UIElements;

public interface IHomeScreenBranch : IBranchBase { }

public class HomeScreenBranch: BranchBase, IHomeScreenBranch
{
    public HomeScreenBranch(IBranch branch) : base(branch)
    {
        SetCanvas(ActiveCanvas.Yes);
    }

    private ISetCanvasOrder _setCanvasOrder;
    
    //Properties
    private bool CannotTweenOnHome => _myBranch.TweenOnHome == DoTween.DoNothing;
    private bool IsControlBar => _myBranch.IsControlBar();

    //Main

    public override void UseEServLocator()
    {
        base.UseEServLocator();
        _setCanvasOrder = EServ.Locator.Get<ISetCanvasOrder>(this);
    }

    public override void OnStart()
    {
        base.OnStart();
        SetControlBarCanvasOrder();
    }

    private void SetControlBarCanvasOrder()
    {
        if(!IsControlBar) return;
        
        var storedCondition = _myBranch.MyCanvas.enabled;
        _myBranch.MyCanvas.enabled = true;
        _myBranch.MyCanvas.overrideSorting = true;
        _myBranch.MyCanvas.sortingOrder = _setCanvasOrder.ReturnControlBarCanvasOrder();
        _myBranch.MyCanvas.enabled = storedCondition;
    }

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


