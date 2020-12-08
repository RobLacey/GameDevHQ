
using System;
using UnityEngine;

public interface IStandard : INodeBase { }

public class Standard : NodeBase, IStandard
{
    public Standard(INode uiNode) : base(uiNode) => _uiNode = uiNode;

    private bool _isToggle;
    private Transform _myTransform, _originalParentTransform, _myChildTransform;


    public override void ObserveEvents()
    {
        base.ObserveEvents();
        EVent.Do.Subscribe<ICancelHoverOver>(OnCancelHooverOver);
    }

    public override void RemoveEvents()
    {
        base.RemoveEvents();
        EVent.Do.Unsubscribe<ICancelHoverOver>(OnCancelHooverOver);
    }

    public override void Start()
    {
        base.Start();
        CacheParentingTransforms();
    }
    
    private void CacheParentingTransforms()
    {
        if (MyBranch.AutoOpenClose == AutoOpenClose.No) return;
        _myTransform = _uiNode.MyBranch.ThisBranchesGameObject.transform;
        _originalParentTransform = _uiNode.HasChildBranch.ThisBranchesGameObject.transform.parent;
        _myChildTransform = _uiNode.HasChildBranch.ThisBranchesGameObject.transform;
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

    private void OnCancelHooverOver(ICancelHoverOver args)
    {
        if(_childTransformChanged)
            ParentChildToThisBranch();
    }

    public override void DeactivateNodeByType()
    {
        if (!IsSelected) return;
        Deactivate();
        SetNotHighlighted();
    }

    public override void SetSelectedStatus(bool isSelected, Action endAction)
    {
        base.SetSelectedStatus(isSelected, endAction);
        ParentChildToThisBranch();
    }

    private void ParentChildToThisBranch()
    {
        if(_uiNode.AutoOpenCloseOverride == IsActive.Yes || !MyBranch.CanAutoOpen()) return;
        
        if (IsSelected)
        {
            SetParentToThis(true, _myTransform);
        }
        else
        {
            SetParentToThis(false, _originalParentTransform);
        }
    }

    private void SetParentToThis(bool transformChanged, Transform newChildParent)
    {
        _childTransformChanged = transformChanged;
        _myChildTransform.parent = newChildParent;
    }

}
