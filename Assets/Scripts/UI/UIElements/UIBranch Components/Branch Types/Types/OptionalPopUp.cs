using System;

public interface IOptionalPopUpBranch : IBranchBase { } 

public class OptionalPopUpPopUp : BranchBase, IStartPopUp, IRemoveOptionalPopUp, IAddOptionalPopUp, IOptionalPopUpBranch
{
    public OptionalPopUpPopUp(IBranch branch) : base(branch)
    {
        _allBranches = branch.FindAllBranches();
        _myBranch.OnStartPopUp += StartPopUp;
    }
    
    //Variables
    private readonly IBranch[] _allBranches;
    private bool _restoreOnHome;
    
    //Properties
    public IBranch ThisPopUp => _myBranch;

    //Events
    private Action<IAddOptionalPopUp> AddOptionalPopUp { get; set; }
    private Action<IRemoveOptionalPopUp> RemoveOptionalPopUp { get; set; }


    public override void FetchEvents()
    {
        base.FetchEvents();
        AddOptionalPopUp = EVent.Do.Fetch<IAddOptionalPopUp>();
        RemoveOptionalPopUp = EVent.Do.Fetch<IRemoveOptionalPopUp>();
    }

    protected override void SaveIfOnHomeScreen(IOnHomeScreen args)
    {
        base.SaveIfOnHomeScreen(args);

        if (!_restoreOnHome || !OnHomeScreen) return;
        
        if (_myBranch.TweenOnHome == DoTween.Tween)
        {
            ActivateWithTween();
        }
        else
        {
            ActivateWithoutTween();
        }
    }

    private void ActivateWithTween()
    {
        _myBranch.DontSetBranchAsActive();
        _myBranch.MoveToThisBranch();
    }

    private void ActivateWithoutTween()
    {
        SetCanvas(ActiveCanvas.Yes);
        if (!_resolvePopUps)
            SetBlockRaycast(BlockRaycast.Yes);
    }

    public void StartPopUp() //TODO add to buffer goes here for when paused. trigger from SaveOnHome?
    {
        if (_gameIsPaused || !OnHomeScreen || !_canStart || _myBranch.CanvasIsEnabled || _resolvePopUps) return; 
        
        IfActiveResolvePopUps();        
        _myBranch.MoveToThisBranch();
    }

    public override void SetUpBranch(IBranch newParentController = null)
    {
        if(!_myBranch.CanvasIsEnabled && !_restoreOnHome) 
            AddOptionalPopUp?.Invoke(this);
        
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
            RemoveOptionalPopUp?.Invoke(this);
        }
    }
}
