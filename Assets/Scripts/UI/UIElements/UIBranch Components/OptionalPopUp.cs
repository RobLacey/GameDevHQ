using System;
using UnityEngine;

public class OptionalPopUp : BranchBase, IStartPopUp
{
    public OptionalPopUp(UIBranch branch, UIBranch[] branchList) : base(branch)
    {
        _allBranches = branchList;
        _myBranch._onStartPopUp = StartPopUp;
        _uiPopUpEvents.SubscribeToNextNodeFromPopUp(RestoreLastPosition);
    }
    
    //Variables
    private readonly UIBranch[] _allBranches;
    private bool _restoreOnHome;

    //Events
    public static event Action<UIBranch> AddOptionalPopUp;
    public static event Action<UIBranch> RemoveOptionalPopUp;

    protected override void SaveIfOnHomeScreen(bool currentlyOnHomeScreen)
    {
        base.SaveIfOnHomeScreen(currentlyOnHomeScreen);

        if (!_restoreOnHome || !_onHomeScreen) return;
        
        if (_myBranch.TweenOnHome == IsActive.Yes)
        {
            _myBranch.DontSetBranchAsActive();
            _myBranch.MoveToThisBranch();
        }
        else
        {
            ActivateBranchCanvas();
            if(_noResolvePopUps) //Check Here
            {
                Debug.Log(_myBranch);

                ActivateBlockRaycast();
            }        }
    }

    public void StartPopUp() //TODO add to buffer goes here for when paused. trigger from SaveOnHome?
    {
        if (_gameIsPaused || !_onHomeScreen || !_canStart || _myBranch.CanvasIsEnabled) return; 
        
        IfActiveResolvePopUps();        
        _myBranch.MoveToThisBranch();
    }

    public override void SetUpBranch(UIBranch newParentController = null)
    {
        if(!_myBranch.CanvasIsEnabled && !_restoreOnHome) 
            AddOptionalPopUp?.Invoke(_myBranch);
            
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

    protected override void ClearBranchForFullscreen(UIBranch ignoreThisBranch = null)
    {
        if(!_myBranch.CanvasIsEnabled) return;
        base.ClearBranchForFullscreen(ignoreThisBranch);
        RemoveOrStorePopUp();
    }

    private void RemoveOrStorePopUp()
    {
        if (_myBranch.CanStoreAndRestoreOptionalPoUp)
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
