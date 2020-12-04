using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface ILinkedToggles : INodeBase { }

public class LinkedToggles : NodeBase, ILinkedToggles
{
    public LinkedToggles(INode node) : base(node)
    {
        var toggleData = node.ToggleData;
        _tabBranch = toggleData.ReturnTabBranch;
        _hasATabBranch = _tabBranch != null;
        SelectedToggle += SaveSelectedNode;
    }

    private List<INode> _toggleGroupMembers = new List<INode>();
    private readonly UIBranch _tabBranch;
    private readonly bool _hasATabBranch;
    private int _hasAGroupStartPoint;

    //Events
    private static event Action<INode, Action> SelectedToggle;

    //Main
    public override void Start()
    {
        base.Start();
        SetUpToggleGroup();
        SetUpTabBranch();
        if (_uiNode.ReturnStartAsSelected == IsActive.No)
        {
           SetAsNotActive();
        }
        else
        {
            SetNodeAsSelected_NoEffects();
            TurnOnTab();
        }
    }

    private void SetUpToggleGroup()
    {
        _toggleGroupMembers = _uiNode.ToggleGroupMembers;
        _hasAGroupStartPoint = _uiNode.HasAGroupStartPoint;
        CheckForStartPosition();
        _toggleGroupMembers.Remove(_uiNode);
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

    private void SetUpTabBranch()
    {
        if (_hasATabBranch)
            _tabBranch.BranchBase.SetUpAsTabBranch();
    }

    private void SaveSelectedNode(INode newNode, Action callback)
    {
        if (!_toggleGroupMembers.Contains(newNode)) return;
        if (!IsSelected) return;
        SetAsNotActive(callback);
    }

    protected override void TurnNodeOnOff()
    {
        if (IsSelected) return;        
        TurnOffOtherTogglesInGroup();
        Activate();
    }

    protected override void Activate()
    {
        TurnOnTab();
        SetSelectedStatus(true, DoPressOnNode);
        ThisNodeIsSelected();
    }

    private void SetAsNotActive(Action callback = null)
    {
        SetNodeAsNotSelected_NoEffects();
        if (!_hasATabBranch) return;
        _tabBranch.StartBranchExitProcess(OutTweenType.Cancel, callback);
    }

    private void TurnOffOtherTogglesInGroup() => SelectedToggle?.Invoke(_uiNode, TurnOnTab);

    private void TurnOnTab()
    {
        if (!_hasATabBranch) return;
        _tabBranch.DontSetBranchAsActive();
        _tabBranch.MoveToThisBranch();
    }
}
