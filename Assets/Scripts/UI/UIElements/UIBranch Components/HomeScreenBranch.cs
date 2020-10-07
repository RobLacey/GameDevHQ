
public class HomeScreenBranchBase: BranchBase
{
    public HomeScreenBranchBase(UIBranch branch) : base(branch) { }
    
    //Properties
    private bool CannotTweenOnHome => _myBranch.TweenOnHome == IsActive.No;

    protected override void SaveInMenu(bool inMenu)
    {
        _inMenu = inMenu;
        ActivateBlockRaycast();
    }

    protected override void SaveOnStart() 
    {
        base.SaveOnStart();
        ActivateBranchCanvas();
    }
    
    //Main
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
        ActivateBranchCanvas();
    }

    public override void MoveBackToThisBranch(UIBranch lastBranch)
    {
        base.MoveBackToThisBranch(lastBranch);
        if (CannotTweenOnHome)
            _myBranch.SetNoTween();
        
        if(lastBranch != _myBranch)
            _myBranch.DontSetBranchAsActive();
        _myBranch.MoveToThisBranch();
        InvokeOnHomeScreen(_myBranch.IsHomeScreenBranch());
    }
}


