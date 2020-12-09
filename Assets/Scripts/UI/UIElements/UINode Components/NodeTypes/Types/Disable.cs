using System;

public class DisabledNode : IDisabledNode
{
    public DisabledNode(IDisableData nodeBase)
    {
        ToThisDisabledNode = nodeBase.UINode;
        _nodeBase = nodeBase;
        ThisIsDisabled = EVent.Do.Fetch<IDisabledNode>();
    }

    private bool _isDisabled;
    private readonly IDisableData _nodeBase;

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
            _nodeBase.SetNodeAsNotSelected_NoEffects();
        }
    }

    public bool IsThisNodeIsDisabled()
    {
        if (!IsDisabled) return false;
        _nodeBase.Navigation.MoveToNextFreeNode();
        return true;
    }

}


