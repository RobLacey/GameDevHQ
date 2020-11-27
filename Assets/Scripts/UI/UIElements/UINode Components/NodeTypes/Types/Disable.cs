using System;

public class DisabledNode : IDisabledNode
{
    private readonly UINode _uiNode;
    private bool _isDisabled;

    private Action<IDisabledNode> ThisIsDisabled { get; } = EVent.Do.FetchEVent<IDisabledNode>();

    public DisabledNode(UINode uiNode) => _uiNode = uiNode;

    public INode ThisNodeIsDisabled => _uiNode;
    
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


