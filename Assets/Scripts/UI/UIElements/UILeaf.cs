using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using NaughtyAttributes;

[RequireComponent(typeof(ColourLerp))]
public class UILeaf : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler,
                                     IMoveHandler, IPointerUpHandler, ISubmitHandler, IPointerExitHandler
{
    [Header("Main Settings")]
    [HorizontalLine(4, color: EColor.Blue, order = 1)]
    [SerializeField] [Label("Is Cancel or Back Button")] bool _isCancelOrBackButton;
    [SerializeField] [ShowIf("_isCancelOrBackButton")] EscapeKey _escapeKeyFunction = EscapeKey.GlobalSetting;
    [SerializeField] [HideIf("_isCancelOrBackButton")] bool _highlightFirstOption = true;
    [SerializeField] [HideIf("_isCancelOrBackButton")] [ReadOnly] bool _isDisabled;
    [SerializeField] [HideIf("_isCancelOrBackButton")] [Label("Preserve When Selected")] PreserveSelection _preseveSelection;
    [SerializeField] [HideIf(EConditionOperator.Or, "GroupSettings", "_isCancelOrBackButton")] ToggleGroup _toggleGroupID = ToggleGroup.None;
    [Header("Settings (Click Arrows To Expand)")]
    [HorizontalLine(4, color: EColor.Blue, order = 1)]
    [EnumFlags] [SerializeField] public Setting setting;
    [SerializeField] [Label("Navigation And On Click Calls")] [ShowIf("NeedNav")] UINavigation _navigation;
    [SerializeField] [Label("Colour Settings")] [ShowIf("NeedColour")] UIColour _colours;
    [SerializeField] [Label("Invert Colour when Highlighted or Selected")] [ShowIf("NeedInvert")] UIInvertColours _invertColourCorrection;
    [SerializeField] [Label("Swap Images, Text or SetUp Toogle Image List")] [ShowIf("NeedSwap")] UISwapper _swapImageOrText;
    [SerializeField] [Label("Change Size Settings")] [ShowIf("NeedSize")] UISize _buttonSize;
    [SerializeField] [Label("Accessories, Outline Or Shadow Settings")] [ShowIf("NeedAccessories")] UIAccessories _accessories;
    [SerializeField] [Label("Audio Settings")] [ShowIf("NeedAudio")] public UIAudio _audio;

    //Variables
    UIBranch _myBranchController;
    Slider _amSlider;
    UIEventTypes _settings = UIEventTypes.Normal;
    UILeaf[] _toggleGroupMembers;

    //Delegates
    Action<UIEventTypes, bool, Setting> SetUp;
    public static event Action<EscapeKey> Canceller;

    //Properties & Enums
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

    public bool NeedNav { get { return (setting & Setting.Navigation) != 0; } }
    public bool NeedColour { get { return (setting & Setting.Colours) != 0; } }
    public bool NeedSize { get { return (setting & Setting.Size) != 0; } }
    public bool NeedInvert { get { return (setting & Setting.Invert) != 0; } }
    public bool NeedSwap { get { return (setting & Setting.Swap) != 0; } }
    public bool NeedAccessories {get{  return (setting & Setting.Accessories ) != 0; } }
    public bool NeedAudio { get { return (setting & Setting.Audio) != 0; } }

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
        _amSlider = GetComponent<Slider>();
        _colours.MyColourLerper = GetComponent<ColourLerp>();
        _myBranchController = GetComponentInParent<UIBranch>();
        _colours.OnAwake();
        _audio.OnAwake(GetComponentInParent<AudioSource>());
        if (_amSlider) _amSlider.interactable = false;
    }

    private void Start()
    {
        SetUpToggleGroup();
    }

    private void OnEnable()
    {
        SetUp += _accessories.OnAwake();
        SetUp += _buttonSize.OnAwake(transform);
        SetUp += _invertColourCorrection.OnAwake();
        SetUp += _swapImageOrText.OnAwake(Selected);
    }

    private void OnDisable()
    {
        SetUp -= _buttonSize.OnDisable();
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
                List<UILeaf> temp = new List<UILeaf>();
                foreach (var item in MyParentController.ThisGroupsUILeafs)
                {
                    if (item != leaf && item._toggleGroupID == leaf._toggleGroupID)
                    {
                        temp.Add(item);
                    }
                }
                leaf._toggleGroupMembers = temp.ToArray();
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData) 
    {
        if (Disabled) return;
        if (eventData.pointerDrag) return;                  //Enables drag on slider to have pressed colour
        _audio.Play(UIEventTypes.Highlighted, setting);
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
            _myBranchController.SaveLastSelected(_myBranchController.LastSelected);
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
        if (_amSlider) { SwitchUIDisplay(); }
    }

    public void MoveToNext() //KB/Ctrl highlight
    {
        if (!_myBranchController.AllowKeys) return;
        _audio.Play(UIEventTypes.Highlighted, setting);
        _myBranchController.LastSelected.SetNotHighlighted();
        _myBranchController.SaveLastSelected(this);
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
                    _audio.Play(UIEventTypes.Selected, setting);
                }
            }
            else
            {
                _navigation.ProcessMoves(eventData, setting);
            }
        }
        else
        {
            _navigation.ProcessMoves(eventData, setting);
        }
    }

    public void OnSubmit(BaseEventData eventData) //KB/Ctrl
    {
        if (Disabled) return;

        if (_isCancelOrBackButton)
        {
            _myBranchController.SaveLastSelected(_myBranchController.LastSelected);
            Canceller?.Invoke(_escapeKeyFunction);
            return;
        }
        SwitchUIDisplay();
        _navigation._asButtonEvent?.Invoke();

        if (_amSlider) { _amSlider.interactable = Selected;  }
    }

    public void InitialiseStartUp()
    {
        if (Disabled) { HandleIfDisabled(Disabled); return; }

        _myBranchController.LastSelected = this;

        if (_preseveSelection != PreserveSelection.Toggle_NotLinked
            && _preseveSelection != PreserveSelection.ToggleGroup_OneAlwaysOn)
        {
            Selected = false;
            if (_highlightFirstOption && _myBranchController.AllowKeys)
            {
                SetAsHighlighted();
            }
            else
            {
                _colours.ResetToNormal(setting);
            }
        }
        else
        {
            if (_highlightFirstOption && _myBranchController.AllowKeys)
            {
                SetAsHighlighted();
            }
        }
    }

    public void SetAsHighlighted()
    {
        if (Disabled) return;
        _colours.SetColourOnEnter(Selected, setting);
        SetButton(UIEventTypes.Highlighted);
    }

    private void SetButton(UIEventTypes uIEventTypes)
    {
        _settings = uIEventTypes;
        SetUp.Invoke(_settings, Selected, setting);
    }

    private void SwitchUIDisplay()
    {
        TurnOffLastSelected();
        _myBranchController.SaveLastSelected(this);

        if (Selected)
        {
            if (_preseveSelection == PreserveSelection.ToggleGroup_OneAlwaysOn)
            {
                if (_myBranchController.LastSelected == this) return;
            }
            DisableChildLevel();
            SetPressed();
            _audio.Play(UIEventTypes.Cancelled, setting);

        }
        else
        {
            ActivateChildLevel();
            SetPressed();
            _audio.Play(UIEventTypes.Selected, setting);
        }
    }

    private void TurnOffLastSelected()
    {
        UILeaf lastElementSelected = _myBranchController.LastSelected;
        if (_preseveSelection == PreserveSelection.ToggleGroup_OneAlwaysOn)
        {
            foreach (var item in _toggleGroupMembers)
            {
                item.DisableChildLevel();
                item.SetNotHighlighted();
            }
        }
        if (lastElementSelected != this)
        {
            if (lastElementSelected._preseveSelection != PreserveSelection.Toggle_NotLinked
                && lastElementSelected._preseveSelection != PreserveSelection.ToggleGroup_OneAlwaysOn)
            {
                lastElementSelected.DisableChildLevel();
                lastElementSelected.SetNotHighlighted();
            }
        }
    }

    private void DisableChildLevel()
    {
        Selected = false;
        if (_amSlider) { QuitSlider(); }
        _swapImageOrText.CycleToggleList(Selected, setting);

        if (_navigation._childBranch && ((setting & Setting.Navigation) != 0) == true)
        {
            _navigation._childBranch.TurnOffBranch();

            if (_navigation._childBranch.LastSelected != null)
            {
                if (_navigation._childBranch.LastSelected._preseveSelection != PreserveSelection.Toggle_NotLinked)
                {
                    _navigation._childBranch.LastSelected.SetNotHighlighted();
                    _navigation._childBranch.LastSelected.DisableChildLevel();
                }
            }
        }
    }

    private void ActivateChildLevel()
    {
        Selected = true;
        if(_amSlider) { _amSlider.interactable = Selected; }
        _swapImageOrText.CycleToggleList(Selected, setting);

        if (_navigation._childBranch && ((setting & Setting.Navigation) != 0) == true)
        {
            _myBranchController.TurnOffOnMoveToChild();
            _navigation._childBranch.MoveToNextLevel(_myBranchController);
        }
    }

    private void SetPressed()
    {
        SetButton(UIEventTypes.Selected);
        StartCoroutine(_buttonSize.PressedSequence());
        _colours.ProcessPress(Selected, setting);
    }

    public void RootCancel()
    {
        if (_preseveSelection != PreserveSelection.Toggle_NotLinked && _preseveSelection != PreserveSelection.ToggleGroup_OneAlwaysOn)
        {
            SetButton(UIEventTypes.Normal);
            _colours.ResetToNormal(setting);
            DisableChildLevel();
        }
    }

    public void OnCancel()
    {
        if (_isCancelOrBackButton)
        {
            SetButton(UIEventTypes.Normal);
            _colours.ResetToNormal(setting);
        }

        if (MyParentController.LastSelected == this) { return; }                    //Stops returning past the Home Level menus

        if (_preseveSelection != PreserveSelection.Toggle_NotLinked && _preseveSelection != PreserveSelection.ToggleGroup_OneAlwaysOn)
        {
            MyParentController.LastSelected.SetButton(UIEventTypes.Normal);
            MyParentController.LastSelected.DisableChildLevel();
        }
        else
        {
            bool temp = Selected;
            MyParentController.LastSelected.DisableChildLevel();
            Selected = temp;
        }
        _audio.Play(UIEventTypes.Cancelled, setting);
        MyParentController.MoveBackALevel();
    }

    public void SetNotHighlighted()
    {
        if (_preseveSelection == PreserveSelection.Never_TempSwitch)
        {
            DisableChildLevel();
        }

        if (Selected)
        {
            SetButton(UIEventTypes.Selected);
            _colours.SetColourOnExit(Selected, setting);
        }
        else
        {
            SetButton(UIEventTypes.Normal);
            _colours.ResetToNormal(setting);
        }
    }

    private void HandleIfDisabled(bool value) //TODO Review Code
    {
        MoveCursorToNextFree();
        DisableChildLevel();
        SetButton(UIEventTypes.Normal);
        _isDisabled = value;

        if (_isDisabled)
        {
            _colours.SetAsDisabled(setting);  //******Working except when already on something that's disabling. Need to set event data etc
        }
        else
        {
            _colours.ResetToNormal(setting);
        }
    }

    private void MoveCursorToNextFree()
    {
        if (_myBranchController.LastSelected == this)
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
}


