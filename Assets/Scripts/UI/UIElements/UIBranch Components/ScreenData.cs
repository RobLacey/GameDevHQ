using System.Collections.Generic;
using UnityEngine;

public class ScreenData : IEventUser
{
    public ScreenData() => ObserveEvents();

    private readonly List<UIBranch> _clearedBranches = new List<UIBranch>();
    public bool  _wasInTheMenu, _locked;
    public bool _wasOnHomeScreen = true;

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

    public void StoreClearScreenData(UIBranch[] allBranches, UIBranch thisBranch, BlockRayCast blockRaycast)
    {
        _locked = true;
        StoreActiveBranches(allBranches, thisBranch, blockRaycast == BlockRayCast.Yes);
    }
    
    private void StoreActiveBranches(UIBranch[] allBranches, UIBranch thisBranch, bool blockRaycast)
    {
        foreach (var branchToClear in allBranches)
        {
            if(branchToClear == thisBranch) continue;
            
            if (branchToClear.CanvasIsEnabled)
                _clearedBranches.Add(branchToClear);
            
            if (blockRaycast) 
                branchToClear.MyCanvasGroup.blocksRaycasts = false;
        }
    }

    public void RestoreScreen()
    {
        if(_clearedBranches.Count == 0) return;
        
        foreach (var branch in _clearedBranches)
        {
            branch.Branch.ActivateBranchCanvas();
            branch.Branch.ActivateBlockRaycast();
        }
        _clearedBranches.Clear();
    }
}
