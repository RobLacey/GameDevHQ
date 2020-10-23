using System.Collections.Generic;
using UnityEngine;

public class ScreenData : IEventUser
{
    public ScreenData() => ObserveEvents();

    private readonly List<UIBranch> _clearedBranches = new List<UIBranch>();
    public INode _lastSelected;
    public UIBranch _activeBranch;
    public bool  _wasInTheMenu, _locked;
    public bool _wasOnHomeScreen = true;

    public void ObserveEvents()
    {
        EventLocator.Subscribe<ISelectedNode>(SaveSelected, this);
        EventLocator.Subscribe<IActiveBranch>(SaveActiveBranch, this);
        EventLocator.Subscribe<IOnHomeScreen>(SaveOnHomeScreen, this);
        EventLocator.Subscribe<IInMenu>(SaveInMenu, this);
    }

    public void RemoveFromEvents()
    {
        EventLocator.Unsubscribe<ISelectedNode>(SaveSelected);
        EventLocator.Unsubscribe<IActiveBranch>(SaveActiveBranch);
        EventLocator.Unsubscribe<IOnHomeScreen>(SaveOnHomeScreen);
        EventLocator.Unsubscribe<IInMenu>(SaveInMenu);
    }

    private void SaveSelected(ISelectedNode args)
    {
        if (_locked) return;
        _lastSelected = args.Selected;
    }

    protected virtual void SaveInMenu(IInMenu args)
    {
        if (_locked) return;
        _wasInTheMenu = args.InTheMenu;
    }

    private void SaveActiveBranch(IActiveBranch args)
    {
        if (_locked) return;
        _activeBranch = args.ActiveBranch;
    }
    
    private void SaveOnHomeScreen(IOnHomeScreen args)
    {
        if (_locked) return;
        _wasOnHomeScreen = args.OnHomeScreen;
    }

    public void StoreClearScreenData(UIBranch[] allBranches, UIBranch thisBranch, BlockRayCast blockRaycast)
    {
        _clearedBranches.Clear();
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
        foreach (var branch in _clearedBranches)
        {
            branch.Branch.ActivateBranchCanvas();
            branch.Branch.ActivateBlockRaycast();
        }
    }
}
