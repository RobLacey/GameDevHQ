public class ToggleNotLinked : NodeBase
{
    public ToggleNotLinked(UINode node) : base(node) => _uiNode = node;

    protected override void TurnNodeOnOff()
    {
        if (_uiNode.IsSelected)
        {
            Deactivate();
            _uiNodeEvents.DoPlayCancelAudio();
        }
        else
        {
            Activate();
            _uiNodeEvents.DoPlaySelectedAudio();
        }
        _uiHistoryTrack.SetSelected(_uiNode);
    }

    public override void DeactivateNode() { }

    private void Activate() => _uiNode.SetSelectedStatus(true, _uiNode.DoPress);

    private void Deactivate() => _uiNode.SetSelectedStatus(false, _uiNode.DoPress);
}
