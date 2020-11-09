public class Standard : NodeBase
{
    public Standard(UINode uiNode) : base(uiNode) => _uiNode = uiNode;

    protected override void TurnNodeOnOff()
    {
        if (_uiNode.IsSelected)
        {
            Deactivate();
            NodeActivated = false;
            ChildIsActive?.RaiseEvent(this);
            _uiNodeEvents.DoPlayCancelAudio();
        }
        else
        {
            Activate();
            NodeActivated = true;
            ChildIsActive?.RaiseEvent(this);
            _uiNodeEvents.DoPlaySelectedAudio();
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
