public interface IToggleNotLinked : INodeBase { }


public class ToggleNotLinked : NodeBase, IToggleNotLinked
{
    public ToggleNotLinked(INode node) : base(node) => _uiNode = node;

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

    public override void DeactivateNode() { }

    private void Activate() => _uiNode.SetSelectedStatus(true, _uiNode.DoPress);

    private void Deactivate() => _uiNode.SetSelectedStatus(false, _uiNode.DoPress);
}
