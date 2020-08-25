
public class StandardBranchBase : BranchBase
{
    public StandardBranchBase(UIBranch branch) : base(branch) { }
    
    public override void SetUpBranch(UIBranch newParentController = null)
    {
        if (_myBranch.CanvasIsEnabled) 
            _myBranch.SetNoTween();
        
        ActivateBranch();
        CanGoToFullscreen();
        _myBranch.ResetBranchesStartPosition();
        _myBranch.SetNewParentBranch(newParentController);
    }

    protected override void MoveBackToThisBranch(UIBranch lastBranch)
    {
        if (lastBranch != _myBranch) return;
        
        base.MoveBackToThisBranch(lastBranch);
        
        if (_myBranch.CanvasIsEnabled)
             _myBranch.SetNoTween();
        
        _myBranch.MoveToThisBranch();
    }
}
