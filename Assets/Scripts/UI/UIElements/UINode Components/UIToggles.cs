using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIToggles
{
    UINode _myNode;
    List<UINode> _toggleGroupMembers = new List<UINode>();
    ButtonFunction _myFunction;
    bool _startAsSelected;

    public UIToggles(UINode node, ButtonFunction function, bool startSelected)
    {
        _myNode = node;
        _myFunction = function;
        _startAsSelected = startSelected;
    }

    public void SetUpToggleGroup(UINode[] thisGroupsUINodes)
    {
        if (_myFunction != ButtonFunction.ToggleGroup) return;

        foreach (var node in thisGroupsUINodes)
        {
            if (node.Function == ButtonFunction.ToggleGroup)
            {
                if (node != _myNode && _myNode.ID == node.ID)
                {
                    _toggleGroupMembers.Add(node);
                }

            }
        }
        if (_startAsSelected)
        {
            _myNode.SetSelected_NoEffects();
            //_myNode.IsSelected = true;
            // _myNode.SetNotHighlighted();
        }
    }

    public void ToggleGroupElements()
    {
        if (_myFunction == ButtonFunction.ToggleGroup)
        {
            foreach (var item in _toggleGroupMembers)
            {
                item.IsSelected = false;
                item.SetNotHighlighted();
            }
        }
    }

}
