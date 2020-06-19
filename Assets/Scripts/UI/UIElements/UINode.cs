using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using NaughtyAttributes;

[RequireComponent(typeof(RectTransform))]

public class UINode : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler,
                                     IMoveHandler, IPointerUpHandler, ISubmitHandler, IPointerExitHandler
{
    [Header("Main Settings")]
    [HorizontalLine(4, color: EColor.Blue, order = 1)]
    [SerializeField] [Label("Is Cancel or Back Button")] bool _isCancelOrBackButton;
    [SerializeField] [ShowIf("_isCancelOrBackButton")] EscapeKey _escapeKeyFunction = EscapeKey.GlobalSetting;
    [SerializeField] [HideIf("_isCancelOrBackButton")] [ValidateInput("SetChildBranch")] [Label("Button/Toggle Function")] 
    ButtonFunction _buttonFunction;
    [SerializeField] [HideIf(EConditionOperator.Or, "GroupSettings", "_isCancelOrBackButton")] 
    ToggleGroup _toggleGroupID = ToggleGroup.None;
    [SerializeField] [HideIf(EConditionOperator.Or, "GroupSettings", "_isCancelOrBackButton")] bool _startAsSelected;

    [Header("Settings (Click Arrows To Expand)")]
    [HorizontalLine(4, color: EColor.Blue, order = 1)]
    [EnumFlags] [Label("UI Functions To Use")] [SerializeField] public Setting _enabledFunctions;
    [SerializeField] [Label("Navigation And On Click Calls")] [ShowIf("UseNavigation")] public UINavigation _navigation;
    [SerializeField] [Label("Colour Settings")] [ShowIf("NeedColour")] UIColour _colours;
    [SerializeField] [Label("Invert Colour when Highlighted or Selected")] [ShowIf("NeedInvert")] UIInvertColours _invertColourCorrection;
    [SerializeField] [Label("Swap Images, Text or SetUp Toogle Image List")] [ShowIf("NeedSwap")] UISwapper _swapImageOrText;
    [SerializeField] [Label("Size And Position Effect Settings")] [ShowIf("NeedSize")] UISizeAndPosition _sizeAndPos;
    [SerializeField] [Label("Accessories, Outline Or Shadow Settings")] [ShowIf("NeedAccessories")] UIAccessories _accessories;
    [SerializeField] [Label("Audio Settings")] [ShowIf("NeedAudio")] public UIAudio _audio;
    [SerializeField] [Label("Tooltip Settings")] [ShowIf("NeedTooltip")] UITooltip _tooltips;
    [SerializeField] [Label("Event Settings")] [ShowIf("NeedEvents")] UIEvents _events;

    //Variables
    Slider _amSlider;
    bool _isDisabled;
    RectTransform _rectForTooltip;
    UIToggles _toggleGroups;

    //Delegates
    Action<UIEventTypes, bool, Setting> StartFunctions;
    public static event Action<EscapeKey> DoCancel;

    //Properties & Enums
    public Slider AmSlider { get { return _amSlider; } }
    public ButtonFunction Function { get { return _buttonFunction; } }
    public ToggleGroup ID { get { return _toggleGroupID; } }
    public UIBranch MyBranch { get; set; }
    public bool IsSelected { get; set; }
    public bool IsCancel { get { return _isCancelOrBackButton; } }
    public UIBranch ChildBranch { get { return _navigation.Child; } }
    public bool IsDisabled
    {
        get { return _isDisabled; }
        set
        {
            HandleIfDisabled(value);
        }
    }

    //Editor Scripts
    #region Editor Scripts

    //[Button] private void DisableObject() { IsDisabled = true; }
    //[Button] private void EnableObject() { IsDisabled = false; }
    //[Button] private void NotSelected() { SetNotSelected_NoEffects(); }
    //[Button] private void AsSelected() { SetSelected_NoEffects(); }

    private bool SetChildBranch(ButtonFunction buttonFunction) 
    {
        if (buttonFunction == ButtonFunction.ToggleGroup
            || buttonFunction == ButtonFunction.Toggle_NotLinked)
        {
            _navigation.NotAToggle = true; 
        }
        else
        {
            _navigation.NotAToggle = false;
        }
        return true;
    }
    private bool UseNavigation() { return (_enabledFunctions & Setting.NavigationAndOnClick) != 0; }
    private bool NeedColour() { return (_enabledFunctions & Setting.Colours) != 0;  }
    private bool NeedSize(){ return (_enabledFunctions & Setting.SizeAndPosition) != 0; } 
    private bool NeedInvert(){ return (_enabledFunctions & Setting.InvertColourCorrection) != 0; } 
    private bool NeedSwap() { return (_enabledFunctions & Setting.SwapImageOrText) != 0; } 
    private bool NeedAccessories(){ return (_enabledFunctions & Setting.Accessories) != 0; } 
    private bool NeedAudio(){ return (_enabledFunctions & Setting.Audio) != 0; } 
    private bool NeedTooltip(){ return (_enabledFunctions & Setting.TooplTip) != 0; } 
    private bool NeedEvents(){ return (_enabledFunctions & Setting.Events) != 0; } 
    private bool GroupSettings() { return _buttonFunction != ButtonFunction.ToggleGroup; }

    #endregion

    private void Awake()
    {
        _rectForTooltip = GetComponent<RectTransform>();
        _amSlider = GetComponent<Slider>();
        MyBranch = GetComponentInParent<UIBranch>();
        _colours.OnAwake(gameObject.GetInstanceID()); //Do I need This
        _tooltips.OnAwake(_enabledFunctions, gameObject.name);
        _navigation.OnAwake(this, MyBranch);
        if (_amSlider) _amSlider.interactable = false;
        _toggleGroups = new UIToggles(this, _buttonFunction, _startAsSelected);
    }


    private void OnEnable()
    {
        StartFunctions += _accessories.OnAwake();
        StartFunctions += _sizeAndPos.OnAwake(transform);
        StartFunctions += _invertColourCorrection.OnAwake();
        StartFunctions += _swapImageOrText.OnAwake(IsSelected);
    }

    private void OnDisable()
    {
        StartFunctions -= _sizeAndPos.OnDisable();
        StartFunctions -= _swapImageOrText.OnDisable();
        StartFunctions -= _invertColourCorrection.OnDisable();
        StartFunctions -= _accessories.OnDisable();
    }

    private void Start()
    {
        _navigation.SetChildsParentBranch();
        _toggleGroups.SetUpToggleGroup(MyBranch.ThisGroupsUINodes);

        if ((_enabledFunctions & Setting.Colours) != 0 && _colours.NoSettings)
        {
            Debug.LogError("No Image or Text set on Colour settings on " + gameObject.name);
        }
    }

    // Input Systems Interfaces

    public void OnPointerEnter(PointerEventData eventData)
    {
        _navigation.PointerEnter(eventData);
       // _events.OnEnterEvent?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _navigation.PointerExit(eventData);
        //_events.OnExitEvent?.Invoke();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_buttonFunction == ButtonFunction.HoverToActivate) return;
        PressedActions();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        //_navigation.PointerUp();
        if (IsDisabled) return;
        if (IsCancel) return;

        if (AmSlider)
        {
            InvokeClickEvents();
            TurnNodeOnOff();
        }
    }

    public void OnMove(AxisEventData eventData)
    {
        _navigation.KeyBoardOrController(eventData);
    }

    public void OnSubmit(BaseEventData eventData)
    {
        if (IsDisabled) return;
        if (IsCancel) return;
        if (_buttonFunction == ButtonFunction.HoverToActivate) return;

        if (AmSlider)        //TODO Need to check this still works properly
        {
            AmSlider.interactable = IsSelected;
            if (!IsSelected)
            {
                InvokeClickEvents();
            }
        }

        PressedActions();
    }

    private void InvokeClickEvents()
    {
        _events._OnButtonClickEvent?.Invoke();
        _events._OnToggleEvent?.Invoke(IsSelected);
    }


    public void InitailNodeAsActive()
    {
        if (IsDisabled) { HandleIfDisabled(IsDisabled); return; }

        if (MyBranch.HighlightFirstOption && MyBranch.AllowKeys)
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

        if (_isCancelOrBackButton) 
        {
            if (MyBranch.MyBranchType == BranchType.PopUp)
            {
                MyBranch.IsAPopUp.RestoreLastPosition();
            }
            else
            {
                DoCancel?.Invoke(_escapeKeyFunction); 
            }
            return; 
        }

        TurnNodeOnOff();
    }

    private void HandleAudio()
    {
        if (IsSelected)
        {
            _audio.Play(UIEventTypes.Cancelled, _enabledFunctions);
        }
        else
        {
            _audio.Play(UIEventTypes.Selected, _enabledFunctions);
        }
    }



    public void TurnNodeOnOff()
    {
        _toggleGroups.ToggleGroupElements();

        if (IsSelected)
        {
            if (_buttonFunction == ButtonFunction.ToggleGroup && MyBranch.LastHighlighted == this) return;
            if (_buttonFunction == ButtonFunction.Toggle_NotLinked) { IsSelected = false; }
            Deactivate();
        }
        else
        {
            Activate();
        }
        DoPressedAction();
        _swapImageOrText.CycleToggle(IsSelected, _enabledFunctions);
    }

    public void Deactivate()
    {
        if (ChildBranch != null && (_enabledFunctions & Setting.NavigationAndOnClick) != 0)
        {
            IsSelected = false;
            _navigation.TurnOffChildren();
        }

        SetSlider(false);
    }

    private void Activate()
    {
        if (_buttonFunction != ButtonFunction.Switch_NeverHold) IsSelected = true;
        if (!AmSlider)  { InvokeClickEvents(); }
        _tooltips.HideToolTip(_enabledFunctions);
        StopAllCoroutines();
        SetSlider(true);

        if (ChildBranch && (_enabledFunctions & Setting.NavigationAndOnClick) != 0)
        {
            MoveToChildBranch();
        }
    }

    private void MoveToChildBranch()
    {
        MyBranch.SaveLastSelected(this);

        if (MyBranch.WhenToMove == WhenToMove.AtTweenEnd)
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
        _events.OnEnterEvent?.Invoke();
        _colours.SetColourOnEnter(IsSelected, _enabledFunctions);
        ActivateUIUFunctions(UIEventTypes.Highlighted);
        StartCoroutine(StartToopTip());
    }

    public void SetNotHighlighted()
    {
        StopAllCoroutines();
        _tooltips.HideToolTip(_enabledFunctions);

        if (IsDisabled) return;
        _events.OnExitEvent?.Invoke();

        if (IsSelected)
        {
            ActivateUIUFunctions(UIEventTypes.Selected);
            _colours.SetColourOnExit(IsSelected, _enabledFunctions);
        }
        else
        {
            ActivateUIUFunctions(UIEventTypes.Normal);
            _colours.ResetToNormal(_enabledFunctions);
        }
    }

    private void ActivateUIUFunctions(UIEventTypes uIEventTypes) //TODO simplify this and move to actual lines
    {
        StartFunctions.Invoke(uIEventTypes, IsSelected, _enabledFunctions);
    }

    private void DoPressedAction()
    {
        ActivateUIUFunctions(UIEventTypes.Selected);
        _sizeAndPos.WhenPressed(_enabledFunctions, IsSelected);
        _colours.ProcessPress(IsSelected, _enabledFunctions);
    }

    public void SetSelected_NoEffects()
    {
        IsSelected = true;
        SetNotHighlighted();
        _swapImageOrText.CycleToggle(IsSelected, _enabledFunctions);
    }

    public void SetNotSelected_NoEffects()
    {
        IsSelected = false;
        SetNotHighlighted();
        _swapImageOrText.CycleToggle(IsSelected, _enabledFunctions);
    }

    private void HandleIfDisabled(bool value) //TODO Review Code
    {
        Deactivate();
        ActivateUIUFunctions(UIEventTypes.Normal);
        _isDisabled = value;

        if (_isDisabled)
        {
            _colours.SetAsDisabled(_enabledFunctions);  
        }
        else
        {
            _colours.ResetToNormal(_enabledFunctions);
        }
    }

    private IEnumerator StartToopTip()
    {
        if ((_enabledFunctions & Setting.TooplTip) != 0)
        {
            yield return new WaitForSeconds(_tooltips._delay);
            _tooltips.IsActive = true;
            StartCoroutine(_tooltips.ToolTipBuild(_rectForTooltip));
            StartCoroutine(_tooltips.StartTooltip(MyBranch.AllowKeys));
        }
        yield return null;
    }

    private void SetSlider(bool selected)
    {
        if (AmSlider)
        {
            IsSelected = selected;
            _amSlider.interactable = IsSelected;
        }
    }
}


