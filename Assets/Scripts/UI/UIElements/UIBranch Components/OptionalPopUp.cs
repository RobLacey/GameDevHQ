using System;
using UnityEngine;

public class OptionalPopUp : BranchBase, IStartPopUp
{
    public OptionalPopUp(UIBranch branch, UIBranch[] branchList) : base(branch)
    {
        _allBranches = branchList;
        _myBranch.OnStartPopUp = StartPopUp;
    }
    
    //Variables
    private readonly UIBranch[] _allBranches;
    private bool _restoreOnHome;

    //Events
    private static CustomEvent<IAddOptionalPopUp, UIBranch> AddOptionalPopUp { get; } 
        = new CustomEvent<IAddOptionalPopUp, UIBranch>();
    private static CustomEvent<IRemoveOptionalPopUp, UIBranch> RemoveOptionalPopUp { get; } 
        = new CustomEvent<IRemoveOptionalPopUp, UIBranch>();

    protected override void SaveIfOnHomeScreen(IOnHomeScreen args)
    {
        base.SaveIfOnHomeScreen(args);

        if (!_restoreOnHome || !OnHomeScreen) return;
        
        if (_myBranch.TweenOnHome == IsActive.Yes)
        {
            _myBranch.DontSetBranchAsActive();
            _myBranch.MoveToThisBranch();
        }
        else
        {
            ActivateBranchCanvas();
            if(_noResolvePopUps)
            {
                ActivateBlockRaycast();
            }        
        }
    }

    public void StartPopUp() //TODO add to buffer goes here for when paused. trigger from SaveOnHome?
    {
        if (_gameIsPaused || !OnHomeScreen || !_canStart || _myBranch.CanvasIsEnabled) return; 
        
        IfActiveResolvePopUps();        
        _myBranch.MoveToThisBranch();
    }

    public override void SetUpBranch(UIBranch newParentController = null)
    {
        if(!_myBranch.CanvasIsEnabled && !_restoreOnHome) 
            AddOptionalPopUp?.RaiseEvent(_myBranch);
            
        IfActiveResolvePopUps();
        ActivateBranchCanvas();
        _screenData.StoreClearScreenData(_allBranches, _myBranch, BlockRayCast.No);
        _restoreOnHome = false;
    }

    private void IfActiveResolvePopUps()
    {
        if (_noResolvePopUps) return;
        _myBranch.DontSetBranchAsActive();
        _myBranch.MyCanvasGroup.blocksRaycasts = false;
    }

    public override void MoveBackToThisBranch(UIBranch lastBranch)
    {
        if (lastBranch != _myBranch) return;
        
        base.MoveBackToThisBranch(lastBranch);
        _myBranch.MoveToThisBranch();
    }

    protected override void ClearBranchForFullscreen(IClearScreen args)
    {
        if(!_myBranch.CanvasIsEnabled) return;
        base.ClearBranchForFullscreen(args);
        RemoveOrStorePopUp();
    }

    public override void ActivateBlockRaycast()
    {
        if(!_noResolvePopUps) return;
        base.ActivateBlockRaycast();
    }

    private void RemoveOrStorePopUp()
    {
        if (_myBranch.CanStoreAndRestoreOptionalPoUp)
        {
            _restoreOnHome = true;
        }
        else
        {
            RemoveOptionalPopUp?.RaiseEvent(_myBranch);
        }
    }

    protected override void RestoreLastPosition((UIBranch nextPopUp, UIBranch currentPopUp) uiData)
    {
        base.RestoreLastPosition(uiData);
        if (_restoreOnHome) InvokeOnHomeScreen(true);
    }
}
