
using UnityEngine;

public class Standard : INodeBase, IServiceUser
{
    private readonly UINode _uiNode;
    private IHistoryTrack _historyTracker;

    public Standard(UINode uiNode) => _uiNode = uiNode;

    public void Start() => SubscribeToService();

    public void SubscribeToService() => _historyTracker = ServiceLocator.GetNewService<IHistoryTrack>(this);

    public void TurnNodeOnOff()
    {
        if (_uiNode.IsSelected)
        {
            Deactivate();
            _uiNode.MyBranch.Branch.MoveBackToThisBranch(_uiNode.MyBranch);
            _uiNode.PlayCancelAudio();
        }
        else
        {
            Activate();
        }
        _historyTracker.SetSelected(_uiNode);
    }

    private void Deactivate() => _uiNode.SetSelectedStatus(false, _uiNode.DoPress);

    private void Activate() 
    {
        _uiNode.SetSelectedStatus(true, _uiNode.DoPress);
        _uiNode.ThisNodeIsSelected();
    }

    public void DeactivateNode()
    {
        if (!_uiNode.IsSelected) return;
        Deactivate();
        _uiNode.SetNotHighlighted();
    }
}
