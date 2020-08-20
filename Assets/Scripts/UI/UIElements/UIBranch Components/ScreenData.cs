using System.Collections.Generic;
using UnityEngine;

public class ScreenData
{
    public ScreenData()
    {
        _uiDataEvents.SubscribeToHighlightedNode(SaveHighlighted);
        _uiDataEvents.SubscribeToSelectedNode(SaveSelected);
        _uiDataEvents.SubscribeToInMenu(SaveInMenu);
        _uiDataEvents.SubscribeToActiveBranch(SaveActiveBranch);
    }

    private readonly UIDataEvents _uiDataEvents = new UIDataEvents();
    public readonly List<UIBranch> _clearedBranches = new List<UIBranch>();
    public UINode _lastHighlighted;
    public UINode _lastSelected;
    public UIBranch _activeBranch;
    public bool  _wasInTheMenu;
    public bool _locked;

    private void SaveHighlighted(UINode newNode)
    {
        if (_locked) return;
        _lastHighlighted = newNode;
    }

    private void SaveSelected(UINode newNode)
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
            if (branchToClear.CanvasIsEnabled && branchToClear != thisBranch)
            {
                _clearedBranches.Add(branchToClear);
                if (blockRaycast) branchToClear._myCanvasGroup.blocksRaycasts = false;
            }
        }
    }
    
    public void RestoreScreen()
    {
        foreach (var branch in _clearedBranches)
        {
            branch.ActivateBranch();
        }
    }


}
