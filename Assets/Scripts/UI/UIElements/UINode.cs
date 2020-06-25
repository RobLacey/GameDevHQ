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
    [SerializeField] [Label("Navigation And On Click Calls")] [ShowIf("UseNavigation")] UINavigation _navigation;
    [SerializeField] [Label("Colour Settings")] [ShowIf("NeedColour")] UIColour _colours;
    [SerializeField] [Label("Invert Colour when Highlighted or Selected")] [ShowIf("NeedInvert")] UIInvertColours _invertColourCorrection;
    [SerializeField] [Label("Swap Images, Text or SetUp Toogle Image List")] [ShowIf("NeedSwap")] UISwapper _swapImageOrText;
    [SerializeField] [Label("Size And Position Effect Settings")] [ShowIf("NeedSize")] UISizeAndPosition _sizeAndPos;
    [SerializeField] [Label("Accessories, Outline Or Shadow Settings")] [ShowIf("NeedAccessories")] UIAccessories _accessories;
    [SerializeField] [Label("Audio Settings")] [ShowIf("NeedAudio")] UIAudio _audio;
    [SerializeField] [Label("Tooltip Settings")] [ShowIf("NeedTooltip")] UITooltip _tooltips;
    [SerializeField] [Label("Event Settings")] [ShowIf("NeedEvents")] UIEvents _events;

    //Variables
    Slider _amSlider;
    bool _isDisabled;
    RectTransform _rectForTooltip;
    UIToggles _toggleGroups;
    

    //Delegates
    Action<UIEventTypes, bool> StartFunctions;
    public static event Action<EscapeKey> DoCancel;

    //Properties & Enums
    public Slider AmSlider { get { return _amSlider; } }
    public ButtonFunction Function { get { return _buttonFunction; } }
    public ToggleGroup ID { get { return _toggleGroupID; } }
    public UIBranch MyBranch { get; set; }
    public bool IsSelected { get; set; }
    public bool IsCancel { get { return _isCancelOrBackButton; } }
    public UIBranch ChildBranch { get { return _navigation.Child; } }
    public IUINavigation INavigation { get; private set; }
    public IUIAudio IAudio { get; private set; } 
    public bool IsDisabled
    {
        get { return _isDisabled;  }
        set
        {
            _isDisabled = value;
            HandleIfDisabled(); 
        }
    }

    //Editor Scripts
    #region Editor Scripts

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
        _audio.OnAwake(_enabledFunctions);
        _rectForTooltip = GetComponent<RectTransform>();
        _amSlider = GetComponent<Slider>();
        MyBranch = GetComponentInParent<UIBranch>();
        _colours.OnAwake(gameObject.GetInstanceID(), _enabledFunctions); //Do I need This
        _tooltips.OnAwake(_enabledFunctions, gameObject.name);
        _events.OnAwake(_enabledFunctions);
        _navigation.OnAwake(this, MyBranch, _enabledFunctions);
        if (_amSlider) _amSlider.interactable = false;
        _toggleGroups = new UIToggles(this, _buttonFunction, _startAsSelected);
        INavigation = _navigation;
        IAudio = _audio;
    }


    private void OnEnable()
    {
        StartFunctions += _accessories.OnAwake(_enabledFunctions);
        StartFunctions += _sizeAndPos.OnAwake(transform, _enabledFunctions);
        StartFunctions += _swapImageOrText.OnAwake(IsSelected, _enabledFunctions);
        StartFunctions += _invertColourCorrection.OnAwake(_enabledFunctions);
    }

    private void OnDisable()
    {
        StartFunctions -= _accessories.OnDisable();
        StartFunctions -= _sizeAndPos.OnDisable();
        StartFunctions -= _swapImageOrText.OnDisable();
        StartFunctions -= _invertColourCorrection.OnDisable();
    }

    private void Start()
    {
        _navigation.SetChildsParentBranch();
        _toggleGroups.SetUpToggleGroup(MyBranch.ThisGroupsUINodes);

        if (_colours.CanActivate && _colours.NoSettings)
        {
            Debug.LogError("No Image or Text set on Colour settings on " + gameObject.name);
        }
    }

    // Input Systems Interfaces

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (IsDisabled) return;
        _navigation.PointerEnter(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (IsDisabled) return;
        _navigation.PointerExit(eventData);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_buttonFunction == ButtonFunction.HoverToActivate) return;
        PressedActions();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
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
       // if (MyBranch.CheckIfPopUpsNeedHandling())
       // {
            _navigation.KeyBoardOrController(eventData);
       // }
    }

    public void OnSubmit(BaseEventData eventData)
    {
        if (IsDisabled) return;
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
        if (!_events.CanActivate) return;
        _events._OnButtonClickEvent?.Invoke();
        _events._OnToggleEvent?.Invoke(IsSelected);
    }


    public void InitailNodeAsActive()
    {
        if (IsDisabled) { HandleIfDisabled(); return; }

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
            DoCancel?.Invoke(_escapeKeyFunction); 
            return; 
        }

        TurnNodeOnOff();
    }

    private void HandleAudio()
    {
        if (IsSelected)
        {
            _audio.Play(UIEventTypes.Cancelled);
        }
        else
        {
            _audio.Play(UIEventTypes.Selected);
        }
    }

    private void TurnNodeOnOff()
    {
        _toggleGroups.ToggleGroupElements();

        if (IsSelected)
        {
            if (_buttonFunction == ButtonFunction.ToggleGroup && MyBranch.LastHighlighted == this) return;
            if (_buttonFunction == ButtonFunction.Toggle_NotLinked) { IsSelected = false; }
            Deactivate();
            MyBranch.SaveLastSelected(MyBranch.MyParentBranch.LastSelected);
        }
        else
        {
            Activate();
        }
        DoPressedAction();
        _swapImageOrText.CycleToggle(IsSelected);
    }

    public void Deactivate()
    {
        if (!IsSelected) return;

        if (ChildBranch != null && _navigation.CanNaviagte)
        {
            IsSelected = false;
            _navigation.TurnOffChildren();
        }
        SetNotHighlighted();

        SetSlider(false);
    }

    private void Activate()
    {
        if (_buttonFunction != ButtonFunction.Switch_NeverHold) IsSelected = true;
        if (!AmSlider)  { InvokeClickEvents(); }
        _tooltips.HideToolTip();
        StopAllCoroutines();
        SetSlider(true);

        if (ChildBranch && _navigation.CanNaviagte)
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
        if (_events.CanActivate) _events.OnEnterEvent?.Invoke();
        _colours.SetColourOnEnter(IsSelected);
        ActivateUIUFunctions(UIEventTypes.Highlighted);
        StartCoroutine(StartToopTip());
    }

    public void SetNotHighlighted()
    {
        StopAllCoroutines();
        _tooltips.HideToolTip();

        if (IsDisabled) return;
        if (_events.CanActivate) _events.OnExitEvent?.Invoke();

        if (IsSelected)
        {
            ActivateUIUFunctions(UIEventTypes.Selected);
            _colours.SetColourOnExit(IsSelected);
        }
        else
        {
            ActivateUIUFunctions(UIEventTypes.Normal);
            _colours.ResetToNormal();
        }
    }

    private void ActivateUIUFunctions(UIEventTypes uIEventTypes) //TODO simplify this and move to actual lines
    {
        StartFunctions.Invoke(uIEventTypes, IsSelected);
    }

    private void DoPressedAction()
    {
        ActivateUIUFunctions(UIEventTypes.Selected);
        _sizeAndPos.WhenPressed(IsSelected);
        _colours.ProcessPress(IsSelected);
    }

    public void SetSelected_NoEffects()
    {
        IsSelected = true;
        SetNotHighlighted();
        _swapImageOrText.CycleToggle(IsSelected);
    }

    public void SetNotSelected_NoEffects()
    {
        IsSelected = false;
        SetNotHighlighted();
        _swapImageOrText.CycleToggle(IsSelected);
    }

    private void HandleIfDisabled()
    {
        Deactivate();
        ActivateUIUFunctions(UIEventTypes.Normal);

        if (_isDisabled)
        {
            _colours.SetAsDisabled();  
        }
        else
        {
            _colours.ResetToNormal();
        }
    }

    private IEnumerator StartToopTip()
    {
        if (_tooltips.CanActivate)
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

    public void DisableObject() { IsDisabled = true; }
    public void EnableObject() { IsDisabled = false; }

}


