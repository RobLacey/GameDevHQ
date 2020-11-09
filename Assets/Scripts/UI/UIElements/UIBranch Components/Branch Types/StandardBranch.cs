public class StandardBranchBase : BranchBase
{
    public StandardBranchBase(UIBranch branch) : base(branch) { }

    public override void SetUpBranch(UIBranch newParentController = null)
    {
        if (_myBranch.CanvasIsEnabled) 
            _myBranch.SetNoTween();
        
        SetCanvas(ActiveCanvas.Yes);
        CanGoToFullscreen();
        _myBranch.ResetBranchesStartPosition();
        SetNewParentBranch(newParentController);
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
