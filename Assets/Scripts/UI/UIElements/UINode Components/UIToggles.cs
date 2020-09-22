using System.Collections.Generic;

public class UIToggles
{
    private readonly UINode _myNode;
    private readonly List<UINode> _toggleGroupMembers = new List<UINode>();
    private readonly ToggleGroup _groupID;

    public UIToggles(UINode node, ToggleGroup groupID)
    {
        _myNode = node;
        _groupID = groupID;
    }

    public void SetUpToggleGroup(UINode[] thisGroupsUINodes)
    {
        foreach (var node in thisGroupsUINodes)
        {
            if (!node.IsToggleGroup || node == _myNode) continue;
            if (_groupID == node.ToggleGroupId)
            {
                _toggleGroupMembers.Add(node);
            }
        }
    }

    public void TurnOffOtherTogglesInGroup()
    {
        foreach (var item in _toggleGroupMembers)
        {
            item.SetNodeAsNotSelected_NoEffects();
        }
    }
}
