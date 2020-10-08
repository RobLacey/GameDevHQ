
public class StandardBranchBase : BranchBase
{
    public StandardBranchBase(UIBranch branch) : base(branch) { }
    
    public override void SetUpBranch(UIBranch newParentController = null)
    {
        if (_myBranch.CanvasIsEnabled) 
            _myBranch.SetNoTween();
        
        ActivateBranchCanvas();
        CanGoToFullscreen();
        _myBranch.ResetBranchesStartPosition();
        SetNewParentBranch(newParentController);
    }

    public override void MoveBackToThisBranch(UIBranch lastBranch)
    {
        if (lastBranch != _myBranch) return;
        base.MoveBackToThisBranch(lastBranch);
        
        if (_myBranch.CanvasIsEnabled)
             _myBranch.SetNoTween();
        
        _myBranch.MoveToThisBranch();
    }

    private void SetNewParentBranch(UIBranch newParentController) 
    {
        if(newParentController is null) return;
        _myBranch.MyParentBranch = newParentController;
    }

}
