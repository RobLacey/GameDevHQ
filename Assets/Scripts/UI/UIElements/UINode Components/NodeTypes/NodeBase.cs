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
    private bool _inMenu;
    private bool _hasFinishedSetUp;
    protected bool _allowKeys;
    private INode _lastHighlighted;
    private IDataHub _myDataHub;
    private readonly IUiEvents _uiFunctionEvents;
    private bool _fromHotKey;

    //Events
    private Action<IHighlightedNode> DoHighlighted { get; set; } 

    private Action<ISelectedNode> DoSelected { get; set; }

    //Properties
    protected bool PointerOverNode { get; set; }
    public IBranch MyBranch { get; protected set; }
    private bool IsDisabled => _disabledNode.IsDisabled;
    public UINavigation Navigation { get; set; }
    protected bool IsSelected { get; private set; }
    public INode Highlighted => _uiNode;
    public INode UINode => _uiNode;

    //Set / Getters
    private void ClearHighlight(ICancelButtonActivated args)
    {
        if(_lastHighlighted == _uiNode && _allowKeys)
            OnExit();
    }
    
    private void SaveHighlighted(IHighlightedNode args) 
    {
        if (_lastHighlighted is null) _lastHighlighted = args.Highlighted;
        UnHighlightThisNode(args);        
        _lastHighlighted = args.Highlighted;
    }

    private void UnHighlightThisNode(IHighlightedNode args)
    {
        if (_allowKeys && PointerOverNode && args.Highlighted != _uiNode)
            OnExit();
    }
    private void SaveAllowKeys(IAllowKeys args)
    {
        _allowKeys = args.CanAllowKeys;
        ClearHighlightedIfSwitchedToMouse();
    }

    private void ClearHighlightedIfSwitchedToMouse()
    {
        var thisNode = ReferenceEquals(_lastHighlighted, _uiNode);

        if (thisNode && !_allowKeys)
            OnExit();
    }

    private void SaveInMenuOrInGame(IInMenu args)
    {
        _inMenu = args.InTheMenu;
        if (HasNotFinishedSetUp()) return;
        
        if(!ReferenceEquals(_lastHighlighted, _uiNode)) return;
        
        if (!_inMenu)
        {
            OnExit();
            return;
        }
        
        if (ReferenceEquals(_lastHighlighted, _uiNode))
            SetNodeAsActive();
    }
    
    private bool HasNotFinishedSetUp()
    {
        if (_hasFinishedSetUp) return false;
        _hasFinishedSetUp = true;
        return true;
    }
    
    
    //Main
    public void OnAwake()
    {
        _disabledNode = EZInject.Class.WithParams<IDisabledNode>(this);
    }

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

        if (_myDataHub.SceneAlreadyStarted)
        {
            SetNodeAsNotSelected_NoEffects();
            _inMenu = _myDataHub.InMenu;
            PointerOverNode = false;
            _allowKeys = _myDataHub.AllowKeys;
        }
    }

    public virtual void FetchEvents()
    {
        DoHighlighted = HistoryEvents.Do.Fetch<IHighlightedNode>();
        DoSelected = HistoryEvents.Do.Fetch<ISelectedNode>();
    }

    public void ObserveEvents()
    {
        InputEvents.Do.Subscribe<IAllowKeys>(SaveAllowKeys);
        HistoryEvents.Do.Subscribe<IInMenu>(SaveInMenuOrInGame);
        HistoryEvents.Do.Subscribe<IHighlightedNode>(SaveHighlighted);
        CancelEvents.Do.Subscribe<ICancelButtonActivated>(ClearHighlight);
    }

    private void UnObserveEvents()
    {
        InputEvents.Do.Unsubscribe<IAllowKeys>(SaveAllowKeys);
        HistoryEvents.Do.Unsubscribe<IInMenu>(SaveInMenuOrInGame);
        HistoryEvents.Do.Unsubscribe<IHighlightedNode>(SaveHighlighted);
        CancelEvents.Do.Unsubscribe<ICancelButtonActivated>(ClearHighlight);
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

    public virtual void DeactivateNodeByType() => OnExit();

    public virtual void ClearNodeCompletely()
    {
        SetNodeAsNotSelected_NoEffects();
        OnExit();
    }

    public void EnableNodeAfterBeingDisabled()
    {
        _disabledNode.IsDisabled = false;
        _uiFunctionEvents.DoIsDisabled(_disabledNode.IsDisabled);
    }

    public void DisableNode()
    {
        _disabledNode.IsDisabled = true;
        _uiFunctionEvents.DoIsDisabled(_disabledNode.IsDisabled);
    }

    public virtual void SetNodeAsActive()
    {
        if (_disabledNode.IsThisNodeIsDisabled()) return;

        ThisNodeIsHighLighted();
        
        if (_allowKeys && _inMenu)
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
        if (IsDisabled) return;
        ThisNodeIsHighLighted();
        PointerOverNode = true;
        _uiFunctionEvents.DoWhenPointerOver(PointerOverNode);
    }

    private void SetNotHighlighted()
    {
        PointerOverNode = false;
        _uiFunctionEvents.DoWhenPointerOver(PointerOverNode);
    }

    protected virtual void ThisNodeIsSelected() => DoSelected?.Invoke(this);

    public virtual void ThisNodeIsHighLighted() => DoHighlighted?.Invoke(this);

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
        
        if (IsDisabled)
        {
            DoMoveToNextNode(moveDirection);
        }
        else
        {
            OnEnter();
        }
    }

    protected virtual void TurnNodeOnOff()
    {
        ThisNodeIsSelected();
        
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

    protected virtual void Deactivate() => SetSelectedStatus(false, DoPressOnNode);

    public void HotKeyPressed(bool setAsActive)
    {
        _fromHotKey = true;
        ThisNodeIsHighLighted();
        ThisNodeIsSelected();
        if(!setAsActive) return;
        SetNodeAsSelected_NoEffects();
    }
    
    public virtual void SetUpGOUIParent(IGOUIModule module) { }
}
