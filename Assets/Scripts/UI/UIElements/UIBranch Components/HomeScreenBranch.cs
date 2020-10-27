public class HomeScreenBranchBase: BranchBase
{
    public HomeScreenBranchBase(UIBranch branch) : base(branch) { }
    
    //Properties
    private bool CannotTweenOnHome => _myBranch.TweenOnHome == IsActive.No;
    
    //Main
    protected override void SaveInMenu(IInMenu args)
    {
        base.SaveInMenu(args);
        ActivateBlockRaycast();
    }

    protected override void SaveOnStart(IOnStart args) 
    {
        base.SaveOnStart(args);
        ActivateBranchCanvas();
        ActivateBlockRaycast();
    }
    
    //Main
    protected override void SetUpBranchesOnStart(ISetUpStartBranches args)
    {
        _myBranch.MyCanvas.enabled = true;
        _myBranch.MyCanvasGroup.blocksRaycasts = false;
        _myBranch.DefaultStartOnThisNode.ThisNodeIsSelected();        
        _myBranch.DefaultStartOnThisNode.ThisNodeIsHighLighted();

        if (args.StartBranch != _myBranch)
        {
            _myBranch.DontSetBranchAsActive();
        }
        _myBranch.MoveToThisBranch();
    }

    public override void SetUpBranch(UIBranch newParentController = null)
    {
        _myBranch.ResetBranchesStartPosition();
        if(!_canStart || !_inMenu) return;
        ActivateBranchCanvas();
    }

    public override void MoveBackToThisBranch(UIBranch lastBranch)
    {
        base.MoveBackToThisBranch(lastBranch);
        SetUpBranchReadyForMoveTo(lastBranch);
        _myBranch.MoveToThisBranch();
        InvokeOnHomeScreen(_myBranch.IsHomeScreenBranch());
    }

    private void SetUpBranchReadyForMoveTo(UIBranch lastBranch)
    {
        if (CannotTweenOnHome)
            _myBranch.SetNoTween();

        if (lastBranch != _myBranch)
            _myBranch.DontSetBranchAsActive();
    }

    public override void ActivateBlockRaycast()
    {
        if(_resolvePopUps) return;
        base.ActivateBlockRaycast();
    }
}


