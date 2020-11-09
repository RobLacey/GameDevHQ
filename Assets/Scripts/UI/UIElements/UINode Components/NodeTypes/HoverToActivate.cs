public class HoverToActivate : NodeBase
{
    private bool _allowKeys;
    private bool _activeChildBranch;
    
    public HoverToActivate(UINode node) : base(node) => _uiNode = node;

    public override void ObserveEvents()
    {
        base.ObserveEvents();
        EventLocator.Subscribe<IAllowKeys>(SaveAllowKeys, this);
        EventLocator.Subscribe<ICancelHoverOver>(CancelHoverOver, this);
        EventLocator.Subscribe<IChildIsActive>(ThisBranchesChildIsActive, this);
        EventLocator.Subscribe<ICancelHoverOverButton>(CancelHoverOverFromButton, this);
    }

    public override void RemoveFromEvents()
    {
        base.RemoveFromEvents();
        EventLocator.Unsubscribe<IAllowKeys>(SaveAllowKeys);
        EventLocator.Unsubscribe<ICancelHoverOver>(CancelHoverOver);
        EventLocator.Unsubscribe<IChildIsActive>(ThisBranchesChildIsActive);
        EventLocator.Unsubscribe<ICancelHoverOverButton>(CancelHoverOverFromButton);
    }

    private void SaveAllowKeys(IAllowKeys newNode) => _allowKeys = newNode.CanAllowKeys;
    private void ThisBranchesChildIsActive(IChildIsActive thisBranch)
    {
        if(thisBranch.MyBranch == _uiNode.HasChildBranch)
        {
            _activeChildBranch = thisBranch.NodeActivated;
        }    
    }

    public override void OnEnter(bool isDragEvent)
    {
        if (!_allowKeys && _uiNode.IsSelected)
        {
            _uiNode.SetAsHighlighted();
            _uiNodeEvents.DoPlayHighlightedAudio();
        }
        else
        {
            TurnNodeOnOff();
        }
    }

    public override void OnExit(bool isDragEvent)
    {
        if (_uiNode.CloseHooverOnExit)
        {
            if(_activeChildBranch) return;
            TurnNodeOnOff();
            _uiNode.SetNotHighlighted();
        }
        else
        {
            _uiNode.SetNotHighlighted();
        }
    }

    public override void OnSelected(bool isDragEvent) => TurnNodeOnOff();

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
            PointerOverNode = false;
        }
        if(!_activeChildBranch)
            _uiHistoryTrack.SetSelected(_uiNode);
        _activeChildBranch = false;
    }

    private void Activate() 
    {
        _uiNode.ThisNodeIsHighLighted();
        _uiNode.SetSelectedStatus(true, _uiNode.DoPress);
        _uiNode.ThisNodeIsSelected();
    }

    private void Deactivate() => _uiNode.SetSelectedStatus(false, _uiNode.DoPress);
    
    public override void DeactivateNode()
    {
        if (!_uiNode.IsSelected) return;
        _activeChildBranch = false;
         Deactivate();
         _uiNode.SetNotHighlighted();
    }
    
    private void CancelHoverOver(ICancelHoverOver args) => CloseHoverOverProcess();

    private void CancelHoverOverFromButton(ICancelHoverOverButton args) => CloseHoverOverProcess();

    private void CloseHoverOverProcess()
    {
        if (!_uiNode.HasChildBranch.CanvasIsEnabled) return;
        TurnNodeOnOff();
        _uiNode.SetNodeAsActive();
    }
}
