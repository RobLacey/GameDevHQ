public class HomeScreenBranch: BranchBase
{
    private bool _startBranch;
    public HomeScreenBranch(UIBranch branch) : base(branch) { }
    
    //Properties
    private bool CannotTweenOnHome => _myBranch.TweenOnHome == IsActive.No;
    
    //Main
    protected override void SaveInMenu(IInMenu args)
    {
        base.SaveInMenu(args);
        SetBlockRaycast(BlockRaycast.Yes);
    }

    protected override void SaveOnStart(IOnStart args) 
    {
        base.SaveOnStart(args);
        if (_startBranch)
        {
            _myBranch.MoveToBranchWithoutTween();
        }
        SetBlockRaycast(BlockRaycast.Yes);
    }

    protected override void SaveIfOnHomeScreen(IOnHomeScreen args)
    {
        base.SaveIfOnHomeScreen(args);
        if (!OnHomeScreen || _myBranch.CanvasIsEnabled) return;
        
        SetCanvas(ActiveCanvas.Yes);
        SetBlockRaycast(BlockRaycast.Yes);
    }

    //Main
    protected override void SetUpBranchesOnStart(ISetUpStartBranches args)
    {
        SetCanvas(ActiveCanvas.Yes);
        SetBlockRaycast(BlockRaycast.No);

        if (args.StartBranch != _myBranch)
        {
            _myBranch.DontSetBranchAsActive();
        }
        else
        {
            _startBranch = true;
            _myBranch.DefaultStartOnThisNode.ReturnNode.ThisNodeIsHighLighted();
        }

        _myBranch.MoveToThisBranch();
    }

    public override void SetUpBranch(UIBranch newParentController = null)
    {
        _myBranch.ResetBranchesStartPosition();
        if(!_canStart || !_inMenu) return;
        SetCanvas(ActiveCanvas.Yes);
        if (CannotTweenOnHome)
            _myBranch.SetNoTween();
        InvokeOnHomeScreen(_myBranch.IsHomeScreenBranch());
    }

    public override void SetBlockRaycast(BlockRaycast active)
    {
        if(_resolvePopUps) return;
        base.SetBlockRaycast(active);
    }
}


