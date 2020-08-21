using System;
using UnityEngine;

public class OptionalPopUp : BranchBase
{
    public OptionalPopUp(UIBranch branch, UIBranch[] branchList) : base(branch)
    {
        _allBranches = branchList;
        _onStartPopUp = StartPopUp;
        _uiPopUpEvents.SubscribeToNextNodeFromPopUp(RestoreLastPosition);
    }
    
    //Variables
    private readonly UIBranch[] _allBranches;
    private readonly UIPopUpEvents _uiPopUpEvents = new UIPopUpEvents();
    private bool _restoreOnHome;
    
    //Properties
    private bool CanClearAndRestoreOnHome 
        => _myBranch._clearOrResetOptional == UIBranch.StoreAndRestorePopUps.StoreAndRestore;

    //Events
    public static event Action<UIBranch> AddOptionalPopUp;
    public static event Action<UIBranch> RemoveOptionalPopUp;

    protected override void SaveIfOnHomeScreen(bool currentlyOnHomeScreen)
    {
        base.SaveIfOnHomeScreen(currentlyOnHomeScreen);

        if (!_restoreOnHome || !_onHomeScreen) return;
        
        if (_myBranch._tweenOnHome == IsActive.Yes)
        {
            _myBranch._setAsActive = false;
            _myBranch.MoveToThisBranch();
        }
        else
        {
            ActivateBranch();
        }
    }

    private void StartPopUp()
    {
        if (_gameIsPaused || !_onHomeScreen) return; //TODO add to buffer goes here for when paused. trigger from SaveOnHome?

        if (_myBranch.CanvasIsEnabled) return;
        
        if (!_noResolvePopUps)
            _myBranch._setAsActive = false;
        
        _myBranch.MoveToThisBranch();
    }
    
    public override void SetUpBranch(UIBranch newParentController = null)
    {
        if(!_canStart) return;
        
        if(!_myBranch.CanvasIsEnabled && !_restoreOnHome) 
            AddOptionalPopUp?.Invoke(_myBranch);
        
        if (!_noResolvePopUps) 
            _myBranch._myCanvasGroup.blocksRaycasts = false;
        
        ActivateBranch();
        _screenData.StoreClearScreenData(_allBranches, _myBranch, BlockRayCast.No);
        _restoreOnHome = false;
    }

    protected override void MoveBackToThisBranch(UIBranch lastBranch)
    {
        if (lastBranch != _myBranch) return;
        
        base.MoveBackToThisBranch(lastBranch);
        _myBranch.MoveToThisBranch();
    }

    protected override void ClearBranch(UIBranch ignoreThisBranch = null)
    {
        if(!_onHomeScreen || !_myBranch.CanvasIsEnabled) return;
        
        base.ClearBranch(ignoreThisBranch);

        if (CanClearAndRestoreOnHome)
        {
            _restoreOnHome = true;
        }
        else
        {
            RemoveOptionalPopUp?.Invoke(_myBranch);
        }
    }

    private void RestoreLastPosition((UIBranch nextPopUp, UIBranch currentPopUp) uiData)
    {
        if (uiData.currentPopUp != _myBranch) return;
        
        if (_restoreOnHome) InvokeOnHomeScreen(true);
        
        ReturnToMenuOrGame(uiData);
    }
}
