public class HoverToActivate : INodeBase, IServiceUser
{
    private readonly UINode _uiNode;
    private IHistoryTrack _uiHistoryTrack;

    public HoverToActivate(UINode node) => _uiNode = node;

    public void Start() => SubscribeToService();
    
    public void SubscribeToService() => _uiHistoryTrack = ServiceLocator.GetNewService<IHistoryTrack>(this);

    public void TurnNodeOnOff()
    {
        if (_uiNode.IsSelected)
        {
            Deactivate();
            _uiNode.MyBranch.Branch.MoveBackToThisBranch(_uiNode.MyBranch);
        }
        else
        {
            Activate();
        }
        _uiHistoryTrack.SetSelected(_uiNode);
    }

    private void Activate() 
    {
        _uiNode.ThisNodeIsHighLighted();
        _uiNode.SetSelectedStatus(true, _uiNode.DoPress);
        _uiNode.ThisNodeIsSelected();
    }
    
    private void Deactivate() => _uiNode.SetSelectedStatus(false, _uiNode.DoPress);
    
    public void DeactivateNode()
    {
        if (!_uiNode.IsSelected) return;
        Deactivate();
        _uiNode.SetNotHighlighted();
    }
}
