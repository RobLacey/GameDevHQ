
public class StandardBranchBase : BranchBase
{
    public StandardBranchBase(UIBranch branch) : base(branch) { }
    
    public override void SetUpBranch(UIBranch newParentController = null)
    {
        if (_myBranch._stayOn == IsActive.Yes && _myBranch.CanvasIsEnabled) 
            _myBranch._tweenOnChange = false;
        
        ActivateBranch();
        CanClearOrRestoreScreen();
        
        if (_myBranch._saveExitSelection == IsActive.No)
        {
            _myBranch.ResetBranchStartPosition();
        }

        SetNewParentBranch(newParentController);
    }

    private void SetNewParentBranch(UIBranch newParentController) 
    {
        if(newParentController is null) return;
            _myBranch.MyParentBranch = newParentController;
    }

}
