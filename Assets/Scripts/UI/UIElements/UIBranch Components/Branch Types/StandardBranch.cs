public class StandardBranch : BranchBase
{
    public StandardBranch(UIBranch branch) : base(branch) { }

    protected override void SetUpBranchesOnStart(ISetUpStartBranches args)
    {
        SetBlockRaycast(BlockRaycast.No);
        if (_isTabBranch) return;
        SetCanvas(ActiveCanvas.No);
    }

    public override void SetUpBranch(UIBranch newParentController = null)
    {
        SetCanvas(ActiveCanvas.Yes);
        CanGoToFullscreen();
        _myBranch.ResetBranchesStartPosition();
        SetNewParentBranch(newParentController);
    }

    protected override void ClearBranchForFullscreen(IClearScreen args)
    {
        if(_isTabBranch) return;
        base.ClearBranchForFullscreen(args);
    }

    private void SetNewParentBranch(UIBranch newParentController) 
    {
        if(newParentController is null) return;
        _myBranch.MyParentBranch = newParentController;
    }

    public override void SetBlockRaycast(BlockRaycast active)
    {
        if(_resolvePopUps) return;
        base.SetBlockRaycast(active);
    }
}
