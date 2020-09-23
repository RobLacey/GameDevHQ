using System;
using System.Collections.Generic;
using UnityEngine;

public class UIToggles
{
    private readonly UINode _myNode;
    private readonly List<UINode> _toggleGroupMembers = new List<UINode>();
    private readonly ToggleGroup _groupID;
    private readonly Canvas _tab;
    private readonly bool _hasATab;

    public static event Action<UINode> SelectedToggle;

    public UIToggles(UINode node, ToggleGroup groupID, Canvas tab)
    {
        _myNode = node;
        _groupID = groupID;
        _tab = tab;
        _hasATab = _tab != null;
        SelectedToggle += SaveSelectedNode;
    }

    public void SetUpToggleGroup(UINode[] thisGroupsUINodes)
    {
        if(_hasATab) _tab.enabled = false;

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
        if(_hasATab) _tab.enabled = false;
    }

    public void TurnOffOtherTogglesInGroup()
    {
        SelectedToggle?.Invoke(_myNode);
        if(_hasATab) _tab.enabled = true;
    }
}
