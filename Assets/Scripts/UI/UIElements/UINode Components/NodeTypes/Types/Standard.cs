
using UnityEngine;

public interface IStandard : INodeBase { }

public class Standard : NodeBase, IStandard
{
    public Standard(INode uiNode) : base(uiNode) => _uiNode = uiNode;

    private bool _childTransformChanged;
    
    public override void OnEnter(bool isDragEvent)
    {
        base.OnEnter(isDragEvent);
        if (MyBranch.OpenHooverOnEnter && !IsSelected)
        {
            TurnNodeOnOff();
        }
    }

    protected override void OnSelected(bool isDragEvent)
    {
        base.OnSelected(isDragEvent);
        ActivateChild();
    }

    private void ActivateChild()
    {
        if (MyBranch.MyParentBranch.OpenHooverOnEnter)
        {
            if (IsSelected)
            {
                _childTransformChanged = true;
                _myChildsTransform.parent = _myTransform;
            }
            else
            {
                _childTransformChanged = false;
                _myChildsTransform.parent = _originalParentTransform;
            }
        }
    }

    protected override void OnCancelHooverOver(ICancelHoverOver args)
    {
        if(_childTransformChanged)
            ActivateChild();
    }

    public override void DeactivateNodeByType()
    {
        if (!IsSelected) return;
        Deactivate();
        SetNotHighlighted();
    }

}
