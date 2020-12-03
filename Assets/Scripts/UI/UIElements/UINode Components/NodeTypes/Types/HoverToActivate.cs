public interface IHoverToActivate : INodeBase { }


public class HoverToActivate : NodeBase, IHoverToActivate
{
    private bool _allowKeys;
    private bool _closedChildBranch;
    
    public HoverToActivate(INode node) : base(node) => _uiNode = node;

    public override void ObserveEvents()
    {
        base.ObserveEvents();
        EVent.Do.Subscribe<IAllowKeys>(SaveAllowKeys);
        EVent.Do.Subscribe<ICancelHoverOver>(CancelHoverOver);
        EVent.Do.Subscribe<ICancelHoverOverButton>(CancelHoverOverFromButton);
    }

    public override void RemoveEvents()
    {
        base.RemoveEvents();
        EVent.Do.Unsubscribe<IAllowKeys>(SaveAllowKeys);
        EVent.Do.Unsubscribe<ICancelHoverOver>(CancelHoverOver);
        EVent.Do.Unsubscribe<ICancelHoverOverButton>(CancelHoverOverFromButton);
    }

    private void SaveAllowKeys(IAllowKeys newNode) => _allowKeys = newNode.CanAllowKeys;

    public override void OnEnter(bool isDragEvent)
    {
        if (_closedChildBranch)
        {
            PointerOverNode = true;
            _closedChildBranch = false;
            return;
        }
        NodeIsActive();
    }

    private void NodeIsActive()
    {
        if (!_allowKeys && _uiNode.IsSelected)
        {
            _uiNode.UINodeEvents.DoWhenPointerOver(PointerOverNode);
           // _uiNode.SetAsHighlighted();
        }
        else
        {
            TurnNodeOnOff();
        }
    }

    public override void OnExit(bool isDragEvent)
    {
        PointerOverNode = false;
        
        if (_uiNode.CloseHooverOnExit)
        {
            TurnNodeOnOff();
        }
        _uiNode.UINodeEvents.DoWhenPointerOver(PointerOverNode);
        //_uiNode.SetNotHighlighted();
    }

    public override void OnSelected(bool isDragEvent) => TurnNodeOnOff();

    protected override void TurnNodeOnOff()
    {
        if (_uiNode.IsSelected)
        {
            Deactivate();
        }
        else
        {
            PointerOverNode = false;
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
    
    public override void DeactivateNode()
    {
        if (!_uiNode.IsSelected) return;
         Deactivate();
         _uiNode.SetNotHighlighted();
    }
    
    private void CancelHoverOver(ICancelHoverOver args) => CloseHoverOverProcess();

    private void CancelHoverOverFromButton(ICancelHoverOverButton args) => CloseHoverOverProcess();

    private void CloseHoverOverProcess()
    {
        if (!_uiNode.HasChildBranch.CanvasIsEnabled) return;
        _closedChildBranch = true;
        TurnNodeOnOff();
    }
}
