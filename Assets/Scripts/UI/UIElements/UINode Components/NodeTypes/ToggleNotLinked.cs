public class ToggleNotLinked : NodeBase
{
    public ToggleNotLinked(UINode node) : base(node) => _uiNode = node;

    public override void TurnNodeOnOff()
    {
        if (_uiNode.IsSelected)
        {
            Deactivate();
            _uiNode.PlayCancelAudio();
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
