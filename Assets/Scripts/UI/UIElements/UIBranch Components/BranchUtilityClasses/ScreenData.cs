using System.Collections.Generic;

public interface IScreenData
{
    bool WasOnHomeScreen { get; }
    void RemoveFromEvents();
    void RestoreScreen();
    void StoreClearScreenData(IBranch[] allBranches, IBranch thisBranch, BlockRaycast blockRaycast);
}

public class ScreenData : IEventUser, IScreenData
{
    public ScreenData(IBranchParams branch)
    {
        if (branch.MyScreenType == ScreenType.FullScreen)
            _isFullscreen = true;
        ObserveEvents();
    }

    private readonly List<IBranch> _clearedBranches = new List<IBranch>();
    private bool  _locked;
    private bool _wasOnHomeScreen = true;
    private readonly bool _isFullscreen;

    //Propertues
    public bool WasOnHomeScreen => _wasOnHomeScreen;

    
    //Main
    public void ObserveEvents() => EventLocator.Subscribe<IOnHomeScreen>(SaveOnHomeScreen, this);

    public void RemoveFromEvents() => EventLocator.Unsubscribe<IOnHomeScreen>(SaveOnHomeScreen);

    private void SaveOnHomeScreen(IOnHomeScreen args)
    {
        if (_locked) return;
        _wasOnHomeScreen = args.OnHomeScreen;
    }

    public void StoreClearScreenData(IBranch[] allBranches, IBranch thisBranch, BlockRaycast blockRaycast)
    {
        _locked = true;
        StoreActiveBranches(allBranches, thisBranch, blockRaycast == BlockRaycast.Yes);
    }
    
    private void StoreActiveBranches(IBranch[] allBranches, IBranch thisBranch, bool blockRaycast)
    {
        foreach (var branchToClear in allBranches)
        {
            if(branchToClear == thisBranch) continue;
            
            if (branchToClear.CanvasIsEnabled && !IsAPopUp(branchToClear))
                _clearedBranches.Add(branchToClear);
            
            if (blockRaycast) 
                branchToClear.BranchBase.SetBlockRaycast(BlockRaycast.No);
        }

        bool IsAPopUp(IBranch branchToClear) => branchToClear.IsAPopUpBranch() || branchToClear.IsTimedPopUp();
    }

    public void RestoreScreen()
    {
        if(_clearedBranches.Count == 0) return;
        
        foreach (var branch in _clearedBranches)
        {
            if(_isFullscreen)
                branch.BranchBase.SetCanvas(ActiveCanvas.Yes);
            branch.BranchBase.SetBlockRaycast(BlockRaycast.Yes);
        }
        _clearedBranches.Clear();
        _locked = false;
    }
}
