
public interface IInGameNode : INodeBase { }

public class InGameNode : NodeBase, IInGameNode
{
    public InGameNode(INode node) : base(node) { }

    protected override void SaveInMenuOrInGame(IInMenu args)
    {
        if (args.InTheMenu)
        {
            OnExit();
        }
    }

    public override void SetNodeAsActive()
    {
        if (_disabledNode.IsThisNodeIsDisabled()) return;
        _inMenu = false;
        OnEnter();
    }

    protected override void SetAsHighlighted()
    {
        if (IsDisabled) return;
        PointerOverNode = true;
        _uiFunctionEvents.DoWhenPointerOver(PointerOverNode);
    }
}
