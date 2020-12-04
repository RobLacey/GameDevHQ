using UnityEngine;

public interface IHoverToActivate : INodeBase { }


public class HoverToActivate : NodeBase, IHoverToActivate
{
    private bool _allowKeys;
    
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

    protected override void OnEnter(bool isDragEvent)
    {
        Debug.Log("******** I am here!!!*********");
        if (!_allowKeys && IsSelected)
        {
            _uiEvents.DoWhenPointerOver(PointerOverNode);
        }
        else
        {
            TurnNodeOnOff();
        }
    }

    protected override void OnExit(bool isDragEvent)
    {
        PointerOverNode = false;
        
        if (_uiNode.CloseHooverOnExit && NoActiveChild())
        {
            CloseHoverOverProcess();
        }
        _uiEvents.DoWhenPointerOver(PointerOverNode);
    }

    private bool NoActiveChild()
    {
        return _uiNode.HasChildBranch is null || !_uiNode.HasChildBranch.CanvasIsEnabled;
    }

    protected override void TurnNodeOnOff()
    {
        if (IsSelected)
        {
            Deactivate();
        }
        else
        {
            PointerOverNode = false;
            Activate();
        }
        ThisNodeIsSelected();
    }

    protected override void Activate() 
    {
        ThisNodeIsHighLighted();
        SetSelectedStatus(true, DoPressOnNode);
    }
    
    public override void DeactivateNodeByType()
    {
        if (!IsSelected) return;
         Deactivate();
         SetNotHighlighted();
    }
    
    private void CancelHoverOver(ICancelHoverOver args) => CloseHoverOverProcess();

    private void CancelHoverOverFromButton(ICancelHoverOverButton args) => CloseHoverOverProcess();

    private void CloseHoverOverProcess()
    {
        if (!_uiNode.HasChildBranch.CanvasIsEnabled) return;
        TurnNodeOnOff();
    }
}
