
public interface IHomeScreenBranch : IBranchBase { }

public class HomeScreenBranch: BranchBase, IHomeScreenBranch
{
    public HomeScreenBranch(IBranch branch) : base(branch) { }
    
    //Properties
    private bool CannotTweenOnHome => _myBranch.TweenOnHome == DoTween.DoNothing;
    private bool IsControlBar => _myBranch.IsControlBar();
    
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

    protected override void SaveIfOnHomeScreen(IOnHomeScreen args)
    {
        if (!OnHomeScreen && args.OnHomeScreen)
        {
            SetCanvas(ActiveCanvas.Yes);
            SetBlockRaycast(BlockRaycast.Yes);
        }
        base.SaveIfOnHomeScreen(args);
    }

    //Main
    protected override void SetUpBranchesOnStart(ISetUpStartBranches args)
    {
        SetCanvas(ActiveCanvas.Yes);
        SetBlockRaycast(BlockRaycast.No);

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
        _myBranch.SetHighlightedNode();
        
        if(!_canStart || !_inMenu) return;
        
        SetCanvas(ActiveCanvas.Yes);
        
        if (CannotTweenOnHome && !OnHomeScreen)
            _myBranch.DoNotTween();
        
        if (OnHomeScreen && _myBranch.GetStayOn() == IsActive.Yes)
            _myBranch.DoNotTween();
        
        InvokeOnHomeScreen(_myBranch.IsHomeScreenBranch());
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

    public override void ActivateStoredPosition()
    {
        if (MyScreenType != ScreenType.FullScreen && _myBranch.IsControlBar())
        {
            InvokeOnHomeScreen(true);
            return;
        }
        base.ActivateStoredPosition();
    }
}


