using UnityEngine;

public class HoverToActivate : NodeBase
{
    private bool _allowKeys;
    public HoverToActivate(UINode node) : base(node) => _uiNode = node;

    public override void ObserveEvents()
    {
        base.ObserveEvents();
        EventLocator.Subscribe<IAllowKeys>(SaveAllowKeys, this);
        EventLocator.Subscribe<ICancelHoverOver>(CancelHoverOver, this);
    }

    public override void RemoveFromEvents()
    {
        base.RemoveFromEvents();
        EventLocator.Unsubscribe<IAllowKeys>(SaveAllowKeys);
        EventLocator.Unsubscribe<ICancelHoverOver>(CancelHoverOver);
    }

    private void SaveAllowKeys(IAllowKeys newNode)
    {
        _allowKeys = newNode.CanAllowKeys;
    }

    public override void OnEnter(bool isDragEvent)
    {
        _uiNode.SetAsHighlighted();
        if(!_allowKeys && _uiNode.IsSelected) return;
        TurnNodeOnOff();
    }

    public override void OnExit(bool isDragEvent)
    {
        Debug.Log(_uiNode.CloseHooverOnExit);
        if (_uiNode.CloseHooverOnExit)
        {
           TurnNodeOnOff();
           _uiNode.SetNotHighlighted();
        }
        else
        {
            _uiNode.SetNotHighlighted();
        }
    }

    public override void OnSelected(bool isDragEvent) => TurnNodeOnOff();

    public override void TurnNodeOnOff()
    {
        if (_uiNode.IsSelected)
        {
            Deactivate();
        }
        else
        {
            Activate();
            PointerOverNode = false;
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
    
    private void CancelHoverOver(ICancelHoverOver args)
    {
        if(!_uiNode.HasChildBranch.CanvasIsEnabled) return;
        TurnNodeOnOff();
        _uiNode.SetNodeAsActive();
    }
}
