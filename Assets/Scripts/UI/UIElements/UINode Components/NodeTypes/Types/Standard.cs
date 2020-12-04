
public interface IStandard : INodeBase { }

public class Standard : NodeBase, IStandard
{
    public Standard(INode uiNode) : base(uiNode) => _uiNode = uiNode;
    
    public override void DeactivateNodeByType()
    {
        if (!IsSelected) return;
        Deactivate();
        SetNotHighlighted();
    }

}
