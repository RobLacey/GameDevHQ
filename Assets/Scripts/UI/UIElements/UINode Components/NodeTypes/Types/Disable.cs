using System;

public class DisabledNode : IDisabledNode
{
    public DisabledNode(UINode uiNode)
    {
        _uiNode = uiNode;
        ThisIsDisabled = EVent.Do.Fetch<IDisabledNode>();
    }

    private readonly UINode _uiNode;
    private bool _isDisabled;

    //Events
    private Action<IDisabledNode> ThisIsDisabled { get; }

    //Properties
    public INode ThisNodeIsDisabled => _uiNode;
    
    //Main
    public bool IsDisabled
    {
        get => _isDisabled;
        set
        {
            _isDisabled = value;
            if (!_isDisabled) return;
            
            ThisIsDisabled?.Invoke(this);
            _uiNode.SetSelectedStatus(false, _uiNode.DoPress);
        }
    }

    public bool NodeIsDisabled()
    {
        if (!IsDisabled) return false;
        _uiNode.Navigation.MoveToNextFreeNode();
        return true;
    }

}


