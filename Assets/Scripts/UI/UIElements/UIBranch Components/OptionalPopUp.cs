﻿
public class OptionalPopUp : BranchBase, IStartPopUp, IRemoveOptionalPopUp, IAddOptionalPopUp
{
    public OptionalPopUp(UIBranch branch, UIBranch[] branchList) : base(branch)
    {
        _allBranches = branchList;
        _myBranch.OnStartPopUp = StartPopUp;
    }
    
    //Variables
    private readonly UIBranch[] _allBranches;
    private bool _restoreOnHome;
    
    //Properties
    public UIBranch ThisPopUp => _myBranch;

    //Events
    private static CustomEvent<IAddOptionalPopUp> AddOptionalPopUp { get; } = new CustomEvent<IAddOptionalPopUp>();
    private static CustomEvent<IRemoveOptionalPopUp> RemoveOptionalPopUp { get; } 
        = new CustomEvent<IRemoveOptionalPopUp>();

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
            SetCanvas(ActiveCanvas.Yes);
            if(!_resolvePopUps)
            {
                SetBlockRaycast(BlockRaycast.Yes);
            }        
        }
    }

    public void StartPopUp() //TODO add to buffer goes here for when paused. trigger from SaveOnHome?
    {
        if (_gameIsPaused || !OnHomeScreen || !_canStart || _myBranch.CanvasIsEnabled || _resolvePopUps) return; 
        
        IfActiveResolvePopUps();        
        _myBranch.MoveToThisBranch();
    }

    public override void SetUpBranch(UIBranch newParentController = null)
    {
        if(!_myBranch.CanvasIsEnabled && !_restoreOnHome) 
            AddOptionalPopUp?.RaiseEvent(this);
        
        IfActiveResolvePopUps();
        SetCanvas(ActiveCanvas.Yes);
        _screenData.StoreClearScreenData(_allBranches, _myBranch, BlockRaycast.No);
        _restoreOnHome = false;
    }

    private void IfActiveResolvePopUps()
    {
        if (!_resolvePopUps) return;
        _myBranch.DontSetBranchAsActive();
        SetBlockRaycast(BlockRaycast.No);
    }

    protected override void ClearBranchForFullscreen(IClearScreen args)
    {
        if(!_myBranch.CanvasIsEnabled) return;
        base.ClearBranchForFullscreen(args);
        RemoveOrStorePopUp();
    }

    public override void SetBlockRaycast(BlockRaycast active)
    {
        if(_resolvePopUps) return;
        base.SetBlockRaycast(active);
    }

    private void RemoveOrStorePopUp()
    {
        if (_myBranch.CanStoreAndRestoreOptionalPoUp)
        {
            _restoreOnHome = true;
        }
        else
        {
            RemoveOptionalPopUp?.RaiseEvent(this);
        }
    }
}
