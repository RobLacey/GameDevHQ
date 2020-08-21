using UnityEngine;

public class HomeScreenBranchBase: BranchBase
{
    public HomeScreenBranchBase(UIBranch branch) : base(branch)
    {
        _uiDataEvents.SubscribeToActiveBranch(SaveActiveBranch);
    }

    //Variables
    private UIBranch _activeBranch;
    private bool _canActivate;
    
    //Properties
    private bool CannotTweenOnHome => _myBranch._tweenOnHome == IsActive.No && !_onHomeScreen;
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
    
    //Main
    protected override void SaveIfOnHomeScreen(bool currentlyOnHomeScreen)
    {
        if(currentlyOnHomeScreen && _onHomeScreen) return;
        
        base.SaveIfOnHomeScreen(currentlyOnHomeScreen);
        
        if (_onHomeScreen)
            ResetHomeScreenBranch();
    }

    protected override void SetUpBranchesOnStart(UIBranch startBranch)
    {
        _myBranch._myCanvas.enabled = true;
        _myBranch._myCanvasGroup.blocksRaycasts = false;

        if (startBranch == _myBranch)
        {
            SetBranchAsStartPoistion();
        }
        _myBranch._setAsActive = false;
        _myBranch.MoveToThisBranch();
    }

    private void SetBranchAsStartPoistion()
    {
        _myBranch.DefaultStartPosition.ThisNodeIsHighLighted();
        _myBranch.DefaultStartPosition.ThisNodeIsSelected();
        _myBranch.SetAsActiveBranch();
    }

    public override void SetUpBranch(UIBranch newParentController = null)
    {
        if (_myBranch._saveExitSelection == IsActive.No)
            _myBranch.ResetBranchStartPosition();

        if(!_canStart || !_inMenu) return;
        
        ActivateBranch();
    }

    protected override void MoveBackToThisBranch(UIBranch lastBranch)
    {
        if (lastBranch != _myBranch) return;

        base.MoveBackToThisBranch(lastBranch);

        if (CannotTweenOnHome)
            _myBranch._tweenOnChange = false;

        _canActivate = true;
        InvokeOnHomeScreen(_isHomeScreenBranch);
    }

    
    private void ResetHomeScreenBranch()
    {
        if (_activeBranch == _myBranch) return;
        
        if (_myBranch._tweenOnHome == IsActive.No)
            _myBranch._tweenOnChange = false;
        
        _myBranch._setAsActive = _canActivate;
        
        if (!_noActivePopUps) _myBranch._setAsActive = false;
        
        _myBranch.MoveToThisBranch();
        _canActivate = false;
    }
}


