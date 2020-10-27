public class ToggleNotLinked : INodeBase, IServiceUser
{
    private readonly UINode _uiNode;
    private IHistoryTrack _uiHistoryTrack;

    public ToggleNotLinked(UINode node) => _uiNode = node;

    public void Start() => SubscribeToService();

    public void SubscribeToService() => _uiHistoryTrack = ServiceLocator.GetNewService<IHistoryTrack>(this);

    public void TurnNodeOnOff()
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

    public void DeactivateNode() { }

    private void Activate() => _uiNode.SetSelectedStatus(true, _uiNode.DoPress);

    private void Deactivate() => _uiNode.SetSelectedStatus(false, _uiNode.DoPress);
}
