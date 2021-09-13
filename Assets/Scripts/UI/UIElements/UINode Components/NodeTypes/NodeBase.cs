using System;
using EZ.Events;
using EZ.Service;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class NodeBase : IEZEventUser, INodeBase, IEZEventDispatcher, ISelectedNode, 
                                 IHighlightedNode, IDisableData, IServiceUser
{
    protected NodeBase(INode node)
    {
        _uiNode = node;
        _uiFunctionEvents = _uiNode.UINodeEvents;
        MyBranch = _uiNode.MyBranch;
    }
    
    //Variables
    protected readonly INode _uiNode;
    private IDisabledNode _disabledNode;
    private bool _hasFinishedSetUp;
    private IDataHub _myDataHub;
    private readonly IUiEvents _uiFunctionEvents;
    private bool _fromHotKey;

    //Events
    private Action<IHighlightedNode> DoHighlighted { get; set; } 

    private Action<ISelectedNode> DoSelected { get; set; }

    //Properties
    private INode LastHighlighted => _myDataHub.Highlighted;
    private bool InMenu => _myDataHub.InMenu;
    protected bool AllowKeys => _myDataHub.AllowKeys;
    protected bool PointerOverNode { get; private set; }
    public IBranch MyBranch { get; protected set; }
    public bool IsDisabled => _disabledNode.IsDisabled;
    public UINavigation Navigation { get; set; }
    protected bool IsSelected { get; private set; }
    public INode Highlighted => _uiNode;
    public INode SelectedNode { get; private set; }
    public INode ThisNode => _uiNode;

    //Set / Getters
    private void SetNewHighlighted(IHighlightedNode args) 
    {
        if ((PointerOverNode && _myDataHub.AllowKeys) && args.Highlighted != _uiNode)
            OnExit();
    }

    private void SaveInMenuOrInGame(IInMenu args)
    {
        if (HasNotFinishedSetUp()) return;
        
        if(!ReferenceEquals(LastHighlighted, _uiNode)) return;
        
        if (!InMenu)
        {
            OnExit();
            return;
        }
        
        if (ReferenceEquals(LastHighlighted, _uiNode))
            SetNodeAsActive();
    }
    
    private bool HasNotFinishedSetUp()
    {
        if (_hasFinishedSetUp) return false;
        _hasFinishedSetUp = true;
        return true;
    }
    
    //Main
    public void OnAwake() => _disabledNode = EZInject.Class.WithParams<IDisabledNode>(this);

    public void OnEnable()
    {
        UseEZServiceLocator();
        FetchEvents();
        ObserveEvents();
        LateStartSetUp();
    }

    public void UseEZServiceLocator() => _myDataHub = EZService.Locator.Get<IDataHub>(this);

    private void LateStartSetUp()
    {
        if(_myDataHub.IsNull()) return;

        if (_myDataHub.SceneStarted)
        {
            SetNodeAsNotSelected_NoEffects();
            PointerOverNode = false;
        }
    }

    public virtual void FetchEvents()
    {
        DoHighlighted = HistoryEvents.Do.Fetch<IHighlightedNode>();
        DoSelected = HistoryEvents.Do.Fetch<ISelectedNode>();
    }

    public void ObserveEvents()
    {
        HistoryEvents.Do.Subscribe<IInMenu>(SaveInMenuOrInGame);
        HistoryEvents.Do.Subscribe<IHighlightedNode>(SetNewHighlighted);
        _disabledNode.OnEnable();
    }

    public void UnObserveEvents()
    {
        HistoryEvents.Do.Unsubscribe<IInMenu>(SaveInMenuOrInGame);
        HistoryEvents.Do.Unsubscribe<IHighlightedNode>(SetNewHighlighted);
        _disabledNode.OnDisable();
    }
    
    public void OnDisable()
    {
        EnableNodeAfterBeingDisabled();
        UnObserveEvents();
        DoHighlighted = null;
        DoSelected = null;
        _myDataHub = null;
    }

    public void OnDestroy()
    {
        UnObserveEvents();
        _myDataHub = null;
        _disabledNode = null;
    }

    public virtual void OnStart() { }

    public void UnHighlightAlwaysOn() => OnExit();
    public virtual void DeactivateNodeByType() => OnExit();

    public virtual void ClearNodeCompletely()
    {
        SetNodeAsNotSelected_NoEffects();
        OnExit();
    }

    public void EnableNodeAfterBeingDisabled()
    {
        _disabledNode.IsDisabled = false;
        _uiFunctionEvents.DoIsDisabled(IsDisabled);
    }

    public void DisableNode()
    {
        if(IsDisabled) return;
        _disabledNode.IsDisabled = true;
        _uiFunctionEvents.DoIsDisabled(IsDisabled);
    }

    public virtual void SetNodeAsActive()
    {
        if (IsDisabled)
            _disabledNode.FindNextFreeNode();
        
        ThisNodeIsHighLighted();
        
        if (AllowKeys && InMenu || MyBranch.AlwaysHighlighted == IsActive.Yes)
        {
            OnEnter();
        }
        else
        {
            if(PointerOverNode) return;
            OnExit();
        }
    }

    private void SetAsHighlighted() 
    {
        ThisNodeIsHighLighted();
        PointerOverNode = true;
        _uiFunctionEvents.DoWhenPointerOver(PointerOverNode);
    }

    private void SetNotHighlighted()
    {
        PointerOverNode = false;
        _uiFunctionEvents.DoWhenPointerOver(PointerOverNode);
    }

    protected void ThisNodeIsSelected(INode node)
    {
        SelectedNode = node;
        DoSelected?.Invoke(this);
    }

    public virtual void ThisNodeIsHighLighted() => DoHighlighted?.Invoke(this);

    public void ThisNodeNotHighLighted() => SetNotHighlighted();

    protected void DoPressOnNode() => _uiFunctionEvents.DoIsPressed();

    public void SelectedAction()
    {
        if (IsDisabled) return;
        TurnNodeOnOff();
    }

    protected void SetSelectedStatus(bool isSelected, Action endAction)
    {
        IsSelected = isSelected;
        _uiFunctionEvents.DoIsSelected(IsSelected);
        endAction?.Invoke();
    }

    protected void SetNodeAsSelected_NoEffects()
    {
        _uiFunctionEvents.DoMuteAudio();
        if (_fromHotKey)
        {
            SetSelectedStatus(true, null);
            _fromHotKey = false;
        }
        else
        {
            SetSelectedStatus(true, DoPressOnNode);
        }

        OnExit();
    }

    public void SetNodeAsNotSelected_NoEffects()
    {
        _uiFunctionEvents.DoMuteAudio();
        SetSelectedStatus(false, DoPressOnNode);
        OnExit();
    }

    public virtual void OnEnter() => SetAsHighlighted();

    public virtual void OnExit() => SetNotHighlighted();

    public void DoMoveToNextNode(MoveDirection moveDirection) => _uiFunctionEvents.DoOnMove(moveDirection);

    public void DoNonMouseMove(MoveDirection moveDirection)
    {
        if(!MyBranch.CanvasIsEnabled)return;
        
        OnEnter();
        
        if (IsDisabled)
            DoMoveToNextNode(moveDirection);
    }

    protected virtual void TurnNodeOnOff()
    {
         ThisNodeIsSelected(_uiNode);
        
        if (IsSelected)
        {
            Deactivate();
        }
        else
        {
            Activate();
        }
    }

    protected virtual void Activate() => SetSelectedStatus(true, DoPressOnNode);

    protected void Deactivate()
    {
        SetSelectedStatus(false, DoPressOnNode);
        ThisNodeIsSelected(null);
    }

    public void HotKeyPressed(bool setAsActive)
    {
        _fromHotKey = true;
        ThisNodeIsHighLighted();
        ThisNodeIsSelected(_uiNode);
        if(!setAsActive) return;
        SetNodeAsSelected_NoEffects();
    }
    
    public virtual void SetUpGOUIParent(IGOUIModule module) { }
}
