using System;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class NodeBase : IEventUser, INodeBase, IEventDispatcher, ISelectedNode, 
                                 IHighlightedNode
                                 
{
    protected NodeBase(INode node)
    {
        _uiNode = node;
        _uiFunctionEvents = _uiNode.UINodeEvents;
        MyBranch = _uiNode.MyBranch;
    }
    
    //Variables
    protected INode _uiNode;
    private IDisabledNode _disabledNode;
    private bool _allowKeys;
    private bool _inMenu;
    private bool _hasFinishedSetUp;
    private INode _lastHighlighted;
    protected readonly IUiEvents _uiFunctionEvents;
    protected Transform _originalParentTransform;
    protected Transform _myTransform;
    protected Transform _myChildsTransform;

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
    private void SaveHighlighted(IHighlightedNode args) 
    {
        if (_lastHighlighted is null) _lastHighlighted = args.Highlighted;
        UnHighlightThisNode(args);        
        _lastHighlighted = args.Highlighted;
    }

    private void UnHighlightThisNode(IHighlightedNode args)
    {
        if(!_allowKeys) return;
        if (_lastHighlighted == _uiNode && args.Highlighted != _uiNode)
        {
            SetNotHighlighted();
        }
    }
    private void SaveAllowKeys(IAllowKeys args)
    {
        _allowKeys = args.CanAllowKeys;
        if(!_allowKeys && ReferenceEquals(_lastHighlighted, _uiNode))
        {
            SetNotHighlighted();
        }    
    }
    
    private void SaveInMenuOrInGame(IInMenu args)
    {
        _inMenu = args.InTheMenu;
        if (HasNotFinishedSetUp()) return;
        
        if (!_inMenu)
        {
            SetNotHighlighted();
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


    public void OnEnable()
    {
        FetchEvents();
        ObserveEvents();
    }
    
    public void OnDisable() => RemoveEvents();

    public virtual void FetchEvents()
    {
        DoHighlighted = EVent.Do.Fetch<IHighlightedNode>();
        DoSelected = EVent.Do.Fetch<ISelectedNode>();
    }

    public virtual void ObserveEvents()
    {
        EVent.Do.Subscribe<IAllowKeys>(SaveAllowKeys);
        EVent.Do.Subscribe<IInMenu>(SaveInMenuOrInGame);
        EVent.Do.Subscribe<IHighlightedNode>(SaveHighlighted);
        EVent.Do.Subscribe<IHotKeyPressed>(HotKeyPressed);
        EVent.Do.Subscribe<ICancelHoverOver>(OnCancelHooverOver);
    }

    public virtual void RemoveEvents()
    {
        EVent.Do.Unsubscribe<IAllowKeys>(SaveAllowKeys);
        EVent.Do.Unsubscribe<IInMenu>(SaveInMenuOrInGame);
        EVent.Do.Unsubscribe<IHighlightedNode>(SaveHighlighted);
        EVent.Do.Unsubscribe<IHotKeyPressed>(HotKeyPressed);
        EVent.Do.Unsubscribe<ICancelHoverOver>(OnCancelHooverOver);
    }

    public virtual void Start()
    {
        _disabledNode = new DisabledNode(_uiNode, this);
        if(_uiNode.HasChildBranch is null) return;
        _myTransform = _uiNode.MyBranch.ThisBranchesGameObject.transform;
        _originalParentTransform = _uiNode.HasChildBranch.MyParentBranch.ThisBranchesGameObject.transform;
        _myChildsTransform = _uiNode.HasChildBranch.ThisBranchesGameObject.transform;
    }

    public virtual void DeactivateNodeByType()
    {
        //Do Nothing for most types
    }

    public void ClearNodeCompletely()
    {
        SetNodeAsNotSelected_NoEffects();
        SetNotHighlighted();
    }

    // ReSharper disable once UnusedMember.Global - Assigned in editor to Enable Object
    public void EnableNode()
    {
        _disabledNode.IsDisabled = false;
        _uiFunctionEvents.DoIsDisabled(_disabledNode.IsDisabled);
    }

    // ReSharper disable once UnusedMember.Global - Assigned in editor to Disable Object
    public void DisableNode()
    {
        _disabledNode.IsDisabled = true;
        _uiFunctionEvents.DoIsDisabled(_disabledNode.IsDisabled);
    }

    public void SetNodeAsActive()
    {
        if (_disabledNode.IsThisNodeIsDisabled()) return;
        
        ThisNodeIsHighLighted();
        
        if (_allowKeys && _inMenu)
        {
            SetAsHighlighted();
        }
        else
        {
            SetNotHighlighted();
        }
    }

    private void SetAsHighlighted() 
    {
        if (IsDisabled) return;
        ThisNodeIsHighLighted();
        PointerOverNode = true;
        _uiFunctionEvents.DoWhenPointerOver(PointerOverNode);
    }

    protected void SetNotHighlighted()
    {
        PointerOverNode = false;
        _uiFunctionEvents.DoWhenPointerOver(PointerOverNode);
    }

    protected void ThisNodeIsSelected() => DoSelected?.Invoke(this);

    public void ThisNodeIsHighLighted() => DoHighlighted?.Invoke(this);
    
    public void DoPressOnNode() => _uiFunctionEvents.DoIsPressed();

    public void SetSelectedStatus(bool isSelected, Action endAction)
    {
        IsSelected = isSelected;
        _uiFunctionEvents.DoIsSelected(IsSelected);
        endAction.Invoke();
    }

    public void SelectedAction(bool isDragEvent)
    {
        if (IsDisabled) return;
        if (_allowKeys) PointerOverNode = false;
        OnSelected(isDragEvent);
    }

    protected virtual void OnSelected(bool isDragEvent)
    {
        if(isDragEvent) return;
        TurnNodeOnOff();
    }

    protected void SetNodeAsSelected_NoEffects()
    {
        _uiFunctionEvents.DoMuteAudio();
        SetSelectedStatus(true, SetNotHighlighted);
    }

    protected void SetNodeAsNotSelected_NoEffects()
    {
        _uiFunctionEvents.DoMuteAudio();
        SetSelectedStatus(false, SetNotHighlighted);
    }

    public virtual void OnEnter(bool isDragEvent)
    {
        if(isDragEvent) return;
        PointerOverNode = true;
        SetAsHighlighted();
    }

    public virtual void OnExit(bool isDragEvent)
    {
        if(isDragEvent) return;
        PointerOverNode = false;
        SetNotHighlighted();
    }

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
            SetAsHighlighted();
        }
    }

    protected virtual void TurnNodeOnOff()
    {
        if (IsSelected)
        {
            Deactivate();
        }
        else
        {
            Activate();
        }
        ThisNodeIsSelected();
    }

    protected virtual void Activate() => SetSelectedStatus(true, DoPressOnNode);

    protected void Deactivate() => SetSelectedStatus(false, DoPressOnNode);

    private void HotKeyPressed(IHotKeyPressed args)
    {
        if (!ReferenceEquals(args.ParentNode, _uiNode) || _disabledNode.IsDisabled) return;
        ThisNodeIsHighLighted();
        SetNodeAsSelected_NoEffects();
        args.MyBranch.MoveToThisBranch();
    }

    protected  virtual void OnCancelHooverOver(ICancelHoverOver args)
    {
        //Tidy up UIBranch hoover exit and Branch Factory
        //Fix hot key race condition
        //Turning off when going to Parent
        //Apply to correct branch types and Node types
        //Sort out where it can be shown
        //Check on and off work
    }

}
