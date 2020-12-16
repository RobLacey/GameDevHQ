using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]

public partial class UINode : MonoBehaviour, INode, IPointerEnterHandler, IPointerDownHandler,
                              IMoveHandler, IPointerUpHandler, ISubmitHandler, IPointerExitHandler, IEventUser
{
    [Header("Main Settings")]
    [HorizontalLine(1, color: EColor.Blue, order = 1)]
    [SerializeField] 
    [ValidateInput("SetChildBranch")] [Label("Button/Toggle Function")] 
    private ButtonFunction _buttonFunction;
    [SerializeField]
    [ShowIf("CanAutoOpenClose")] [Label("Auto Open/Close Override")]
    private IsActive _autoOpenCloseOverride = IsActive.No;
    [SerializeField]
    [ShowIf("CanAutoOpenClose")] [Label("Auto Open/Close Override")]
    private float _autoOpenDelay;
    [SerializeField] 
    [ShowIf("IsCancelOrBack")] 
    private EscapeKey _escapeKeyFunction = EscapeKey.GlobalSetting;
    [SerializeField] 
    [HideIf(EConditionOperator.Or, "GroupSettings", "IsCancelOrBack")]
    private ToggleData _toggleData;
    
    [SerializeField] 
    [BoxGroup("Functions")] [Label("UI Functions To Use")] [HorizontalLine(1, color: EColor.Blue, order = 1)]
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

    //Variables
    private bool _childIsMoving, _setUpFinished;
    private IUiEvents _uiNodeEvents;
    private INodeBase _nodeBase;
    private readonly List<NodeFunctionBase> _activeFunctions = new List<NodeFunctionBase>();
    private bool _canStart;

    //Properties
    public RectTransform MainCanvas => FindObjectOfType<UIHub>().GetComponent<RectTransform>();
    public bool IsToggleGroup => _buttonFunction == ButtonFunction.ToggleGroup;
    private bool IsToggleNotLinked => _buttonFunction == ButtonFunction.ToggleNotLinked;
    private bool IsCancelOrBack => _buttonFunction == ButtonFunction.CancelOrBack;
    public bool CanStoreNodeInHistory => IsToggleGroup || IsToggleNotLinked || HasChildBranch is null;
    public ToggleData ToggleData => _toggleData;
    public IBranch MyBranch { get; private set; }
    public IBranch HasChildBranch
    {
        get => _navigation.ChildBranch;
        set => _navigation.ChildBranch = value;
    }

    public EscapeKey EscapeKeyType => _escapeKeyFunction;
    public GameObject ReturnGameObject => gameObject;
    public IsActive AutoOpenCloseOverride => _autoOpenCloseOverride;
    public float AutoOpenDelay => _autoOpenDelay;
    public IUiEvents UINodeEvents => _uiNodeEvents;
    
    //Setting / Getters
    private void CanStart(IOnStart args) => _canStart = true;
    
    //Main
    private void Awake()
    {
        MyBranch = GetComponentInParent<IBranch>();
        _uiNodeEvents = new UiEvents(gameObject.GetInstanceID(), this);
        if (IsToggleGroup || IsToggleNotLinked)
            HasChildBranch = null;
        ObserveEvents();
    }
    
    public void ObserveEvents() => EVent.Do.Subscribe<IOnStart>(CanStart);

    private void OnEnable() => SetUpUiFunctions();

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
        StartNodeFactory();
        _nodeBase.OnEnable();
        _nodeBase.Start();
    }

    private void SetChildParentBranch()
    {
        if (!(HasChildBranch is null))
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
    public void SetAsHotKeyParent() => _nodeBase.HotKeyPressed();

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
    public void OnSubmit(BaseEventData eventData) => _nodeBase.SelectedAction();
    public void OnMove(AxisEventData eventData)
    {
        if(!_canStart) return;
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
