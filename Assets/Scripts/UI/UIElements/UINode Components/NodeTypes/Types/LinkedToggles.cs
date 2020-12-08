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
        _allNodes = _uiNode.MyBranch.ThisGroupsUiNodes.ToList();
        _myToggleGroupId = toggleData.ReturnToggleId;
        _startAsSelected = toggleData.StartAsSelected == IsActive.Yes;
        SelectedToggle += SaveSelectedNode;
    }

    //Variables
    private readonly List<INode> _allNodes;
    private readonly List<INode> _toggleGroupMembers = new List<INode>();
    private readonly UIBranch _tabBranch;
    private readonly bool _hasATabBranch;
    private int _hasAGroupStartPoint;
    private readonly ToggleGroup _myToggleGroupId;
    private bool _startAsSelected;

    //Events
    private static event Action<INode, Action> SelectedToggle;

    //Main
    public override void Start()
    {
        base.Start();
        SetUpToggleGroup();
        SetUpTabBranch();
        if (!_startAsSelected)
        {
           SetAsNotActive();
        }
        else
        {
            SetNodeAsSelected_NoEffects();
            TurnOnTab();
        }

        _childTransformChanged = false;
    }
    
    private void SetUpToggleGroup()
    {
        foreach (var node in _allNodes.Where(node => node.IsToggleGroup)
                                      .Where(node => _myToggleGroupId == node.ToggleData.ReturnToggleId))
        {
            _toggleGroupMembers.Add(node);
            CheckForStartPosition(node);
        }

        AssignStartIfNonExists();
        _toggleGroupMembers.Remove(_uiNode);
    }

    private void AssignStartIfNonExists()
    {
        if (_hasAGroupStartPoint != 0) return;
        _toggleGroupMembers.First().ToggleData.SetStartAsSelected();
        _startAsSelected = true;
    }

    private void CheckForStartPosition(INode node)
    {
        if (node.ToggleData.StartAsSelected == IsActive.Yes)
        {
            _hasAGroupStartPoint++;
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
    
    public override void OnEnter(bool isDragEvent)
    {
        base.OnEnter(isDragEvent);
        if(_uiNode.AutoOpenCloseOverride == IsActive.Yes) return;
        
        if (MyBranch.CanAutoOpen() && !IsSelected)
        {
            TurnNodeOnOff();
        }
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
