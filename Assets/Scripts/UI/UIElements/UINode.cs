using UnityEngine;
using System.Collections.Generic;
using UIElements;
using NaughtyAttributes;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]

public partial class UINode : MonoBehaviour, INode, IPointerEnterHandler, IPointerDownHandler,
                              IMoveHandler, IPointerUpHandler, ISubmitHandler, IPointerExitHandler, IEventUser
{
    [Header("Main Settings")]
    [HorizontalLine(1, color: EColor.Blue, order = 1)]
    [SerializeField] 
    [Label("Button/Toggle Function")] 
    private ButtonFunction _buttonFunction;
    [SerializeField]
    [ShowIf(CanAutoOpenClose)] [Label("Auto Open/Close Override")]
    private IsActive _autoOpenCloseOverride = IsActive.No;
    [SerializeField]
    [ShowIf(CanAutoOpenClose)] [Label("Auto Open/Close Delay")]
    [Range(0, 1)]private float _autoOpenDelay = 0;
    [SerializeField] 
    [ShowIf(CancelOrBack)] 
    private EscapeKey _escapeKeyFunction = EscapeKey.GlobalSetting;
    [SerializeField] 
    [HideIf(EConditionOperator.Or, ShowGroupSettings, CancelOrBack)]
    private ToggleData _toggleData;

    [Header("Tween Data", order = 2)] [Space(20, order = 1)]
    [HorizontalLine(1, EColor.Blue , order = 3)]

    [SerializeField] 
    [Label("UI Functions To Use")]
    private Setting _enabledFunctions;
    [SerializeField] 
    [Space(15f)]  [Label("Navigation And On Click Calls")] [ShowIf(HasNavigation)]  
    private NavigationSettings _navigation;
    [SerializeField] 
    [Space(5f)]  [Label("Colour Settings")] [ShowIf(HasColour)]  
    private ColourSettings _coloursTest;
    [SerializeField] 
    [Space(5f)]  [Label("Invert Colour when Highlighted or Selected")] [ShowIf(HasInvert)]  
    private InvertColoursSettings _invertColourCorrection;
    [SerializeField] 
    [Space(5f)]  [Label("Swap Images or Text on Select or Highlight")] [ShowIf(HasSwap)] 
    private SwapImageOrTextSettings _swapImageOrText;
    [SerializeField] 
    [Space(5f)]  [Label("Size And Position Effect Settings")] [ShowIf(HasSize)]  
    private SizeAndPositionSettings _sizeAndPos;
    [SerializeField] 
    [Space(5f)]  [Label("Accessories, Outline Or Shadow Settings")] [ShowIf(HasAccessories)]  
    private AccessoriesSettings _accessories;
    [SerializeField] 
    [Space(5f)]  [Label("Audio Settings")] [ShowIf(HasAudio)]  
    private AudioSettings _audio;
    [SerializeField] 
    [Space(5f)]  [Label("Tooltip Settings")] [ShowIf(HasTooltip)]  
    private ToolTipSettings _tooltips;
    [SerializeField] 
    [Space(5f)]  [Label("Event Settings")] [ShowIf(HasEvents)]  
    private EventSettings _events;

    //Variables
    private bool _childIsMoving, _setUpFinished;
    private IUiEvents _uiNodeEvents;
    private INodeBase _nodeBase;
    private readonly List<NodeFunctionBase> _activeFunctions = new List<NodeFunctionBase>();
    private bool _canStart;
    private bool _allowKeys;
    private InputScheme _inputScheme;

    //Properties
    public RectTransform MainCanvas => FindObjectOfType<UIHub>().GetComponent<RectTransform>();
    public bool IsToggleGroup => _buttonFunction == ButtonFunction.ToggleGroup;
    private bool IsToggleNotLinked => _buttonFunction == ButtonFunction.ToggleNotLinked;
    private bool IsCancelOrBack => _buttonFunction == ButtonFunction.CancelOrBack;
    public bool CanNotStoreNodeInHistory => IsToggleGroup || IsToggleNotLinked || HasChildBranch is null;
    public ToggleData ToggleData => _toggleData;
    public IBranch MyBranch { get; private set; }
    public IBranch HasChildBranch
    {
        get => _navigation.ChildBranch;
        set => _navigation.ChildBranch = value;
    }
    private bool SetIfCanNavigate() => IsAToggle() || IsCancelOrBack;
    private bool IsAToggle() => _buttonFunction == ButtonFunction.ToggleGroup
                                || _buttonFunction == ButtonFunction.ToggleNotLinked;

    public EscapeKey EscapeKeyType => _escapeKeyFunction;
    public GameObject ReturnGameObject => gameObject;
    public IsActive AutoOpenCloseOverride => _autoOpenCloseOverride;
    public float AutoOpenDelay => _autoOpenDelay;
    public IUiEvents UINodeEvents => _uiNodeEvents;
    public InputScheme InputScheme => _inputScheme;
    
    //Setting / Getters
    private void CanStart(IOnStart args) => _canStart = true;
    private void AllowKeys(IAllowKeys args) => _allowKeys = args.CanAllowKeys;
    
    //Main
    private void Awake()
    {
        _inputScheme = FindObjectOfType<UIInput>().ReturnScheme;
        MyBranch = GetComponentInParent<IBranch>();
        _uiNodeEvents = new UiEvents(gameObject.GetInstanceID(), this);
        if (IsToggleGroup || IsToggleNotLinked)
            HasChildBranch = null;
        ObserveEvents();
    }
    
    public void ObserveEvents()
    {
        EVent.Do.Subscribe<IOnStart>(CanStart);
        EVent.Do.Subscribe<IAllowKeys>(AllowKeys);
    }

    private void OnEnable()
    {
        SetUpUiFunctions();
        StartNodeFactory();
        _nodeBase.OnEnable();
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

    private void OnDisable()
    {
        foreach (var nodeFunctionBase in _activeFunctions)
        {
            nodeFunctionBase.OnDisable();
        }
    }

    private void Start()
    {
        SetChildParentBranch();        
        _nodeBase.Start();
        
        foreach (var func in _activeFunctions)
        {
            func.Start();
        }
    }

    private void SetChildParentBranch()
    {
        if(_buttonFunction == ButtonFunction.InGameUi) return;
        if (HasChildBranch.IsNotNull())
        {
            HasChildBranch.MyParentBranch = MyBranch;
        }
    }

    private void StartNodeFactory()
    {
        _nodeBase = NodeFactory.Factory(_buttonFunction, this);
        _nodeBase.Navigation = _navigation.Instance;
    }

    public void SetNodeAsActive() => _nodeBase.SetNodeAsActive();
    public void SetAsHotKeyParent(bool setAsActive) => _nodeBase.HotKeyPressed(setAsActive);

    public void DeactivateNode() => _nodeBase.DeactivateNodeByType();

    public void ClearNode() => _nodeBase.ClearNodeCompletely();

    // Use To Disable Node from external scripts
    public void DisableNode() => _nodeBase.DisableNode();

    public void EnableNode() => _nodeBase.EnableNode();

    public void ThisNodeIsHighLighted() => _nodeBase.ThisNodeIsHighLighted();
    
    //Input Interfaces
    public void OnPointerEnter(PointerEventData eventData) => _nodeBase.OnEnter();

    public void OnPointerExit(PointerEventData eventData) => _nodeBase.OnExit();
    public void OnPointerDown(PointerEventData eventData) => _nodeBase.SelectedAction();

    public void OnSubmit(BaseEventData eventData)
    {
        if(!_allowKeys) return;
        _nodeBase.SelectedAction();
    }
    public void OnMove(AxisEventData eventData)
    {
        if(!_canStart || !_allowKeys) return;
        _nodeBase.DoMoveToNextNode(eventData.moveDir);
    }
    public void OnPointerUp(PointerEventData eventData) { }

    public void DoNonMouseMove(MoveDirection moveDirection) => _nodeBase.DoNonMouseMove(moveDirection);

    /// <summary>
    /// Needed maybe for when sliders and scroll bar are used
    /// </summary>
    /// <param name="eventData"></param>
    /// <returns></returns>
    private static bool IsDragEvent(PointerEventData eventData) => eventData.pointerDrag;
}
