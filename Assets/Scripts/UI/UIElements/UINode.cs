using UnityEngine;
using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]

public partial class UINode : MonoBehaviour, INode, IEventUser, ICancelButtonActivated,
                              IHighlightedNode, ISelectedNode, IPointerEnterHandler, IPointerDownHandler,
                              IMoveHandler, IPointerUpHandler, ISubmitHandler, IPointerExitHandler
{
    [Header("Main Settings")]
    [HorizontalLine(1, color: EColor.Blue, order = 1)]
    [SerializeField] 
    [ValidateInput("SetChildBranch")] [Label("Button/Toggle Function")] 
    private ButtonFunction _buttonFunction;
    [SerializeField] 
    [ShowIf("IsCancelOrBack")] 
    private EscapeKey _escapeKeyFunction = EscapeKey.GlobalSetting;
    [SerializeField] 
    [HideIf(EConditionOperator.Or, "GroupSettings", "IsCancelOrBack")]
    private ToggleData _toggleData;
    [SerializeField] 
    [ShowIf(EConditionOperator.Or, "IsHoverToActivate")] 
    private bool _closeHoverOnExit;
    [SerializeField] 
    [ShowIf(EConditionOperator.Or, "IsToggleGroup")] 
    private IsActive _startAsSelected = IsActive.No;
    [SerializeField] 
    [Space(5f)] [BoxGroup("Functions")] [Label("UI Functions To Use")]  
    private Setting _enabledFunctions;
    [SerializeField] 
    [Space(15f)] [BoxGroup("Functions")] [Label("Navigation And On Click Calls")] [ShowIf("UseNavigation")]  
    private NavigationSettings _navigation;
    [SerializeField] 
    [Space(5f)] [BoxGroup("Functions")] [Label("Colour Settings")] [ShowIf("NeedColour")]  
    private ColourSettings _coloursTest;
    [SerializeField] 
    [Space(5f)] [BoxGroup("Functions")] [Label("Invert Colour when Highlighted or Selected")] 
    [ShowIf("NeedInvert")]  
    private InvertColoursSettings _invertColourCorrection;
    [SerializeField] 
    [Space(5f)] [BoxGroup("Functions")] [Label("Swap Images or Text on Select or Highlight")] 
    [ShowIf("NeedSwap")] 
    private SwapImageOrTextSettings _swapImageOrText;
    [SerializeField] 
    [Space(5f)] [BoxGroup("Functions")] [Label("Size And Position Effect Settings")] 
    [ShowIf("NeedSize")]  
    private SizeAndPositionSettings _sizeAndPos;
    [SerializeField] 
    [Space(5f)] [BoxGroup("Functions")] [Label("Accessories, Outline Or Shadow Settings")] 
    [ShowIf("NeedAccessories")]  
    private AccessoriesSettings _accessories;
    [SerializeField] 
    [Space(5f)] [BoxGroup("Functions")] [Label("Audio Settings")] [ShowIf("NeedAudio")]  
    private AudioSettings _audio;
    [SerializeField] 
    [Space(5f)] [BoxGroup("Functions")] [Label("Tooltip Settings")] [ShowIf("NeedTooltip")]  
    private ToolTipSettings _tooltips;
    [SerializeField] 
    [Space(5f)] [BoxGroup("Functions")] [Label("Event Settings")] [ShowIf("NeedEvents")]  
    private EventSettings _events;
    // [ContextMenuItem("TestCase", "Test")]
    // [SerializeField] private Vector3 _test;
    // [ColorUsage(false)]
    // [SerializeField]
    // private Color colorNoAlpha;
    //
    // [ColorUsage(true, true, 0.0f, 0.5f, 0.0f, 0.5f)]
    // [SerializeField]
    // private Color colorHdr;
    //
    // [Header("Text Attributes")]
    // [TextArea]
    // [Tooltip("A string using the TextArea attribute")]
    // [SerializeField]
    // private string descriptionTextArea;
    // [Multiline]
    // [Tooltip("A string using the MultiLine attribute")]
    // [SerializeField]
    // private string descriptionMultiLine;
    // private void Test()
    // {
    //     _test = GetComponent<RectTransform>().anchoredPosition3D;
    // }

    //Variables
    private bool _inMenu, _allowKeys, _childIsMoving;
    private INode _lastHighlighted;
    private IUiEvents _uiNodeEvents;
    private IDisabledNode _disabledNode;
    private INodeBase _nodeBase;
    private readonly List<NodeFunctionBase> _activeFunctions = new List<NodeFunctionBase>();

    private bool _canStart;
    private bool _setUpFinished;

    //Events
    private static CustomEvent<IHighlightedNode> DoHighlighted { get; } 
        = new CustomEvent<IHighlightedNode>();
    private static CustomEvent<ISelectedNode> DoSelected { get; } 
        = new CustomEvent<ISelectedNode>();
    
    //Properties & Enums
    public bool IsToggleGroup => _buttonFunction == ButtonFunction.ToggleGroup;
    public bool IsToggleNotLinked => _buttonFunction == ButtonFunction.ToggleNotLinked;
    public bool DontStoreTheseNodeTypesInHistory => IsToggleGroup || IsToggleNotLinked || HasChildBranch is null;
    private bool IsCancelOrBack => _buttonFunction == ButtonFunction.CancelOrBack;
    public bool IsSelected { get; private set; }
    public ToggleData ToggleData => _toggleData;
    public IBranch MyBranch { get; private set; }
    public IBranch HasChildBranch => _navigation.ChildBranch;
    public EscapeKey EscapeKeyType => _escapeKeyFunction;
    public GameObject ReturnGameObject => gameObject;
    public INode Highlighted => this;
    public INode Selected => this;
    public UINavigation Navigation => _navigation.Instance;
    public bool CloseHooverOnExit => _closeHoverOnExit;
    private void CanStart(IOnStart args) => _canStart = true;
    public IsActive ReturnStartAsSelected => _startAsSelected;
    public IsActive SetStartAsSelected { set => _startAsSelected = value; }

    private void SaveInMenuOrInGame(IInMenu args)
    {
        _inMenu = args.InTheMenu;
        if (SetUpFinished()) return;
        
        if (!_inMenu)
        {
            SetNotHighlighted();
        }
        else if (ReferenceEquals(_lastHighlighted, this) && _allowKeys) //TODO Check this works
        {
            SetNodeAsActive();
        }
    }

    private bool SetUpFinished()
    {
        if (_setUpFinished) return false;
        _setUpFinished = true;
        return true;
    }

    private void SaveAllowKeys(IAllowKeys args)
    {
        _allowKeys = args.CanAllowKeys;
        if(!_allowKeys && ReferenceEquals(_lastHighlighted, this)) //TODO Check this works
        {
            SetNotHighlighted();
        }    
    }

    private void SaveHighlighted(IHighlightedNode args) 
    {
        if (_lastHighlighted is null) _lastHighlighted = args.Highlighted;
        UnHighlightThisNode(args);        
        _lastHighlighted = args.Highlighted;
    }

    private void UnHighlightThisNode(IHighlightedNode args)
    {
        if(!_allowKeys) return;
        if (_lastHighlighted == this && args.Highlighted != this)
        {
            SetNotHighlighted();
        }
    }
    
    // ReSharper disable once UnusedMember.Global - Assigned in editor to Disable Object
    public void DisableNode()
    {
        _disabledNode.IsDisabled = true;
        _uiNodeEvents.DoIsDisabled(_disabledNode.IsDisabled);
    }
    // ReSharper disable once UnusedMember.Global - Assigned in editor to Enable Object
    public void EnableNode()
    {
        _disabledNode.IsDisabled = false;
        _uiNodeEvents.DoIsDisabled(_disabledNode.IsDisabled);
    }

    //Main
    private void Awake()
    {
        MyBranch = GetComponentInParent<IBranch>();
        _uiNodeEvents = new UiEvents(gameObject.GetInstanceID(), this);
        StartNodeFactory();
        SetUpUiFunctions();
        ObserveEvents();
    }

    private void StartNodeFactory()
    {
        _disabledNode = new DisabledNode(this);
        _nodeBase = NodeFactory.Factory(_buttonFunction, this);
    }

    private void SetUpUiFunctions()
    {
        _activeFunctions.Add(_coloursTest.SetUp(_uiNodeEvents, _enabledFunctions));
        _activeFunctions.Add(_events.SetUp(_uiNodeEvents, _enabledFunctions));
        _activeFunctions.Add(_accessories.SetUp(_uiNodeEvents, _enabledFunctions));
        _activeFunctions.Add(_invertColourCorrection.SetUp(_uiNodeEvents, _enabledFunctions));
        _activeFunctions.Add(_swapImageOrText.SetUp(_uiNodeEvents, _enabledFunctions));
        _activeFunctions.Add(_sizeAndPos.SetUp(_uiNodeEvents, _enabledFunctions));
        _activeFunctions.Add(_tooltips.SetUp(_uiNodeEvents, _enabledFunctions));
        _activeFunctions.Add(_navigation.SetUp(_uiNodeEvents, _enabledFunctions));
        _activeFunctions.Add(_audio.SetUp(_uiNodeEvents, _enabledFunctions));
    }

    public void ObserveEvents()
    {
        EventLocator.Subscribe<IAllowKeys>(SaveAllowKeys, this);
        EventLocator.Subscribe<IHighlightedNode>(SaveHighlighted, this);
        EventLocator.Subscribe<IInMenu>(SaveInMenuOrInGame, this);
        EventLocator.Subscribe<IHotKeyPressed>(HotKeyPressed, this);
        EventLocator.Subscribe<IOnStart>(CanStart, this);
    }

    public void RemoveFromEvents()
    {
        EventLocator.Unsubscribe<IAllowKeys>(SaveAllowKeys);
        EventLocator.Unsubscribe<IHighlightedNode>(SaveHighlighted);
        EventLocator.Unsubscribe<IInMenu>(SaveInMenuOrInGame);
        EventLocator.Unsubscribe<IHotKeyPressed>(HotKeyPressed);
        EventLocator.Unsubscribe<IOnStart>(CanStart);
    }
    
    private void OnDisable()
    {
        foreach (var nodeFunctionBase in _activeFunctions)
        {
            nodeFunctionBase.OnDisable();
        }
        _nodeBase.OnDisable();
    }

    private void Start() => _nodeBase.Start();

    public void SetNodeAsActive()
    {
        if (_disabledNode.NodeIsDisabled() || !_canStart) return;
        
        if (_allowKeys && _inMenu)
        {
            _nodeBase.OnEnter(false);
            SetAsHighlighted();
        }
        else
        {
            SetNotHighlighted();
        }
    }

    public void DoPress() => _uiNodeEvents.DoIsPressed();

    public void DeactivateNode() => _nodeBase.DeactivateNode();

    public void SetAsHighlighted() 
    {
        if (_disabledNode.IsDisabled) return;
        _uiNodeEvents.DoWhenPointerOver(_nodeBase.PointerOverNode);
        ThisNodeIsHighLighted();
    }

    public void SetNotHighlighted()
    {
        _nodeBase.PointerOverNode = false;
        _uiNodeEvents.DoWhenPointerOver(_nodeBase.PointerOverNode);
    }

    public void SetNodeAsSelected_NoEffects() => SetSelectedStatus(true, SetNotHighlighted);

    public void SetNodeAsNotSelected_NoEffects()
    {
        _uiNodeEvents.DoMuteAudio();
        SetSelectedStatus(false, SetNotHighlighted);
    }

    public void SetSelectedStatus(bool isSelected, Action endAction)
    {
        IsSelected = isSelected;
        _uiNodeEvents.DoIsSelected(IsSelected);
        endAction.Invoke();
    }
    
    public void ThisNodeIsSelected() => DoSelected?.RaiseEvent(this);

    public void ThisNodeIsHighLighted() => DoHighlighted?.RaiseEvent(this);
    
    //Input Interfaces
    public void OnPointerEnter(PointerEventData eventData) => _nodeBase.OnEnter(IsDragEvent(eventData));
    public void OnPointerExit(PointerEventData eventData) => _nodeBase.OnExit(IsDragEvent(eventData));
    public void OnPointerDown(PointerEventData eventData) => PressedActions(IsDragEvent(eventData));
    public void OnSubmit(BaseEventData eventData) => PressedActions(false);
    public void OnMove(AxisEventData eventData) => DoMoveToNextNode(eventData.moveDir);
    
    public void OnPointerUp(PointerEventData eventData) { }
    private static bool IsDragEvent(PointerEventData eventData) => eventData.pointerDrag;
    
    private void PressedActions(bool isDragEvent)
    {
        if (_disabledNode.IsDisabled) return;
        if (_allowKeys) _nodeBase.PointerOverNode = false;
        _nodeBase.OnSelected(isDragEvent);
    }

    private void DoMoveToNextNode(MoveDirection moveDirection) => _uiNodeEvents.DoOnMove(moveDirection);

    public void CheckIfMoveAllowed(MoveDirection moveDirection)
    {
        if(!MyBranch.CanvasIsEnabled)return;
        
        if (_disabledNode.IsDisabled)
        {
            DoMoveToNextNode(moveDirection);
        }
        else
        {
            _nodeBase.OnEnter(false);
        }
    }

    private void HotKeyPressed(IHotKeyPressed args)
    {
        if (args.ParentNode != this) return;
        ThisNodeIsSelected();
        ThisNodeIsHighLighted();
        SetNodeAsSelected_NoEffects();
    }
}
