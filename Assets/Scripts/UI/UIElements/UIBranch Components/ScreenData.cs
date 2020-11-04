using System.Collections.Generic;

public class ScreenData : IEventUser
{
    public ScreenData(ScreenType screenType)
    {
        if (screenType == ScreenType.FullScreen)
            _isFullscreen = true;
        ObserveEvents();
    }

    private readonly List<UIBranch> _clearedBranches = new List<UIBranch>();
    public bool  _wasInTheMenu, _locked;
    public bool _wasOnHomeScreen = true;
    private readonly bool _isFullscreen;

    public void ObserveEvents()
    {
        EventLocator.Subscribe<IOnHomeScreen>(SaveOnHomeScreen, this);
        EventLocator.Subscribe<IInMenu>(SaveInMenu, this);
    }

    public void RemoveFromEvents()
    {
        EventLocator.Unsubscribe<IOnHomeScreen>(SaveOnHomeScreen);
        EventLocator.Unsubscribe<IInMenu>(SaveInMenu);
    }
    
    protected virtual void SaveInMenu(IInMenu args)
    {
        if (_locked) return;
        _wasInTheMenu = args.InTheMenu;
    }
    
    private void SaveOnHomeScreen(IOnHomeScreen args)
    {
        if (_locked) return;
        _wasOnHomeScreen = args.OnHomeScreen;
    }

    public void StoreClearScreenData(UIBranch[] allBranches, UIBranch thisBranch, BlockRaycast blockRaycast)
    {
        _locked = true;
        StoreActiveBranches(allBranches, thisBranch, blockRaycast == BlockRaycast.Yes);
    }
    
    private void StoreActiveBranches(UIBranch[] allBranches, UIBranch thisBranch, bool blockRaycast)
    {
        foreach (var branchToClear in allBranches)
        {
            if(branchToClear == thisBranch) continue;
            
            if (branchToClear.CanvasIsEnabled && !IsAPopUp(branchToClear))
                _clearedBranches.Add(branchToClear);
            
            if (blockRaycast) 
                branchToClear.Branch.SetBlockRaycast(BlockRaycast.No);
        }

        bool IsAPopUp(UIBranch branchToClear) => branchToClear.IsAPopUpBranch() || branchToClear.IsTimedPopUp;
    }

    public void RestoreScreen()
    {
        if(_clearedBranches.Count == 0) return;
        
        foreach (var branch in _clearedBranches)
        {
            if(_isFullscreen)
                branch.Branch.SetCanvas(ActiveCanvas.Yes);
            branch.Branch.SetBlockRaycast(BlockRaycast.Yes);
        }
        _clearedBranches.Clear();
    }
}
