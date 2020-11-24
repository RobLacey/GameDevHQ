
public interface IStandard : INodeBase { }

public class Standard : NodeBase, IStandard
{
    public Standard(INode uiNode) : base(uiNode) => _uiNode = uiNode;
    
    protected override void TurnNodeOnOff()
    {
        if (_uiNode.IsSelected)
        {
            Deactivate();
        }
        else
        {
            Activate();
        }
        _uiHistoryTrack.SetSelected(_uiNode);
    }

    private void Deactivate() => _uiNode.SetSelectedStatus(false, _uiNode.DoPress);

    private void Activate() 
    {
        _uiNode.SetSelectedStatus(true, _uiNode.DoPress);
        _uiNode.ThisNodeIsSelected();
    }

    public override void DeactivateNode()
    {
        if (!_uiNode.IsSelected) return;
        Deactivate();
        _uiNode.SetNotHighlighted();
    }

}
