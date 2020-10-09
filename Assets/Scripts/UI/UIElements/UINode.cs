using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;
using NaughtyAttributes;

[RequireComponent(typeof(RectTransform))]

public partial class UINode : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler,
                              IMoveHandler, IPointerUpHandler, ISubmitHandler, IPointerExitHandler, 
                              ISelectHandler, IDisabled, INode, IServiceUser
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
    [HideIf(EConditionOperator.Or, "GroupSettings", "IsCancelOrBack")] 
    private bool _startAsSelected;
    [SerializeField] 
    [Label("UI Functions To Use")] [BoxGroup("Node Functions")] 
    private Setting _enabledFunctions;
    [SerializeField] [Space(5f)]
    [Label("Navigation And On Click Calls")] [ShowIf("UseNavigation")] [BoxGroup("Node Functions")] 
    private UINavigation _navigation;
    [SerializeField] 
    [Space(5f)] [Label("Colour Settings")] [ShowIf("NeedColour")] [BoxGroup("Node Functions")] 
    private UIColour _colours;
    [SerializeField] 
    [Space(5f)] [Label("Invert Colour when Highlighted or Selected")] 
    [ShowIf("NeedInvert")] [BoxGroup("Node Functions")] 
    private UIInvertColours _invertColourCorrection;
    [SerializeField] 
    [Space(5f)] [Label("Swap Images or Text on Select or Highlight")] 
    [ShowIf("NeedSwap")] [BoxGroup("Node Functions")]
    private UIImageTextToggle _swapImageOrText;
    [SerializeField] 
    [Space(5f)] [Label("Size And Position Effect Settings")] 
    [ShowIf("NeedSize")] [BoxGroup("Node Functions")] 
    private UISizeAndPosition _sizeAndPos;
    [SerializeField] 
    [Space(5f)] [Label("Accessories, Outline Or Shadow Settings")] 
    [ShowIf("NeedAccessories")] [BoxGroup("Node Functions")] 
    private UIAccessories _accessories;
    [SerializeField] 
    [Space(5f)] [Label("Audio Settings")] [ShowIf("NeedAudio")] [BoxGroup("Node Functions")] 
    private UIAudio _audio;
    [SerializeField] 
    [Space(5f)] [Label("Tooltip Settings")] [ShowIf("NeedTooltip")] [BoxGroup("Node Functions")] 
    private UITooltip _tooltips;
    [SerializeField] 
    [Space(5f)] [Label("Event Settings")] [ShowIf("NeedEvents")] [BoxGroup("Node Functions")] 
    private UIEvents _events;

    //Variables
    private bool _isDisabled, _inMenu, _allowKeys;
    private UIToggles _toggleGroups;
    private UIDataEvents _uiDataEvents;
    private UIControlsEvents _uiControlsEvents;
    private INode _lastHighlighted;
    private UiActions _uiActions;
    private List<NodeFunctionBase> _list;
    private IHistoryTrack _uiHistoryTrack;

    //Events
    public static event Action<EscapeKey> DoCancelButtonPressed;
    public static event Action<INode> DoHighlighted, DoSelected; 
    
    //Properties & Enums
    public bool IsToggleGroup => _buttonFunction == ButtonFunction.ToggleGroup;
    private bool IsToggleNotLinked => _buttonFunction == ButtonFunction.ToggleNotLinked;
    public bool DontStoreTheseNodeTypesInHistory => IsToggleGroup || IsToggleNotLinked;
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
        else if (_lastHighlighted.ReturnNode == this && _allowKeys)
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

    private void SaveHighLighted(INode newNode)
    {
        if (_lastHighlighted is null) _lastHighlighted = newNode;
        
        if (_lastHighlighted.ReturnNode == this && newNode.ReturnNode != this)
            SetNotHighlighted();
        
        _lastHighlighted = newNode;
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
        if (_tabBranch && IsToggleGroup)
            _tabBranch.IsTabBranch();
        SetUpUiFunctions();
        SetUpIfNodeIsAToggle();
    }

    private void SetUpIfNodeIsAToggle()
    {
        if (IsToggleGroup)
            _toggleGroups = new UIToggles(node: this, _toggleGroupId, _tabBranch);

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
        _navigation.OnAwake(_uiActions, _enabledFunctions, this);
        _audio.OnAwake(_uiActions, _enabledFunctions);
    }

    private void OnEnable()
    {
        _uiDataEvents.SubscribeToInMenu(SaveInMenu);
        _uiDataEvents.SubscribeToHighlightedNode(SaveHighLighted);
        _uiControlsEvents.SubscribeToAllowKeys(SaveAllowKeys);
    }
    
    public void SubscribeToService()
    {
        _uiHistoryTrack = ServiceLocator.GetNewService<IHistoryTrack>(this);
        //return _uiHistoryTrack is null;
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
        _audio.OnDisable(_uiActions);
    }

    private void Start()
    {
        SubscribeToService();
        if (AmSlider) AmSlider.interactable = false;
        if (!IsToggleGroup) return;
        SetUpToggleGroup();
    }

    private void SetUpToggleGroup()
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
           if(!IsToggleNotLinked) MyBranch.Branch.MoveBackToThisBranch(MyBranch);
            Deactivate();
            PlayCancelAudio();
        }
        else
        {
            if (IsToggleGroup) 
                _toggleGroups.TurnOffOtherTogglesInGroup();
            Activate();
        }
        SetSlider(IsSelected);
        _uiHistoryTrack.SetSelected(this);
       // HistoryTracker.selected?.Invoke(this);
    }
    
    private void Deactivate() => SetSelectedStatus(false, DoPress);

    private void Activate() 
    {
        SetSelectedStatus(true, DoPress);
        if(!IsToggleGroup && !IsToggleNotLinked) ThisNodeIsSelected();
    }

    private void DoPress() => _uiActions._isPressed?.Invoke();
    
    public void PlayCancelAudio()
    {
        //Debug.Log(this);
        _uiActions?._canPlayCancelAudio.Invoke();
    }

    public void DeactivateNode()
    {
        if (!IsSelected || IsToggleGroup || IsToggleNotLinked) return;
        Deactivate();
        SetNotHighlighted();
        
        // if (HasChildBranch)
        // {
        //     //HasChildBranch.TurnOffChildBranches(HasChildBranch);
        // }
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
    
    public void ThisNodeIsSelected()
    {
        DoSelected?.Invoke(this);
    }

    public void ThisNodeIsHighLighted() => DoHighlighted?.Invoke(this);
}
