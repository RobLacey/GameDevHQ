using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using NaughtyAttributes;

public class UINode : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler,
                                     IMoveHandler, IPointerUpHandler, ISubmitHandler, IPointerExitHandler
{
    [Header("Main Settings")]
    [HorizontalLine(4, color: EColor.Blue, order = 1)]
    [SerializeField] [Label("Is Cancel or Back Button")] bool _isCancelOrBackButton;
    [SerializeField] [ShowIf("_isCancelOrBackButton")] EscapeKey _escapeKeyFunction = EscapeKey.GlobalSetting;
    [SerializeField] [HideIf("_isCancelOrBackButton")] bool _highlightFirstOption = true;
    [SerializeField] [ReadOnly] bool _isDisabled;
    [SerializeField] [HideIf("_isCancelOrBackButton")] [Label("Preserve When Selected")] PreserveSelection _preseveSelection;
    [SerializeField] [HideIf(EConditionOperator.Or, "GroupSettings", "_isCancelOrBackButton")] ToggleGroup _toggleGroupID = ToggleGroup.None;
    [SerializeField] [HideIf(EConditionOperator.Or, "GroupSettings", "_isCancelOrBackButton")] bool _startAsSelected;
    [Header("Settings (Click Arrows To Expand)")]
    [HorizontalLine(4, color: EColor.Blue, order = 1)]
    [EnumFlags] [Label("UI Functions To Use")] [SerializeField] public Setting _functionToUse;
    [SerializeField] [Label("Navigation And On Click Calls")] [ShowIf("UseNavigation")] UINavigation _navigation;
    [SerializeField] [Label("Colour Settings")] [ShowIf("NeedColour")] UIColour _colours;
    [SerializeField] [Label("Invert Colour when Highlighted or Selected")] [ShowIf("NeedInvert")] UIInvertColours _invertColourCorrection;
    [SerializeField] [Label("Swap Images, Text or SetUp Toogle Image List")] [ShowIf("NeedSwap")] UISwapper _swapImageOrText;
    [SerializeField] [Label("Size And Position Effect Settings")] [ShowIf("NeedSize")] UISizeAndPosition _sizeAndPos;
    [SerializeField] [Label("Accessories, Outline Or Shadow Settings")] [ShowIf("NeedAccessories")] UIAccessories _accessories;
    [SerializeField] [Label("Audio Settings")] [ShowIf("NeedAudio")] public UIAudio _audio;
    [SerializeField] [Label("Tooltip Settings")] [ShowIf("NeedTooltip")] UITooltip _tooltips;

    //Variables
    //UIBranch _myBranchController;
    Slider _amSlider;
    UIEventTypes _eventType = UIEventTypes.Normal;
    UINode[] _toggleGroupMembers;
    Vector3[] _myCorners = new Vector3[4];


    //Delegates
    Action<UIEventTypes, bool, Setting> SetUp;
    public static event Action<EscapeKey> Canceller;

    //Properties & Enums
    public UIBranch MyBranchController { get; set; }
    public EscapeKey EscapeKeyFunction { get { return _escapeKeyFunction; } }
    public UIBranch MyParentController { get; set; }
    public bool Selected { get; set; }
    public bool Disabled
    {
        get { return _isDisabled; }
        set
        {
            HandleIfDisabled(value);
        }
    }

    //Editor Scripts
    #region Editor Scripts

    [Button] private void DisableObject() { Disabled = true; }
    [Button] private void EnableObject() { Disabled = false; }

    public bool UseNavigation()  {  return (_functionToUse & Setting.NavigationAndOnClick) != 0; }
    public bool NeedColour() { return (_functionToUse & Setting.Colours) != 0;  }
    public bool NeedSize(){ return (_functionToUse & Setting.SizeAndPosition) != 0; } 
    public bool NeedInvert(){ return (_functionToUse & Setting.InvertColourCorrection) != 0; } 
    public bool NeedSwap() { return (_functionToUse & Setting.SwapImageOrText) != 0; } 
    public bool NeedAccessories(){ return (_functionToUse & Setting.Accessories) != 0; } 
    public bool NeedAudio(){ return (_functionToUse & Setting.Audio) != 0; } 
    public bool NeedTooltip(){ return (_functionToUse & Setting.TooplTip) != 0; } 

    public bool GroupSettings()
    {
        if (_preseveSelection == PreserveSelection.ToggleGroup_OneAlwaysOn || _preseveSelection == PreserveSelection.ToggleGroup_AllOff)
        {
            return false;
        }
        return true;
    }

    #endregion

    private void Awake()
    {
        GetComponent<RectTransform>().GetWorldCorners(_myCorners);
        _amSlider = GetComponent<Slider>();
        MyBranchController = GetComponentInParent<UIBranch>();
        _colours.OnAwake(gameObject.GetInstanceID());
        _audio.OnAwake(GetComponentInParent<AudioSource>());
        _tooltips.OnAwake(_functionToUse, _myCorners, gameObject.name);
        if (_amSlider) _amSlider.interactable = false;
    }

    private void Start()
    {
        SetUpToggleGroup();
        if ((_functionToUse & Setting.Colours) != 0 && _colours.NoSettings)
        {
            Debug.LogError("No Image or Text set on Colour settings on " + gameObject.name);
        }
    }

    private void OnEnable()
    {
        SetUp += _accessories.OnAwake();
        SetUp += _sizeAndPos.OnAwake(transform);
        SetUp += _invertColourCorrection.OnAwake();
        SetUp += _swapImageOrText.OnAwake(Selected);
    }

    private void OnDisable()
    {
        SetUp -= _sizeAndPos.OnDisable();
        SetUp -= _swapImageOrText.OnDisable();
        SetUp -= _invertColourCorrection.OnDisable();
        SetUp -= _accessories.OnDisable();
    }

    private void SetUpToggleGroup()
    {
        foreach (var leaf in MyParentController.ThisGroupsUILeafs)
        {
            if (leaf._preseveSelection == PreserveSelection.ToggleGroup_AllOff
                || leaf._preseveSelection == PreserveSelection.ToggleGroup_OneAlwaysOn)
            {
                List<UINode> temp = new List<UINode>();
                foreach (var item in MyParentController.ThisGroupsUILeafs)
                {
                    if (item != leaf && item._toggleGroupID == leaf._toggleGroupID)
                    {
                        temp.Add(item);
                    }
                }
                if (_startAsSelected)
                {
                    SetSelected_NoEffects();
                }
                leaf._toggleGroupMembers = temp.ToArray();
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Disabled) return;
        if (eventData.pointerDrag) return;                  //Enables drag on slider to have pressed colour
        _audio.Play(UIEventTypes.Highlighted, _functionToUse);
        SetAsHighlighted();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (Disabled) return;
        if (eventData.pointerDrag) return;                      //Enables drag on slider to have pressed colour
        SetNotHighlighted();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (Disabled) return;
        if (_isCancelOrBackButton)
        {
            MyBranchController.SaveLastSelected(MyBranchController.LastSelected);
            Canceller?.Invoke(_escapeKeyFunction);
            return;
        }
        SwitchUIDisplay();
        if (!_amSlider)
        {
            _navigation._asButtonEvent?.Invoke();
            _navigation._asToggleEvent?.Invoke(Selected);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (Disabled) return;
        if (_amSlider) 
        {
            _navigation._asButtonEvent?.Invoke();
            _navigation._asToggleEvent?.Invoke(Selected);
            SwitchUIDisplay(); 
        }
    }

    public void MoveToNext() //KB/Ctrl highlight
    {
        if (!MyBranchController.AllowKeys) return;
        MyBranchController.LastSelected.SetNotHighlighted();
        MyBranchController.SaveLastSelected(this);
        _audio.Play(UIEventTypes.Highlighted, _functionToUse);
        SetAsHighlighted();
    }

    public void OnMove(AxisEventData eventData)
    {
        if (_amSlider)
        {
            if (Selected)
            {
                if (eventData.moveDir == MoveDirection.Left || eventData.moveDir == MoveDirection.Right)
                {
                    _audio.Play(UIEventTypes.Selected, _functionToUse);
                }
            }
            else
            {
                _navigation.ProcessMoves(eventData, _functionToUse);
            }
        }
        else
        {
            _navigation.ProcessMoves(eventData, _functionToUse);
        }
    }

    public void OnSubmit(BaseEventData eventData) //KB/Ctrl
    {
        if (Disabled) return;

        if (_isCancelOrBackButton)
        {
            MyBranchController.SaveLastSelected(MyBranchController.LastSelected);
            Canceller?.Invoke(_escapeKeyFunction);
            return;
        }
        SwitchUIDisplay();

        if (_amSlider)        //TODO Needs to be test witha slider
        { 
            _amSlider.interactable = Selected;
            if (!Selected)
            {
                _navigation._asButtonEvent?.Invoke();
                _navigation._asToggleEvent?.Invoke(Selected);
            }
        }
        else
        {
            _navigation._asButtonEvent?.Invoke();
            _navigation._asButtonEvent?.Invoke();
        }
    }

    public void InitialiseStartUp()
    {
        if (Disabled) { HandleIfDisabled(Disabled); return; }

        MyBranchController.LastSelected = this;

        if (_preseveSelection != PreserveSelection.Toggle_NotLinked
            && _preseveSelection != PreserveSelection.ToggleGroup_OneAlwaysOn)
        {
            Selected = false;
            if (_highlightFirstOption && MyBranchController.AllowKeys)
            {
                SetAsHighlighted();
            }
            else
            {
                _colours.ResetToNormal(_functionToUse);
            }
        }
        else
        {
            if (_highlightFirstOption && MyBranchController.AllowKeys)
            {
                SetAsHighlighted();
            }
        }
    }

    public void SetAsHighlighted()
    {
        if (Disabled) return;
        _colours.SetColourOnEnter(Selected, _functionToUse);
        SetButton(UIEventTypes.Highlighted);
        StartCoroutine(StartToopTip());
    }

    private void SetButton(UIEventTypes uIEventTypes)
    {
        _eventType = uIEventTypes;
        SetUp.Invoke(_eventType, Selected, _functionToUse);
    }

    private void SwitchUIDisplay()
    {
        TurnOffBranchLastSelected();
        MyBranchController.SaveLastSelected(this);

        if (Selected)
        {
            if (_preseveSelection == PreserveSelection.ToggleGroup_OneAlwaysOn)
            {
                if (MyBranchController.LastSelected == this) return;
            }
            DisableLevel();
            SetPressed();
            _audio.Play(UIEventTypes.Cancelled, _functionToUse);

        }
        else
        {
            ActivateLevel();
            SetPressed();
            _audio.Play(UIEventTypes.Selected, _functionToUse);
        }
    }

    private void TurnOffBranchLastSelected() // Review may need to check mastercontrollers last selected
    {
        UINode lastElementSelected = MyBranchController.LastSelected;
        if (_preseveSelection == PreserveSelection.ToggleGroup_OneAlwaysOn)
        {
            foreach (var item in _toggleGroupMembers)
            {
                item.DisableLevel();
                item.SetNotHighlighted();
            }
        }
        if (lastElementSelected != this) // Review it's use
        {
            if (lastElementSelected._preseveSelection != PreserveSelection.Toggle_NotLinked
                && lastElementSelected._preseveSelection != PreserveSelection.ToggleGroup_OneAlwaysOn)
            {
                lastElementSelected.DisableLevel();
                lastElementSelected.SetNotHighlighted();
            }
        }
    }

    public void DisableLevel()
    {
        Selected = false;
        if (_amSlider) { QuitSlider(); }
        _swapImageOrText.CycleToggle(Selected, _functionToUse);

        TurnOffChildren();
    }


    private void ActivateLevel()
    {
        if(_preseveSelection != PreserveSelection.Never_TempSwitch) Selected = true;
        if (_amSlider) { _amSlider.interactable = Selected; }
        _swapImageOrText.CycleToggle(Selected, _functionToUse);
        _tooltips.HideToolTip(_functionToUse);
        StopAllCoroutines();
        TurnOnChildren();
    }

    private void TurnOffChildren()
    {
        if (_navigation._childBranch && (_functionToUse & Setting.NavigationAndOnClick) != 0)
        {
            _navigation._childBranch.StartOutTweens();

            if (_navigation._childBranch.LastSelected != null)
            {
                if (_navigation._childBranch.LastSelected._preseveSelection != PreserveSelection.Toggle_NotLinked)
                {
                    _navigation._childBranch.LastSelected.SetNotHighlighted();
                    _navigation._childBranch.LastSelected.DisableLevel();
                }
            }
        }
    }

    private void TurnOnChildren()
    {
        if (_navigation._childBranch && (_functionToUse & Setting.NavigationAndOnClick) != 0)
        {
            MyBranchController.TurnOffOnMoveToChild(_navigation._moveType);
            _navigation._childBranch.MoveToNextLevel(MyBranchController);
        }
    }

    private void SetPressed()
    {
        SetButton(UIEventTypes.Selected);
        StartCoroutine(_sizeAndPos.PressedSequence(_functionToUse));
        _colours.ProcessPress(Selected, _functionToUse);
    }

    public void OnCancel()
    {
        if (_isCancelOrBackButton)
        {
            Selected = false;
            SetNotHighlighted();
        }

        if (MyParentController.LastSelected == this) { return; }                    //Stops returning past the Home Level menus

        if (_preseveSelection != PreserveSelection.Toggle_NotLinked && _preseveSelection != PreserveSelection.ToggleGroup_OneAlwaysOn)
        {
            MyParentController.LastSelected.SetButton(UIEventTypes.Normal);
            MyParentController.LastSelected.DisableLevel();
        }
        else
        {
            bool temp = Selected;
            MyParentController.LastSelected.DisableLevel();
            Selected = temp;
        }
        _audio.Play(UIEventTypes.Cancelled, _functionToUse);
        MyParentController.MoveBackALevel();
    }

    public void SetSelected_NoEffects()
    {
        Selected = true;
        SetNotHighlighted();
    }

    public void SetNotHighlighted()
    {
        StopAllCoroutines();
        _tooltips.HideToolTip(_functionToUse);

        if (Selected)
        {
            SetButton(UIEventTypes.Selected);
            _colours.SetColourOnExit(Selected, _functionToUse);
        }
        else
        {
            SetButton(UIEventTypes.Normal);
            _colours.ResetToNormal(_functionToUse);
        }
    }

    private void HandleIfDisabled(bool value) //TODO Review Code
    {
        MoveCursorToNextFree();
        DisableLevel();
        SetButton(UIEventTypes.Normal);
        _isDisabled = value;

        if (_isDisabled)
        {
            _colours.SetAsDisabled(_functionToUse);  //******Working except when already on something that's disabling. Need to set event data etc
        }
        else
        {
            _colours.ResetToNormal(_functionToUse);
        }
    }

    private void MoveCursorToNextFree()
    {
        if (MyBranchController.LastSelected == this)
        {
            if (_navigation._setNavigation != NavigationType.None)
            {
                if (_navigation.down) { _navigation.down.MoveToNext(); }
                else if (_navigation.up) { _navigation.up.MoveToNext(); }
                else if (_navigation.right) { _navigation.right.MoveToNext(); }
                else if (_navigation.left) { _navigation.left.MoveToNext(); }
            }
            else
            {
                OnCancel();
                Debug.Log("No where to go except down");
            }
        }
    }

    private void QuitSlider()
    {
        _navigation._asButtonEvent?.Invoke();
        _amSlider.interactable = Selected;
    }

    private IEnumerator StartToopTip()
    {
        if ((_functionToUse & Setting.TooplTip) != 0)
        {
            yield return new WaitForSeconds(_tooltips._delay);
            _tooltips.IsActive = true;
            StartCoroutine(_tooltips.ToolTipBuild());
            StartCoroutine(_tooltips.StartTooltip(MyBranchController.AllowKeys));
        }
        yield return null;
    }
}


