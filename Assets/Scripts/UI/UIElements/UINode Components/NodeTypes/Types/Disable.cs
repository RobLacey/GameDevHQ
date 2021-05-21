using System;
using EZ.Events;

public class DisabledNode : IDisabledNode, IEZEventDispatcher
{
    public DisabledNode(IDisableData nodeBase)
    {
        ThisIsTheDisabledNode = nodeBase.UINode;
        _nodeBase = nodeBase;
    }

    private bool _isDisabled;
    private readonly IDisableData _nodeBase;

    //Events
    private Action<IDisabledNode> ThisIsDisabled { get; set; }

    //Properties
    public INode ThisIsTheDisabledNode { get; }

    //Main
    public void OnEnable() => FetchEvents();

    public void OnDisable()
    {
        _isDisabled = false;
        ThisIsDisabled = null;
    }

    public void FetchEvents() => ThisIsDisabled = HistoryEvents.Do.Fetch<IDisabledNode>();

    public bool IsDisabled
    {
        get => _isDisabled;
        set
        {
            _isDisabled = value;
            if (!_isDisabled) return;
            
            ThisIsDisabled?.Invoke(this);
            _nodeBase.SetNodeAsNotSelected_NoEffects();
        }
    }

    public bool IsThisNodeIsDisabled()
    {
        if (!IsDisabled) return false;
        _nodeBase.Navigation.MoveToNextFreeNode();
        return true;
    }
}


