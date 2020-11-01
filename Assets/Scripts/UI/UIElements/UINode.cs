using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using NaughtyAttributes;

[RequireComponent(typeof(RectTransform))]

public partial class UINode : MonoBehaviour, INode, IEventUser, ICancelButtonActivated,
                              IHighlightedNode, ISelectedNode
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
    private ToggleGroup _toggleGroupId = ToggleGroup.None;
    [SerializeField]
    [HideIf(EConditionOperator.Or, "GroupSettings", "IsCancelOrBack")] 
    private UIBranch _tabBranch;
    [SerializeField] 
    [ShowIf(EConditionOperator.Or, "IsHoverToActivate")] 
    private bool _closeHoverOnExit;
    [SerializeField] 
    [HideIf(EConditionOperator.Or, "IsCancelOrBack")] 
    private bool _startAsSelected;
    [SerializeField] 
    [Space(15f)] [Label("UI Functions To Use")]  
    private Setting _enabledFunctions;
    [HorizontalLine(1, color: EColor.Blue, order = 1)]
    [SerializeField] 
    /*[Space(15f)] */[Label("Navigation And On Click Calls")] [ShowIf("UseNavigation")]  
    private NavigationSettings _navigation;
    [SerializeField] 
    [Space(5f)] [Label("Colour Settings")] [ShowIf("NeedColour")]  
    private ColourSettings _coloursTest;
    [SerializeField] 
    [Space(5f)] [Label("Invert Colour when Highlighted or Selected")] 
    [ShowIf("NeedInvert")]  
    private InvertColoursSettings _invertColourCorrection;
    [SerializeField] 
    [Space(5f)] [Label("Swap Images or Text on Select or Highlight")] 
    [ShowIf("NeedSwap")] 
    private SwapImageOrTextSettings _swapImageOrText;
    [SerializeField] 
    [Space(5f)] [Label("Size And Position Effect Settings")] 
    [ShowIf("NeedSize")]  
    private SizeAndPositionSettings _sizeAndPos;
    [SerializeField] 
    [Space(5f)] [Label("Accessories, Outline Or Shadow Settings")] 
    [ShowIf("NeedAccessories")]  
    private AccessoriesSettings _accessories;
    [SerializeField] 
    [Space(5f)] [Label("Audio Settings")] [ShowIf("NeedAudio")]  
    private AudioSettings _audio;
    [SerializeField] 
    [Space(5f)] [Label("Tooltip Settings")] [ShowIf("NeedTooltip")]  
    private ToolTipSettings _tooltips;
    [SerializeField] 
    [Space(5f)] [Label("Event Settings")] [ShowIf("NeedEvents")]  
    private EventSettings _events;

    //Variables
    private bool _inMenu, _allowKeys;
    private INode _lastHighlighted;
    private UiActions _uiActions;
    private IDisable _disable;
    private INodeBase _nodeBase;
    private readonly List<NodeFunctionBase> _activeFunctions = new List<NodeFunctionBase>();
    private bool _pointerOver;

    //Events
    private static CustomEvent<IHighlightedNode> DoHighlighted { get; } 
        = new CustomEvent<IHighlightedNode>();
    private static CustomEvent<ISelectedNode> DoSelected { get; } 
        = new CustomEvent<ISelectedNode>();
    
    //Properties & Enums
    public bool IsToggleGroup => _buttonFunction == ButtonFunction.ToggleGroup;
    public bool IsToggleNotLinked => _buttonFunction == ButtonFunction.ToggleNotLinked;
    private bool IsHoverToActivate => _buttonFunction == ButtonFunction.HoverToActivate;
    public bool DontStoreTheseNodeTypesInHistory => IsToggleGroup || IsToggleNotLinked || !HasChildBranch;
    private bool IsCancelOrBack => _buttonFunction == ButtonFunction.CancelOrBack;
    public bool IsSelected { get; set; }
    private Slider AmSlider { get; set; }
    public ToggleGroup ToggleGroupId => _toggleGroupId;
    public UIBranch MyBranch { get; private set; }
    public UIBranch HasChildBranch => _navigation.ChildBranch;
    public UINode ReturnNode => this;
    public EscapeKey EscapeKeyType => _escapeKeyFunction;
    public INode Highlighted => this;
    public INode Selected => this;
    public ToggleGroup ToggleGroupID => _toggleGroupId;
    public UIBranch ToggleBranch => _tabBranch;
    public bool StartAsSelected => _startAsSelected;
    public UINavigation Navigation => _navigation.Instance;

    private void SaveInMenuOrInGame(IInMenu args)
    {
        _inMenu = args.InTheMenu;
        if (!_inMenu)
        {
            SetNotHighlighted();
        }
        else if (_lastHighlighted.ReturnNode == this && _allowKeys)
        {
            SetAsHighlighted();
        }
    }
    
    private void SaveAllowKeys(IAllowKeys args)
    {
        _allowKeys = args.CanAllowKeys;
        if(!_allowKeys && _lastHighlighted.ReturnNode == this)
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
        if (_lastHighlighted.ReturnNode == this && args.Highlighted.ReturnNode != this)
        {
            Debug.Log(this);
            SetNotHighlighted();
        }
    }

    private void HomeGroupChanged(ISwitchGroupPressed args) => _pointerOver = false;
    
    // ReSharper disable once UnusedMember.Global - Assigned in editor to Disable Object
    public void DisableNode()
    {
        _disable.IsDisabled = true;
        _uiActions._isDisabled?.Invoke(_disable.IsDisabled);
    }
    // ReSharper disable once UnusedMember.Global - Assigned in editor to Enable Object
    public void EnableNode()
    {
        _disable.IsDisabled = false;
        _uiActions._isDisabled?.Invoke(_disable.IsDisabled);
    }

    //Main
    private void Awake()
    {
        StartNodeFactory();
        _uiActions = new UiActions(gameObject.GetInstanceID(), this);
        AmSlider = GetComponent<Slider>();
        MyBranch = GetComponentInParent<UIBranch>();
        SetUpUiFunctions();
        ObserveEvents();
    }

    private void StartNodeFactory()
    {
        _disable = new Disable(this);
        _nodeBase = NodeFactory.Factory(_buttonFunction, this);
    }

    private void SetUpUiFunctions()
    {
        _activeFunctions.Add(_coloursTest.SetUp(_uiActions, _enabledFunctions));
        _activeFunctions.Add(_events.SetUp(_uiActions, _enabledFunctions));
        _activeFunctions.Add(_accessories.SetUp(_uiActions, _enabledFunctions));
        _activeFunctions.Add(_invertColourCorrection.SetUp(_uiActions, _enabledFunctions));
        _activeFunctions.Add(_swapImageOrText.SetUp(_uiActions, _enabledFunctions));
        _activeFunctions.Add(_sizeAndPos.SetUp(_uiActions, _enabledFunctions));
        _activeFunctions.Add(_tooltips.SetUp(_uiActions, _enabledFunctions));
        _activeFunctions.Add(_navigation.SetUp(_uiActions, _enabledFunctions));
        _activeFunctions.Add(_audio.SetUp(_uiActions, _enabledFunctions));
    }

    public void ObserveEvents()
    {
        EventLocator.Subscribe<IAllowKeys>(SaveAllowKeys, this);
        EventLocator.Subscribe<IHighlightedNode>(SaveHighlighted, this);
        EventLocator.Subscribe<IInMenu>(SaveInMenuOrInGame, this);
        EventLocator.Subscribe<ISwitchGroupPressed>(HomeGroupChanged, this);
    }

    public void RemoveFromEvents()
    {
        EventLocator.Unsubscribe<IAllowKeys>(SaveAllowKeys);
        EventLocator.Unsubscribe<IHighlightedNode>(SaveHighlighted);
        EventLocator.Unsubscribe<IInMenu>(SaveInMenuOrInGame);
        EventLocator.Unsubscribe<ISwitchGroupPressed>(HomeGroupChanged);
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
        if (AmSlider) AmSlider.interactable = false;
        _nodeBase.Start();
    }
    
    public void SetNodeAsActive()
    {
        if (_disable.NodeIsDisabled() || _pointerOver) return;
        
        if (_allowKeys && _inMenu)
        {
            SetAsHighlighted();
        }
        else
        {
            ThisNodeIsHighLighted();
        }
    }

    private void PressedActions()
    {
        if (_disable.IsDisabled) return;
        _nodeBase.TurnNodeOnOff();
    }

    public void DoPress() => _uiActions._isPressed?.Invoke();
    
    public void PlayCancelAudio() => _uiActions?._canPlayCancelAudio.Invoke();

    public void DeactivateNode() => _nodeBase.DeactivateNode();

    private void SetAsHighlighted() 
    {
        if (_disable.IsDisabled) return;
        _pointerOver = true;
        _uiActions._whenPointerOver?.Invoke(_pointerOver);
        ThisNodeIsHighLighted();
    }

    public void SetNotHighlighted()
    {
        _pointerOver = false;
        _uiActions._whenPointerOver?.Invoke(_pointerOver);
    } 

    public void SetNodeAsSelected_NoEffects() => SetSelectedStatus(true, SetNotHighlighted);

    public void SetNodeAsNotSelected_NoEffects() => SetSelectedStatus(false, SetNotHighlighted);

    public void SetSelectedStatus(bool isSelected, Action endAction)
    {
        IsSelected = isSelected;
        _uiActions._isSelected?.Invoke(IsSelected);
        endAction.Invoke();
    }
    
    private void SetSlider(bool selected)
    {
        if (!AmSlider) return;
        IsSelected = selected;
        AmSlider.interactable = IsSelected;
    }
    
    public void ThisNodeIsSelected() => DoSelected?.RaiseEvent(this);

    public void ThisNodeIsHighLighted() => DoHighlighted?.RaiseEvent(this);
}
