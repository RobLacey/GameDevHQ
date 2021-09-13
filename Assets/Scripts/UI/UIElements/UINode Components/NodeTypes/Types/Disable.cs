using System;
using EZ.Events;
using EZ.Inject;
using EZ.Service;
using UnityEngine;

public interface IDisableData : IParameters
{
    INode ThisNode { get; }
    void SetNodeAsNotSelected_NoEffects();
}

public class DisabledNode : IDisabledNode, IEZEventDispatcher, IServiceUser
{
    public DisabledNode(IDisableData nodeBase)
    {
        ThisIsTheDisabledNode = nodeBase.ThisNode;
        _nodeBase = nodeBase;
    }

    private bool _isDisabled;
    private readonly IDisableData _nodeBase;
    private IDataHub _myDataHub;

    //Events
    private Action<IDisabledNode> ThisIsDisabled { get; set; }

    //Properties
    public INode ThisIsTheDisabledNode { get; }

    //Main
    public void OnEnable()
    {
        UseEZServiceLocator();
        FetchEvents();
    }

    public void OnDisable()
    {
        _isDisabled = false;
        ThisIsDisabled = null;
    }

    public void FetchEvents() => ThisIsDisabled = HistoryEvents.Do.Fetch<IDisabledNode>();
    public void UseEZServiceLocator() => _myDataHub = EZService.Locator.Get<IDataHub>(this);


    public bool IsDisabled
    {
        get => _isDisabled;
        set
        {
            _isDisabled = value;
            if (!_isDisabled) return;
            
            ThisIsDisabled?.Invoke(this);
            _nodeBase.SetNodeAsNotSelected_NoEffects();
            if(_myDataHub.Highlighted == ThisIsTheDisabledNode)
                FindNextFreeNode();
        }
    }

    public void FindNextFreeNode()
    {
        INode freeNode = null;
        
        foreach (var node in ThisIsTheDisabledNode.MyBranch.ThisGroupsUiNodes)
        {
            if(node.IsDisabled) continue;
            freeNode = node;
            break;
        }

        if (freeNode.IsNotNull())
        {
            freeNode.SetNodeAsActive();
        }
        else
        {
            Debug.Log("Help Nothing is free");
        }
    }
}


