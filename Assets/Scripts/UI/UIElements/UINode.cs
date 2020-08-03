using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using NaughtyAttributes;

[RequireComponent(typeof(Slider))]
[RequireComponent(typeof(RectTransform))]

public partial class UINode : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler,
                              IMoveHandler, IPointerUpHandler, ISubmitHandler, IPointerExitHandler
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
    [SerializeField] [EnumFlags] [Label("UI Functions To Use")] 
    public Setting _enabledFunctions;
    [SerializeField] [Label("Navigation And On Click Calls")] [ShowIf("UseNavigation")] 
    UINavigation _navigation;
    [SerializeField] [Label("Colour Settings")] [ShowIf("NeedColour")] 
    UIColour _colours;
    [SerializeField] [Label("Invert Colour when Highlighted or Selected")] [ShowIf("NeedInvert")] 
    UIInvertColours _invertColourCorrection;
    [SerializeField] [Label("Swap Images, Text or SetUp Toggle Image List")] [ShowIf("NeedSwap")] 
    UISwapper _swapImageOrText;
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
    private bool _pointerOver;
    private bool _inMenu;

    //Delegates
    private Action<UIEventTypes, bool> _startUiFunctions;
    public static event Action<EscapeKey> DoCancel;
    public static event Action<UINode> DoHighlighted; 
    public static event Action<UINode> DoSelected; 

    //Properties & Enums
    public Slider AmSlider { get; private set; }
    public ButtonFunction Function => _buttonFunction;
    public ToggleGroup Id => _toggleGroupId;
    public UIBranch MyBranch { get; private set; }
    public bool IsSelected { get; set; }
    public UIBranch HasChildBranch => _navigation.Child;
    public UINavigation Navigation => _navigation;
    public UIAudio Audio => _audio;
    private bool NotActiveSlider => IsDisabled || _isCancelOrBack || !AmSlider || MyBranch.AllowKeys;
    private bool IsToggleGroup => _buttonFunction == ButtonFunction.ToggleGroup;
    private bool IsToggleNotLinked => _buttonFunction == ButtonFunction.Toggle_NotLinked;
    private bool CanGoToChildBranch => HasChildBranch & _navigation.CanNaviagte;
    private void SetInMenu(bool isInMenu) => _inMenu = isInMenu;


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
        SetUpUiFunctions();
        _toggleGroups = new UIToggles(this, _buttonFunction, _startAsSelected);
    }

    private void SetUpUiFunctions()
    {
        _audio.OnAwake(_enabledFunctions);
        _colours.OnAwake(gameObject.GetInstanceID(), _enabledFunctions);
        _tooltips.OnAwake(_enabledFunctions, gameObject.name);
        _events.OnAwake(_enabledFunctions);
        _navigation.OnAwake(this, MyBranch, _enabledFunctions);
    }

    private void OnEnable()
    {
        _startUiFunctions += _accessories.OnAwake(_enabledFunctions);
        _startUiFunctions += _sizeAndPos.OnAwake(transform, _enabledFunctions);
        _startUiFunctions += _swapImageOrText.OnAwake(IsSelected, _enabledFunctions);
        _startUiFunctions += _invertColourCorrection.OnAwake(_enabledFunctions);
        UIHub.SetInMenu += SetInMenu;
    }

    private void OnDisable()
    {
        _startUiFunctions -= _accessories.OnDisable();
        _startUiFunctions -= _sizeAndPos.OnDisable();
        _startUiFunctions -= _swapImageOrText.OnDisable();
        _startUiFunctions -= _invertColourCorrection.OnDisable();
        UIHub.SetInMenu -= SetInMenu;
    }

    private void Start()
    {
        if (AmSlider) AmSlider.interactable = false;
        _navigation.SetChildsParentBranch();
        _toggleGroups.SetUpToggleGroup(MyBranch.ThisGroupsUiNodes);
        
        if (MyBranch.IsAPopUpBranch() || MyBranch.IsPauseMenuBranch()) 
            _escapeKeyFunction = EscapeKey.BackOneLevel;

        if (_colours.CanActivate && _colours.NoSettings)
        {
            Debug.LogError("No Image or Text set on Colour settings on " + gameObject.name);
        }
    }

    private void InvokeClickEvents()
    {
        if (!_events.CanActivate) return;
        _events._OnButtonClickEvent?.Invoke();
        _events._OnToggleEvent?.Invoke(IsSelected);
    }

    public void SetNodeAsActive()
    {
        if (IsDisabled)
        {
            HandleIfDisabled(); 
            return;
        }
        DoHighlighted?.Invoke(this);

        if ( MyBranch.AllowKeys && _inMenu)
        {
            SetAsHighlighted();
        }
        else
        {
            SetNotHighlighted();
        }
    }

    public void PressedActions()
    {
        if (IsDisabled) return;
        HandleAudio();

        if (_isCancelOrBack) 
        {
            DoCancel?.Invoke(_escapeKeyFunction); 
            return; 
        }

        TurnNodeOnOff();
    }

    private void HandleAudio()
    {
        _audio.Play(IsSelected ? UIEventTypes.Cancelled : UIEventTypes.Selected);
    }

    private void TurnNodeOnOff()
    {
        _toggleGroups.ToggleGroupElements();

        if (IsSelected)
        {
            if (IsToggleGroup && MyBranch.LastHighlighted == this) return;
            if (IsToggleNotLinked) { IsSelected = false; }
            Deactivate();
            DoSelected?.Invoke(MyBranch.MyParentBranch.LastSelected);
            //MyBranch.SaveLastSelected(MyBranch.MyParentBranch.LastSelected);
        }
        else
        {
            Activate();
            //DoSelected?.Invoke(this);
        }

        _startUiFunctions.Invoke(UIEventTypes.Selected, IsSelected);
        _sizeAndPos.WhenPressed(IsSelected);
        _colours.ProcessPress(IsSelected);
        _swapImageOrText.CycleToggle(IsSelected);
    }

    public void Deactivate()
    {
        if (!IsSelected) return;
        
        if (CanGoToChildBranch)
        {
            IsSelected = false;
            _navigation.TurnOffChildren();
        }
        if(!_pointerOver) SetNotHighlighted();
        SetSlider(false);
    }

    private void Activate()
    {
        IsSelected = true;
        StopAllCoroutines();
        _tooltips.HideToolTip();
        SetSlider(true);
        if(!AmSlider) InvokeClickEvents();
        if (CanGoToChildBranch) MoveToChildBranch();
    }
    
    private void MoveToChildBranch()
    {
        DoSelected?.Invoke(this);

        //MyBranch.SaveLastSelected(this);

        if (MyBranch.WhenToMove == WhenToMove.AfterEndOfTween)
        {
            _navigation.MoveAfterTween();
        }
        else
        {
            _navigation.MoveOnClick();
        }
    }

    public void SetAsHighlighted()
    {
        if (IsDisabled) return;
        _colours.SetColourOnEnter(IsSelected);
        _startUiFunctions.Invoke(UIEventTypes.Highlighted, IsSelected);
        StartCoroutine(_tooltips.StartToolTip(MyBranch, _rectForTooltip));
        DoHighlighted?.Invoke(this);
    }

    public void SetNotHighlighted()
    {
        StopAllCoroutines();
        _tooltips.HideToolTip();

        if (IsDisabled) return;

        if (IsSelected)
        {
            if (_pointerOver)
            {
                _startUiFunctions.Invoke(UIEventTypes.Selected, IsSelected);
            }
            else
            {
                _startUiFunctions.Invoke(UIEventTypes.Normal, IsSelected);
            }
            _colours.SetColourOnExit(IsSelected);
        }
        else
        {
            if (_pointerOver)
            {
                _startUiFunctions.Invoke(UIEventTypes.Highlighted, IsSelected);
                _colours.SetColourOnEnter(IsSelected);
            }
            else
            {
                _startUiFunctions.Invoke(UIEventTypes.Normal, IsSelected);
                _colours.ResetToNormalColour();
            }
        }
    }

    public void SetSelected_NoEffects() //TODO Review - Add DoSelect
    {
        IsSelected = true;
        SetNotHighlighted();
        _sizeAndPos.WhenPressed(IsSelected);
        _swapImageOrText.CycleToggle(IsSelected);
    }

    public void SetNotSelected_NoEffects()
    {
        IsSelected = false;
        SetNotHighlighted();
        _sizeAndPos.WhenPressed(IsSelected);
        _swapImageOrText.CycleToggle(IsSelected);
    }

    private void HandleIfDisabled()
    {
        Deactivate();
        _startUiFunctions.Invoke(UIEventTypes.Normal, IsSelected);

        if (_isDisabled)
        {
            _colours.SetAsDisabled();  
        }
        else
        {
            _colours.ResetToNormalColour();
        }
    }

    private void SetSlider(bool selected)
    {
        if (!AmSlider) return;
        IsSelected = selected;
        AmSlider.interactable = IsSelected;
    }

    // ReSharper disable once UnusedMember.Global - Used to Disable Object
    public void DisableObject() { IsDisabled = true; }
    // ReSharper disable once UnusedMember.Global - Used to Enable Object
    public void EnableObject() { IsDisabled = false; }

    public void TriggerExitEvent()
    {
        if (_events.CanActivate) 
            _events.OnExitEvent?.Invoke();
    }

    public void TriggerEnterEvent()
    {
        if (_events.CanActivate) 
            _events.OnEnterEvent?.Invoke();
    }

    public void SetAsSelected()
    {
        DoSelected?.Invoke(this);
    }
    
    public void SetThisAsHighLighted()
    {
        DoHighlighted?.Invoke(this);
    }
}


