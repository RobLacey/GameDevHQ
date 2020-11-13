public class ResolvePopUp : BranchBase, IStartPopUp, IAddResolvePopUp
{
    public ResolvePopUp(UIBranch branch, UIBranch[] branchList) : base(branch)
    {
        _allBranches = branchList;
        _myBranch.OnStartPopUp += StartPopUp;
    }    

    //Variables
    private readonly UIBranch[] _allBranches;

    //Properties
    public UIBranch ThisPopUp => _myBranch;

    //Events
    private static CustomEvent<IAddResolvePopUp> AddResolvePopUp { get; } = new CustomEvent<IAddResolvePopUp>();
    
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
        _screenData.StoreClearScreenData(_allBranches, _myBranch, BlockRaycast.Yes);
        SetCanvas(ActiveCanvas.Yes);
        CanGoToFullscreen();
        AddResolvePopUp?.RaiseEvent(this);
    }
}
