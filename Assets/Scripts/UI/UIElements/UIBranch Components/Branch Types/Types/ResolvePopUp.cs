public interface IResolvePopUpBranch : IBranchBase { }

public class ResolvePopUp : BranchBase, IStartPopUp, IAddResolvePopUp, IResolvePopUpBranch
{
    public ResolvePopUp(IBranch branch) : base(branch)
    {
        _allBranches = branch.FindAllBranches();
        _myBranch.OnStartPopUp += StartPopUp;
    }    

    //Variables
    private readonly IBranch[] _allBranches;

    //Properties
    public IBranch ThisPopUp => _myBranch;

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
    
    public override void SetUpBranch(IBranch newParentController = null)
    {
        if(_myBranch.CanvasIsEnabled) return;
        _screenData.StoreClearScreenData(_allBranches, _myBranch.ThisBranch, BlockRaycast.Yes);
        SetCanvas(ActiveCanvas.Yes);
        CanGoToFullscreen();
        AddResolvePopUp?.RaiseEvent(this);
    }
}
