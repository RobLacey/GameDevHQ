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
    private bool CannotTweenOnHome => _myBranch.TweenOnHome == IsActive.No && !_onHomeScreen;
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
        _myBranch.MyCanvas.enabled = true;
        _myBranch.MyCanvasGroup.blocksRaycasts = false;

        if (startBranch != _myBranch)
        {
            _myBranch.DontSetBranchAsActive();
        }
        _myBranch.MoveToThisBranch();
    }

    public override void SetUpBranch(UIBranch newParentController = null)
    {
        _myBranch.ResetBranchesStartPosition();
        if(!_canStart || !_inMenu) return;

        if (_onHomeScreen) _myBranch.SetNoTween();
        
        ActivateBranch();
    }

    protected override void MoveBackToThisBranch(UIBranch lastBranch)
    {
        if (lastBranch != _myBranch) return;
        
        base.MoveBackToThisBranch(lastBranch);
        if (CannotTweenOnHome)
            _myBranch.SetNoTween();
        
        _myBranch.MoveToThisBranch();
        InvokeOnHomeScreen(_myBranch.IsHomeScreenBranch());
    }

    
    private void ResetHomeScreenBranch()
    {
        if (_activeBranch == _myBranch) return;
        
        if (_myBranch.TweenOnHome == IsActive.No)
            _myBranch.SetNoTween();
        
        _myBranch.DontSetBranchAsActive();
        _myBranch.MoveToThisBranch();
    }
}


