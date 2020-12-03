﻿using System;
using System.Collections.Generic;
using System.Linq;

public interface ILinkedToggles : INodeBase { }

public class LinkedToggles : NodeBase, ILinkedToggles
{
    private readonly List<INode> _toggleGroupMembers = new List<INode>();
    private readonly ToggleGroup _groupID;
    private readonly UIBranch _tabBranch;
    private readonly bool _hasATabBranch;
    private int _hasAGroupStartPoint;
    private bool _isSelected;
    private IToggles _test;

    //Events
    private static event Action<INode, Action> SelectedToggle;
    
    public LinkedToggles(INode node) : base(node)
    {
        var toggleData = node.ToggleData;
        _groupID = toggleData.ReturnToggleId;
        _tabBranch = toggleData.ReturnTabBranch;
        _hasATabBranch = _tabBranch != null;
        SelectedToggle += SaveSelectedNode;
    }
    
    //Main
    public override void Start()
    {
        base.Start();
        SetUpToggleGroup(_uiNode.MyBranch.ThisGroupsUiNodes);
        SetUpTabBranch();
        if (_uiNode.ReturnStartAsSelected == IsActive.No) return;
        TurnOffOtherTogglesInGroup();
        _uiNode.SetNodeAsSelected_NoEffects();
         ActivateTabBranch();    
    }

    private void SetUpToggleGroup(IEnumerable<INode> thisGroupsUINodes)
    {
        foreach (var node in thisGroupsUINodes)
        {
            if (!node.IsToggleGroup) continue;
            if (_groupID != node.ToggleData.ReturnToggleId) continue;
            
            _toggleGroupMembers.Add(node);
            CheckIfIsStartNode(node);
        }
        CheckForStartPosition();
        _toggleGroupMembers.Remove(_uiNode);
    }

    private void SetUpTabBranch()
    {
        if (_hasATabBranch)
            _tabBranch.BranchBase.SetUpAsTabBranch();
    }

    private void ActivateTabBranch()
    {
        if (_hasATabBranch)
            TurnOnTab();
    }

    public override void DeactivateNode() { }

    private void CheckIfIsStartNode(IToggles node)
    {
        if (node.ReturnStartAsSelected == IsActive.Yes)
            _hasAGroupStartPoint++;
    }

    private void CheckForStartPosition()
    {
        if (_hasAGroupStartPoint == 0)
        {
            _toggleGroupMembers.First().SetStartAsSelected = IsActive.Yes;
        }        
        else if (_hasAGroupStartPoint > 1)
        {
            throw new Exception("To many start Point : " + _uiNode);
        }
    }

    private void SaveSelectedNode(INode newNode, Action callback)
    {
        if (!_toggleGroupMembers.Contains(newNode)) return;
        Deactivate(callback);
    }

    private void Deactivate(Action callback)
    {
        _uiNode.SetNodeAsNotSelected_NoEffects();
        if (!_hasATabBranch || !_isSelected) return;
        _tabBranch.StartBranchExitProcess(OutTweenType.Cancel, callback);
        _isSelected = false;
    }

    protected override void TurnNodeOnOff()
    {
        if (_uiNode.IsSelected) return;        
        TurnOffOtherTogglesInGroup();
        Activate();
    }

    private void Activate()
    {
        TurnOnTab();
        _uiNode.SetSelectedStatus(true, _uiNode.DoPress);
        _uiHistoryTrack.SetSelected(_uiNode);
    }

    private void TurnOffOtherTogglesInGroup()
    {
        SelectedToggle?.Invoke(_uiNode, TurnOnTab);
        _isSelected = true;
    }

    private void TurnOnTab()
    {
        if (!_hasATabBranch) return;
        _tabBranch.DontSetBranchAsActive();
        _tabBranch.MoveToThisBranch();
    }
}