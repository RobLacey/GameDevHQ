using UnityEngine;

public class HomeScreenBranchBase: BranchBase
{
    public HomeScreenBranchBase(UIBranch branch) : base(branch)
    {
        _uiDataEvents.SubscribeToActiveBranch(SaveActiveBranch);
    }

    private UIBranch _activeBranch;

    private void SaveActiveBranch(UIBranch newBranch) => _activeBranch = newBranch;

    protected override void SaveInMenu(bool inMenu)
    {
        _inMenu = inMenu;
        ActivateBranch();
    }

    protected override void SaveOnStart()
    {
        base.SaveOnStart();
        ActivateBranch();
    }

    protected override void SaveIfOnHomeScreen(bool currentlyOnHomeScreen)
    {
        if(currentlyOnHomeScreen && _onHomeScreen) return;
        
        base.SaveIfOnHomeScreen(currentlyOnHomeScreen);
        
        if (_onHomeScreen)
        {
            ResetHomeScreenBranch();
        }
        else
        {
            ClearBranch();
        }
    }

    protected override void SetUpBranchesAt(UIBranch startBranch)
    {
        _myBranch._myCanvas.enabled = true;
        _myBranch._myCanvasGroup.blocksRaycasts = false;

        if (startBranch == _myBranch)
        {
            _myBranch.DefaultStartPosition.ThisNodeIsHighLighted();
            _myBranch.DefaultStartPosition.ThisNodeIsSelected();
            _myBranch.SetAsActiveBranch();
        }

        _myBranch._setAsActive = false;
        _myBranch.MoveToThisBranch();
    }

    public override void BasicSetUp(UIBranch newParentController = null)
    {
        ActivateBranch();
        
        if (_myBranch._saveExitSelection == IsActive.No)
        {
            _myBranch.ResetBranchStartPosition();
        }

        if(!_canStart || !_inMenu) return;
        
        if (_myBranch._stayOn == IsActive.Yes && _onHomeScreen) 
            _myBranch._tweenOnChange = false;

        if (_myBranch._tweenOnHome == IsActive.No && !_onHomeScreen)
        {
            _myBranch._tweenOnChange = false;
        }
        InvokeOnHomeScreen(_isHomeScreenBranch);
    }
    
    private void ResetHomeScreenBranch()
    {
        if (_activeBranch == _myBranch) return;
        
        if (_myBranch._tweenOnHome == IsActive.Yes)
                _myBranch.ActivateInTweens();
        
        _myBranch._myCanvas.enabled = true;
        _myBranch._myCanvasGroup.blocksRaycasts = true;
    }
}


