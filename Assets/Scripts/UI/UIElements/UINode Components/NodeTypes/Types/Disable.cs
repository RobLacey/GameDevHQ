using System;

public class DisabledNode : IDisabledNode
{
    public DisabledNode(INode uiNode, NodeBase nodeBase)
    {
        ToThisDisabledNode = uiNode;
        _nodeBase = nodeBase;
        ThisIsDisabled = EVent.Do.Fetch<IDisabledNode>();
    }

    private bool _isDisabled;
    private readonly NodeBase _nodeBase;

    //Events
    private Action<IDisabledNode> ThisIsDisabled { get; }

    //Properties
    public INode ToThisDisabledNode { get; }

    //Main
    public bool IsDisabled
    {
        get => _isDisabled;
        set
        {
            _isDisabled = value;
            if (!_isDisabled) return;
            
            ThisIsDisabled?.Invoke(this);
            _nodeBase.SetSelectedStatus(false, _nodeBase.DoPressOnNode);
        }
    }

    public bool IsThisNodeIsDisabled()
    {
        if (!IsDisabled) return false;
        _nodeBase.Navigation.MoveToNextFreeNode();
        return true;
    }

}


