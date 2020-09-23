﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using NaughtyAttributes;

[RequireComponent(typeof(RectTransform))]

public partial class UINode : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler,
                              IMoveHandler, IPointerUpHandler, ISubmitHandler, IPointerExitHandler, 
                              ISelectHandler, IDisabled, INode
{
    [Header("Main Settings")]
    [HorizontalLine(4, color: EColor.Blue, order = 1)]
    [SerializeField] 
    [ValidateInput("SetChildBranch")] [Label("Button/Toggle Function")] private ButtonFunction _buttonFunction;
    [SerializeField] 
    [ShowIf("IsCancelOrBack")] private EscapeKey _escapeKeyFunction = EscapeKey.GlobalSetting;
    [SerializeField] 
    [HideIf(EConditionOperator.Or, "GroupSettings", "IsCancelOrBack")] 
    private ToggleGroup _toggleGroupId = ToggleGroup.None;
    [SerializeField]
    [HideIf(EConditionOperator.Or, "GroupSettings", "IsCancelOrBack")] private Canvas _tab;
    [SerializeField] 
    [HideIf(EConditionOperator.Or, "GroupSettings", "IsCancelOrBack")] private bool _startAsSelected;

    //[Header("Settings (Click Arrows To Expand)")]
    [HorizontalLine(4, color: EColor.Blue, order = 1)]
    [SerializeField] 
    [Label("UI Functions To Use")] public Setting _enabledFunctions;
    [SerializeField] 
    [Label("Navigation And On Click Calls")] [ShowIf("UseNavigation")] private UINavigation _navigation;
    [SerializeField] 
    [Label("Colour Settings")] [ShowIf("NeedColour")] private UIColour _colours;
    [SerializeField] 
    [Label("Invert Colour when Highlighted or Selected")] [ShowIf("NeedInvert")] 
    private UIInvertColours _invertColourCorrection;
    [SerializeField] 
    [Label("Swap Images or Text on Select or Highlight")] [ShowIf("NeedSwap")] 
    private UIImageTextToggle _swapImageOrText;
    [SerializeField] 
    [Label("Size And Position Effect Settings")] [ShowIf("NeedSize")] private UISizeAndPosition _sizeAndPos;
    [SerializeField] 
    [Label("Accessories, Outline Or Shadow Settings")] [ShowIf("NeedAccessories")] private UIAccessories _accessories;
    [SerializeField] 
    [Label("Audio Settings")] [ShowIf("NeedAudio")] private UIAudio _audio;
    [SerializeField] 
    [Label("Tooltip Settings")] [ShowIf("NeedTooltip")] private UITooltip _tooltips;
    [SerializeField] 
    [Label("Event Settings")] [ShowIf("NeedEvents")] private UIEvents _events;

    //Variables
    private bool _isDisabled, _inMenu, _allowKeys, _holdStateOnPause;
    private UIToggles _toggleGroups;
    private UIDataEvents _uiDataEvents;
    private UIControlsEvents _uiControlsEvents;
    private INode _lastHighlighted;
    private UiActions _uiActions;

    //Events
    public static event Action<EscapeKey> DoCancelButtonPressed;
    public static event Action<INode> DoHighlighted, DoSelected; 
    
    //Properties & Enums
    public bool IsToggleGroup => _buttonFunction == ButtonFunction.ToggleGroup;
    private bool IsToggleNotLinked => _buttonFunction == ButtonFunction.ToggleNotLinked;
    private bool IsCancelOrBack => _buttonFunction == ButtonFunction.CancelOrBack;
    private bool IsSelected { get; set; }
    private Slider AmSlider { get; set; }
    public ToggleGroup ToggleGroupId => _toggleGroupId;
    public UIBranch MyBranch { get; private set; }
    public UIBranch HasChildBranch => _navigation.Child;
    public UINode ReturnNode => this;

    private void SaveInMenu(bool isInMenu)
    {
        _inMenu = isInMenu;
        if (!_inMenu)
        {
            SetNotHighlighted();
        }
        else if (_lastHighlighted.ReturnNode == this)
        {
            SetAsHighlighted();
        }
    }
    
    private void SaveAllowKeys(bool allow)
    {
        _allowKeys = allow;
        if(!_allowKeys && _lastHighlighted.ReturnNode == this)
            SetNotHighlighted();
    }

    private void SaveGameIsPaused(bool isPaused) => _holdStateOnPause = MyBranch.CanvasIsEnabled && isPaused;

    private void SaveHighLighted(INode newNode)
    {
        if (_lastHighlighted is null) _lastHighlighted = newNode;
        if (_lastHighlighted.ReturnNode == this && newNode.ReturnNode != this)
            SetNotHighlighted();
        _lastHighlighted = newNode;
    }

    private void SaveLastSelected(INode newNode)
    {
        if (DeselectNodeNotAllowed()) return;
        if (CheckNodeIsNotBranchesChild(newNode.ReturnNode))
        {
            DeactivateAndCancelChildren();
        }        
        bool DeselectNodeNotAllowed()
            => newNode.ReturnNode == this || !IsSelected || IsToggleGroup || IsToggleNotLinked || _holdStateOnPause;
    }

    private bool CheckNodeIsNotBranchesChild(UINode newNodeToTest)
    {
        if (MyBranch == newNodeToTest.MyBranch) return true;
        var lastNodeSelected = newNodeToTest.MyBranch.MyParentBranch.LastSelected;
        return lastNodeSelected.ReturnNode != this;
    }

    public bool IsDisabled
    {
        get => _isDisabled;
        private set
        {
            _isDisabled = value;
            if (_isDisabled)
                Deactivate();
            _uiActions._isDisabled?.Invoke(_isDisabled);
        }
    }
    
    // ReSharper disable once UnusedMember.Global - Assigned in editor to Disable Object
    public void DisableObject() => IsDisabled = true; 
    // ReSharper disable once UnusedMember.Global - Assigned in editor to Enable Object
    public void EnableObject() => IsDisabled = false; 

    //Main
    private void Awake()
    {
        _uiActions = new UiActions(gameObject.GetInstanceID());
        AmSlider = GetComponent<Slider>();
        MyBranch = GetComponentInParent<UIBranch>();
        _uiDataEvents = new UIDataEvents();
        _uiControlsEvents = new UIControlsEvents();
        SetUpUiFunctions();
        SetUpIfNodeIsAToggle();
    }

    private void SetUpIfNodeIsAToggle()
    {
        if (IsToggleGroup)
            _toggleGroups = new UIToggles(this, _toggleGroupId, _tab);

        if (IsToggleNotLinked || IsToggleGroup)
            _navigation.Child = null;
    }

    private void SetUpUiFunctions()
    {
        var rectTransform = GetComponent<RectTransform>();
        _colours.OnAwake(_uiActions, _enabledFunctions);
        _events.OnAwake(_uiActions, _enabledFunctions);
        _accessories.OnAwake(_uiActions, _enabledFunctions);
        _invertColourCorrection.OnAwake(_uiActions, _enabledFunctions);
        _swapImageOrText.OnAwake(_uiActions, _enabledFunctions);
        _sizeAndPos.OnAwake(_uiActions, _enabledFunctions, rectTransform);
        _tooltips.OnAwake(_uiActions, _enabledFunctions, rectTransform);
        _navigation.OnAwake(_uiActions, _enabledFunctions, MyBranch);
        
        _audio.OnAwake(_enabledFunctions);
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
        _events.OnDisable(_uiActions);
        _accessories.OnDisable(_uiActions);
        _invertColourCorrection.OnDisable(_uiActions);
        _swapImageOrText.OnDisable(_uiActions);
        _sizeAndPos.OnDisable(_uiActions);
        _tooltips.OnDisable(_uiActions);
        _navigation.OnDisable(_uiActions);
    }

    private void Start()
    {
        if (AmSlider) AmSlider.interactable = false;
        if (!IsToggleGroup) return;
        SeTUpToggleGroup();
    }

    private void SeTUpToggleGroup()
    {
        _toggleGroups.SetUpToggleGroup(MyBranch.ThisGroupsUiNodes);
        if (!_startAsSelected) return;
        _toggleGroups.TurnOffOtherTogglesInGroup();
        SetNodeAsSelected_NoEffects();
    }

    public void SetNodeAsActive()
    {
        if (NodeIsDisabled()) return;
        _lastHighlighted = this;
        
        if (_allowKeys && _inMenu)
        {
            SetAsHighlighted();
        }
        else
        {
            ThisNodeIsHighLighted();
        }
    }

    private bool NodeIsDisabled()
    {
        if (!_isDisabled) return false;
        _navigation.MoveToNextFreeNode();
        return true;
    }

    private void PressedActions()
    {
        if (IsDisabled) return;
        if (CancelOrBackButton()) return;
        TurnNodeOnOff();
    }

    private bool CancelOrBackButton()
    {
        if (!IsCancelOrBack) return false;
        DoCancelButtonPressed?.Invoke(_escapeKeyFunction);
        return true;
    }

    private void TurnNodeOnOff()
    {
        if (IsSelected)
        {
           if (IsToggleGroup) return;
            MyBranch.MoveToBranchWithoutTween();
            Deactivate();
        }
        else
        {
            if (IsToggleGroup) 
                _toggleGroups.TurnOffOtherTogglesInGroup();
            Activate();
        }
        SetSlider(IsSelected);
    }
    
    private void Deactivate() => SetSelectedStatus(false, DoPress);

    private void Activate() 
    {
        SetSelectedStatus(true, DoPress);
        if(!IsToggleGroup && !IsToggleNotLinked) ThisNodeIsSelected();
    }

    private void DoPress() => _uiActions._isPressed?.Invoke();

    public void DeactivateAndCancelChildren() 
    {
        if(IsSelected && !IsToggleGroup && !IsToggleNotLinked) 
            Deactivate();
        SetNotHighlighted();
    }
    
    private void SetAsHighlighted() 
    {
        if (IsDisabled) return;
        _uiActions._whenPointerOver?.Invoke(true);
        ThisNodeIsHighLighted();
    }

    private void SetNotHighlighted() => _uiActions._whenPointerOver?.Invoke(false);

    public void SetNodeAsSelected_NoEffects() => SetSelectedStatus(true, SetNotHighlighted);

    public void SetNodeAsNotSelected_NoEffects() => SetSelectedStatus(false, SetNotHighlighted);

    private void SetSelectedStatus(bool isSelected, Action endAction)
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
    
    public void ThisNodeIsSelected() => DoSelected?.Invoke(this); 

    public void ThisNodeIsHighLighted() => DoHighlighted?.Invoke(this); 
}
