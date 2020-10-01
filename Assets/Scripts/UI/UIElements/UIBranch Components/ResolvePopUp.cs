using System;

public class ResolvePopUp : BranchBase, IStartPopUp
{
    public ResolvePopUp(UIBranch branch, UIBranch[] branchList) : base(branch)
    {
        _allBranches = branchList;
        _myBranch._onStartPopUp = StartPopUp;
        _uiPopUpEvents.SubscribeToNextNodeFromPopUp(RestoreLastPosition);
    }    

    //Variables
    private readonly UIBranch[] _allBranches;

    //Events
    public static event Action<UIBranch> AddResolvePopUp;

    public void StartPopUp()
    {
        if(!_canStart) return;
        if (_gameIsPaused) return; //TODO add to buffer goes here for when paused. trigger from SaveOnHome?

        if (!_myBranch.CanvasIsEnabled)
            _myBranch.MoveToThisBranch();
    }
    
    public override void SetUpBranch(UIBranch newParentController = null)
    {
        if(_myBranch.CanvasIsEnabled) return;
        
        ActivateBranchCanvas();
        _screenData.StoreClearScreenData(_allBranches, _myBranch, BlockRayCast.Yes);
        AddResolvePopUp?.Invoke(_myBranch);
    }

    public override void MoveBackToThisBranch(UIBranch lastBranch)
    {
        if (lastBranch != _myBranch) return;
        
        base.MoveBackToThisBranch(lastBranch);
        _myBranch.MoveToThisBranch();
    }
    
    private void RestoreLastPosition((UIBranch nextPopUp, UIBranch currentPopUp) data)
    {
        if (data.currentPopUp != _myBranch) return;
        
        _screenData.RestoreScreen();
        ReturnToMenuOrGame(data);
    }
}
