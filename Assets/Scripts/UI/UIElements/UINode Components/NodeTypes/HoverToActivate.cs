using UnityEngine;

public class HoverToActivate : NodeBase
{
    private bool _allowKeys;
    public HoverToActivate(UINode node) : base(node) => _uiNode = node;

    public override void ObserveEvents()
    {
        base.ObserveEvents();
        EventLocator.Subscribe<IAllowKeys>(SaveAllowKeys, this);
    }

    public override void RemoveFromEvents()
    {
        base.RemoveFromEvents();
        EventLocator.Unsubscribe<IAllowKeys>(SaveAllowKeys);
    }

    private void SaveAllowKeys(IAllowKeys newNode)
    {
        _allowKeys = newNode.CanAllowKeys;
    }

    public override void OnEnter(bool isDragEvent)
    {
        Debug.Log(_uiNode);
        _uiNode.SetAsHighlighted();
        if(!_allowKeys && _uiNode.IsSelected) return;
       // if(_uiNode.IsSelected) return;
        // PointerOverNode = false;
        TurnNodeOnOff();
    }

    public override void OnExit(bool isDragEvent)
    {
        if (_uiNode.CloseHooverOnExit)
        {
           TurnNodeOnOff();
        }
        else
        {
            _uiNode.SetNotHighlighted();
        }
    }

    public override void OnSelected(bool isDragEvent)
    {
        _uiNode.PlayCancelAudio();
        TurnNodeOnOff();
    }

    protected override void TurnNodeOnOff()
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
}
