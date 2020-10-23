public class ResolvePopUp : BranchBase, IStartPopUp
{
    public ResolvePopUp(UIBranch branch, UIBranch[] branchList) : base(branch)
    {
        _allBranches = branchList;
        _myBranch.OnStartPopUp = StartPopUp;
    }    

    //Variables
    private readonly UIBranch[] _allBranches;

    //Events
    private static CustomEvent<IAddResolvePopUp, UIBranch> AddResolvePopUp { get; } 
        = new CustomEvent<IAddResolvePopUp, UIBranch>();
    
    //Main
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
        
        _screenData.StoreClearScreenData(_allBranches, _myBranch, BlockRayCast.Yes);
        ActivateBranchCanvas();
        CanGoToFullscreen();
        AddResolvePopUp?.RaiseEvent(_myBranch);
    }

    public override void MoveBackToThisBranch(UIBranch lastBranch)
    {
        if (lastBranch != _myBranch) return;
        
        base.MoveBackToThisBranch(lastBranch);
        _myBranch.MoveToThisBranch();
    }

    protected override void RestoreLastPosition((UIBranch nextPopUp, UIBranch currentPopUp) data)
    {
        base.RestoreLastPosition(data);
        ActivateStoredPosition();
    }
}
