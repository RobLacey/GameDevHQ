
using UnityEngine;

public class StandardBranchBase : BranchBase
{
    public StandardBranchBase(UIBranch branch) : base(branch) { }
    
    public override void SetUpBranch(UIBranch newParentController = null)
    {
        if (_myBranch._stayOn == IsActive.Yes && _myBranch.CanvasIsEnabled) 
            _myBranch._tweenOnChange = false;
        
        ActivateBranch();
        CanClearScreen();
        
        if (_myBranch._saveExitSelection == IsActive.No)
        {
            _myBranch.ResetBranchStartPosition();
        }

        SetNewParentBranch(newParentController);
    }

    protected override void MoveBackToThisBranch(UIBranch lastBranch)
    {
        if (lastBranch != _myBranch) return;
        
        base.MoveBackToThisBranch(lastBranch);
        
        if (_myBranch._stayOn == IsActive.Yes && _myBranch.CanvasIsEnabled) //TODO check works for internal
             _myBranch._tweenOnChange = false;
        
        _myBranch.MoveToThisBranch();
    }

    private void SetNewParentBranch(UIBranch newParentController) 
    {
        if(newParentController is null) return;
            _myBranch.MyParentBranch = newParentController;
    }
}
