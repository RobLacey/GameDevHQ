using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using NaughtyAttributes;

[RequireComponent(typeof(Slider))]
[RequireComponent(typeof(RectTransform))]

public partial class UINode : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler,
                              IMoveHandler, IPointerUpHandler, ISubmitHandler, IPointerExitHandler, ISelectHandler
{
    [Header("Main Settings")]
    [HorizontalLine(4, color: EColor.Blue, order = 1)]
    [SerializeField] [Label("Is Cancel or Back Button")] 
    bool _isCancelOrBack;
    [SerializeField] [ShowIf("_isCancelOrBack")] 
    EscapeKey _escapeKeyFunction = EscapeKey.GlobalSetting;
    [SerializeField] [HideIf("_isCancelOrBack")] [ValidateInput("SetChildBranch")] [Label("Button/Toggle Function")] 
    ButtonFunction _buttonFunction;
    [SerializeField] [HideIf(EConditionOperator.Or, "GroupSettings", "_isCancelOrBack")] 
    ToggleGroup _toggleGroupId = ToggleGroup.None;
    [SerializeField] [HideIf(EConditionOperator.Or, "GroupSettings", "_isCancelOrBack")] 
    bool _startAsSelected;

    [Header("Settings (Click Arrows To Expand)")]
    [HorizontalLine(4, color: EColor.Blue, order = 1)]
    [SerializeField] [Label("UI Functions To Use")] 
    public Setting _enabledFunctions;
    [SerializeField] [Label("Navigation And On Click Calls")] [ShowIf("UseNavigation")] 
    UINavigation _navigation;
    [SerializeField] [Label("Colour Settings")] [ShowIf("NeedColour")] 
    UIColour _colours;
    [SerializeField] [Label("Invert Colour when Highlighted or Selected")] [ShowIf("NeedInvert")] 
    UIInvertColours _invertColourCorrection;
    [SerializeField] [Label("Swap Images or Text on Select or Highlight")] [ShowIf("NeedSwap")] 
    UIImageTextToggle _swapImageOrText;
    [SerializeField] [Label("Size And Position Effect Settings")] [ShowIf("NeedSize")] 
    UISizeAndPosition _sizeAndPos;
    [SerializeField] [Label("Accessories, Outline Or Shadow Settings")] [ShowIf("NeedAccessories")] 
    UIAccessories _accessories;
    [SerializeField] [Label("Audio Settings")] [ShowIf("NeedAudio")] 
    UIAudio _audio;
    [SerializeField] [Label("Tooltip Settings")] [ShowIf("NeedTooltip")] 
    UITooltip _tooltips;
    [SerializeField] [Label("Event Settings")] [ShowIf("NeedEvents")] 
    UIEvents _events;

    //Variables
    private bool _isDisabled;
    private RectTransform _rectForTooltip;
    private UIToggles _toggleGroups;
    //private bool _pointerOver;
    private bool _inMenu;
    private bool _allowKeys;
    private UIDataEvents _uiDataEvents;
    private UIControlsEvents _uiControlsEvents;
    private bool _holdState;
    private UINode _lastHighlighted;
    private readonly UiActions _uiActions = new UiActions();

    //Delegates
    //private Action<UIEventTypes, bool> _startUiFunctions;

    //Events
    public static event Action<EscapeKey> DoCancelButtonPressed;
    public static event Action<UINode> DoHighlighted; 
    public static event Action<UINode> DoSelected;
    
    //Properties & Enums
    public Setting ActiveFunctions => _enabledFunctions;
    public Slider AmSlider { get; private set; }
    public ButtonFunction Function => _buttonFunction;
    public ToggleGroup Id => _toggleGroupId;
    public UIBranch MyBranch { get; private set; }
    public bool IsSelected { get; set; }
    public UIBranch HasChildBranch => _navigation.Child;
    public UINavigation Navigation => _navigation;
    public UIAudio Audio => _audio;
    private bool NotActiveSlider => IsDisabled || _isCancelOrBack || !AmSlider || _allowKeys;
    private bool IsToggleGroup => _buttonFunction == ButtonFunction.ToggleGroup;
    private bool IsToggleNotLinked => _buttonFunction == ButtonFunction.ToggleNotLinked;
    private bool CanGoToChildBranch => HasChildBranch & _navigation.CanNavigate;

    private void SaveInMenu(bool isInMenu)
    {
        _inMenu = isInMenu;
        if (!_inMenu)
        {
            SetNotHighlighted();
        }
        else if (_lastHighlighted == this)
        {
            //ThisNodeIsHighLighted();
            SetAsHighlighted();
        }
    }
    
    private void SaveAllowKeys(bool allow)
    {
        _allowKeys = allow;
        if(!_allowKeys && _lastHighlighted == this)
            SetNotHighlighted();
    }

    private void SaveGameIsPaused(bool isPaused)
    {
        if (MyBranch.CanvasIsEnabled && isPaused)
        {
            _holdState = true;
        }
        else
        {
            _holdState = false;
        }
    }

    private void SaveLastSelected(UINode newNode) // TODO Use to loose set not selected
    {
        if (DeselectNodeNotAllowed()) return;

        if (NewNodeIsNotBranchesChild(newNode))
        {
            Deactivate();
            SetNotSelected_NoEffects();
        }
        bool DeselectNodeNotAllowed()
            => newNode == this || !IsSelected || IsToggleGroup || IsToggleNotLinked || _holdState;

        bool NewNodeIsNotBranchesChild(UINode uiNode) => uiNode.MyBranch.MyParentBranch.LastSelected != this;
    }

    private void SaveHighLighted(UINode newNode) //TODO Review
    {
        _lastHighlighted = newNode;
    }

    public bool IsDisabled
    {
        get => _isDisabled;
        private set
        {
            _isDisabled = value;
            HandleIfDisabled(); 
        }
    }

    private void Awake()
    {
        _rectForTooltip = GetComponent<RectTransform>();
        AmSlider = GetComponent<Slider>();
        MyBranch = GetComponentInParent<UIBranch>();
        _uiDataEvents = new UIDataEvents();
        _uiControlsEvents = new UIControlsEvents();
        SetUpUiFunctions();
        _toggleGroups = new UIToggles(this, _buttonFunction, _startAsSelected);
    }

    private void SetUpUiFunctions()
    {
        _colours.OnAwake(this, _uiActions);
        _events.OnAwake(this, _uiActions);
        _accessories.OnAwake(this, _uiActions);
        _invertColourCorrection.OnAwake(this, _uiActions);
        _swapImageOrText.OnAwake(this, _uiActions);
        _sizeAndPos.OnAwake(this, _uiActions);
        
        _audio.OnAwake(_enabledFunctions);
        _navigation.OnAwake(this, MyBranch, _enabledFunctions);
        _tooltips.OnAwake(_enabledFunctions, gameObject.name);
    }

    private void OnEnable()
    {

        _uiDataEvents.SubscribeToInMenu(SaveInMenu);
        _uiControlsEvents.SubscribeToAllowKeys(SaveAllowKeys);
        _uiDataEvents.SubscribeToSelectedNode(SaveLastSelected);
        _uiDataEvents.SubscribeToHighlightedNode(SaveHighLighted);
        _uiDataEvents.SubscribeToGameIsPaused(SaveGameIsPaused);
    }

    private void OnDisable()
    {
        _colours.OnDisable(_uiActions);
        _accessories.OnDisable(_uiActions);
        _events.OnDisable(_uiActions);
        _invertColourCorrection.OnDisable(_uiActions);
        _swapImageOrText.OnDisable(_uiActions);
        
        //_startUiFunctions -= _sizeAndPos.OnDisable();
    }

    private void Start()
    {
        if (AmSlider) AmSlider.interactable = false;
        _toggleGroups.SetUpToggleGroup(MyBranch.ThisGroupsUiNodes);
    }
    
    public void SetNodeAsActive()
    {
        if (IsDisabled)
        {
            HandleIfDisabled(); 
            return;
        }

        _lastHighlighted = this;
        
        if (_allowKeys && _inMenu)
        {
            SetAsHighlighted();
        }
        else
        {
            ThisNodeIsHighLighted();
            SetNotHighlighted();
        }
    }

    public void PressedActions()
    {
        if (IsDisabled) return;
        HandleAudio();

        if (_isCancelOrBack) 
        {
            DoCancelButtonPressed?.Invoke(_escapeKeyFunction); 
            return; 
        }

        TurnNodeOnOff();
    }

    private void HandleAudio()
    {
        //_audio.Play(IsSelected ? UIEventTypes.Cancelled : UIEventTypes.Selected);
    }

    private void TurnNodeOnOff()
    {
        _toggleGroups.ToggleGroupElements();

        if (IsSelected)
        {
            if (IsToggleGroup && MyBranch.LastHighlighted == this) return;
            Deactivate();
            MyBranch.MoveToBranchWithoutTween();
        }
        else
        {
            Activate();
            Debug.Log(IsSelected + " from press");
        }
        _uiActions._isSelected?.Invoke(IsSelected);
        _uiActions._isPressed?.Invoke();
        //_startUiFunctions.Invoke(UIEventTypes.Selected, IsSelected);
        //_sizeAndPos.WhenPressed(IsSelected);
        //_swapImageOrText.CycleToggle(IsSelected);
    }

    public void Deactivate()
    {
        if (!IsSelected) return;
        
        IsSelected = false;
        if (CanGoToChildBranch)
        {
            _navigation.TurnOffChildren();
        }
        SetSlider(false);
    }

    private void Activate()
    {
        IsSelected = true;
        StopAllCoroutines();
        _tooltips.HideToolTip();
        SetSlider(true);
        if (CanGoToChildBranch) 
            MoveToChildBranch();
    }
    
    private void MoveToChildBranch()
    {
        ThisNodeIsSelected();
        _navigation.StartMoveToChild();
    }

    public void SetAsHighlighted()
    {
        if (IsDisabled) return;
        _uiActions._whenPointerOver?.Invoke(true);

        ThisNodeIsHighLighted();
        //_startUiFunctions.Invoke(UIEventTypes.Highlighted, IsSelected);
        StartCoroutine(_tooltips.StartToolTip(_rectForTooltip));
    }

    public void SetNotHighlighted()
    {
        StopAllCoroutines();
        _tooltips.HideToolTip();
        _uiActions._whenPointerOver?.Invoke(false);

        
       // if (IsDisabled) return;

        // if (IsSelected)
        // {
        //     if (_pointerOver)
        //     {
        //         //_startUiFunctions.Invoke(UIEventTypes.Selected, IsSelected);
        //     }
        //     else
        //     {
        //         //_startUiFunctions.Invoke(UIEventTypes.Normal, IsSelected);
        //     }
        // }
        // else
        // {
        //     if (_pointerOver)
        //     {
        //         _startUiFunctions.Invoke(UIEventTypes.Highlighted, IsSelected);
        //     }
        //     else
        //     {
        //         _startUiFunctions.Invoke(UIEventTypes.Normal, IsSelected);
        //     }
        // }
    }

    public void SetSelected_NoEffects() //TODO Review - Add DoSelect
    {
        IsSelected = true;
        _uiActions._isSelected?.Invoke(true);
        //_uiActions._isPressed?.Invoke();
        SetNotHighlighted();
        //_sizeAndPos.WhenPressed(IsSelected);
        //_swapImageOrText.CycleToggle(IsSelected);
    }

    public void SetNotSelected_NoEffects()
    {
        IsSelected = false;
        _uiActions._isSelected?.Invoke(false);
        //_uiActions._isPressed?.Invoke();
        SetNotHighlighted();
        //_sizeAndPos.WhenPressed(IsSelected);
        //_swapImageOrText.CycleToggle(IsSelected);
    }

    private void HandleIfDisabled()
    {
        Deactivate();
        //_startUiFunctions.Invoke(UIEventTypes.Normal, IsSelected);
       _uiActions._isDisabled?.Invoke(_isDisabled);
    }

    private void SetSlider(bool selected)
    {
        if (!AmSlider) return;
        IsSelected = selected;
        AmSlider.interactable = IsSelected;
    }

    // ReSharper disable once UnusedMember.Global - Used to Disable Object
    public void DisableObject() => IsDisabled = true; 
    // ReSharper disable once UnusedMember.Global - Used to Enable Object
    public void EnableObject() => IsDisabled = false; 

    public void ThisNodeIsSelected() => DoSelected?.Invoke(this);

    public void ThisNodeIsHighLighted() => DoHighlighted?.Invoke(this);
}


