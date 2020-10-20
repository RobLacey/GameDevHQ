using System.Collections.Generic;

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
        EventLocator.SubscribeToEvent<ISelectedNode, INode>(SaveSelected, this);
        EventLocator.SubscribeToEvent<IActiveBranch, UIBranch>(SaveActiveBranch, this);
        EventLocator.SubscribeToEvent<IOnHomeScreen,bool>(SaveOnHomeScreen, this);
        EventLocator.SubscribeToEvent<IInMenu, bool>(SaveInMenu, this);
    }

    public void RemoveFromEvents()
    {
        EventLocator.UnsubscribeFromEvent<ISelectedNode, INode>(SaveSelected);
        EventLocator.UnsubscribeFromEvent<IActiveBranch, UIBranch>(SaveActiveBranch);
        EventLocator.UnsubscribeFromEvent<IOnHomeScreen, bool>(SaveOnHomeScreen);
        EventLocator.UnsubscribeFromEvent<IInMenu, bool>(SaveInMenu);
    }

    private void SaveSelected(INode newNode)
    {
        if (_locked) return;
        _lastSelected = newNode;
    }

    protected virtual void SaveInMenu(bool isInMenu)
    {
        if (_locked) return;
        _wasInTheMenu = isInMenu;
    }

    private void SaveActiveBranch(UIBranch newBranch)
    {
        if (_locked) return;
        _activeBranch = newBranch;
    }
    
    private void SaveOnHomeScreen(bool onHomeScreen)
    {
        if (_locked) return;
        _wasOnHomeScreen = onHomeScreen;
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
