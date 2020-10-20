using System;
using System.Collections.Generic;

public class UIToggles : IEventUser
{
    private readonly UINode _myNode;
    private readonly List<UINode> _toggleGroupMembers = new List<UINode>();
    private readonly ToggleGroup _groupID;
    private readonly UIBranch _tabBranch;
    private readonly bool _hasATabBranch;
    private bool _isSelected;

    //Events
    public static event Action<UINode> SelectedToggle;
    
    public UIToggles(UINode node, ToggleGroup groupID, UIBranch tabBranch)
    {
        ObserveEvents();
        _myNode = node;
        _groupID = groupID;
        _tabBranch = tabBranch;
        _hasATabBranch = tabBranch != null;
        SelectedToggle += SaveSelectedNode;
    }
    
    public void ObserveEvents()
    {
        EventLocator.SubscribeToEvent<IActiveBranch, UIBranch>(Activate, this);
    }

    public void RemoveFromEvents()
    {
        EventLocator.UnsubscribeFromEvent<IActiveBranch, UIBranch>(Activate);
    }


    private void Activate(UIBranch newBranch)
    {
        if (newBranch == _myNode.MyBranch && _isSelected)
            TurnOnTab();
    }

    public void SetUpToggleGroup(UINode[] thisGroupsUINodes)
    {
        foreach (var node in thisGroupsUINodes)
        {
            if (!node.IsToggleGroup || node == _myNode) continue;
            if (_groupID != node.ToggleGroupId) continue;
            _toggleGroupMembers.Add(node);
        }
    }

    private void SaveSelectedNode(UINode newNode)
    {
        if (!_toggleGroupMembers.Contains(newNode)) return;
        _myNode.SetNodeAsNotSelected_NoEffects();

        if (!_hasATabBranch || !_isSelected) return;
        _tabBranch.StartBranchExitProcess(OutTweenType.Cancel);
        _isSelected = false;

    }

    public void TurnOffOtherTogglesInGroup()
    {
        SelectedToggle?.Invoke(_myNode);
        _isSelected = true;
        TurnOnTab();
    }

    private void TurnOnTab()
    {
        if (!_hasATabBranch) return;
        _tabBranch.DontSetBranchAsActive();
        _tabBranch.MoveToThisBranch();
    }
}
