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
    [Header("Main Settings")] [HorizontalLine(4, color: EColor.Blue, order = 1)]
    [SerializeField] [Label("Is Cancel or Back Button")] bool _isCancelOrBackButton;
    [SerializeField] bool _highlightFirstOption = true;
    [SerializeField] [ReadOnly] bool _isDisabled;
    [SerializeField] [Label("Preserve When Selected")] PreserveSelection _preseveSelection;
    [SerializeField] [HideIf("GroupSettings")] ToggleGroup _toggleGroupID = ToggleGroup.None;
    [Header("Settings (Click Arrows To Expand)")] [HorizontalLine(4, color: EColor.Blue, order = 1)]
    [SerializeField] [Label("Navigation And On Click Calls")] UINavigation _navigation;
    [SerializeField] [Label("Colour Settings")] UIColour _colours;
    [SerializeField] [Label("Invert Colour when Highlighted or Selected")] UIInvertColours _invertColourCorrection;
    [SerializeField] [Label("Swap Images, Text or SetUp Toogle Image List")] UISwapper _swapImageOrText;
    [SerializeField] [Label("Change Size Settings")] UISize _buttonSize;
    [SerializeField] [Label("Accessories, Outline Or Shadow Settings")] UIAccessories _accessories;
    [SerializeField] [Label("Audio Settings")] public UIAudio _audio;

    [Button] private void DisableObject() { Disabled = true; }
    [Button] private void EnableObject() { Disabled = false; }

    //Variables
    UIBranch _myBranchController;
    Slider _amSlider;
    UIEventTypes _settings = UIEventTypes.Normal;
    UILeaf[] _toggleGroupMembers;

    //Delegates
    Action<UIEventTypes, bool> SetUp;

    //Properties & Enums
    enum PreserveSelection { Never, NewSelectionAnyWhere, GroupOnly_AllOff, GroupOnly_OneAlwaysOn, Independent }
    public UIBranch MyParentController { get; set; }
    public bool AllowKeys { get; set; } = true;
    public bool Selected { get; set; }
    public bool IsOn { get; set; } //TODO Test I actually Need this

    public bool Disabled 
    {
        get { return _isDisabled; }
        set
        {
            HandleIfDisabled(value);
        }
    }

    #region Editor Scripts

    public bool GroupSettings()
    {
        if (_preseveSelection == PreserveSelection.GroupOnly_OneAlwaysOn || _preseveSelection == PreserveSelection.GroupOnly_AllOff)
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
        foreach (var leaf in MyParentController.ThisGroupsUILeafs)
        {
            if (leaf._preseveSelection == PreserveSelection.GroupOnly_AllOff 
                || leaf._preseveSelection == PreserveSelection.GroupOnly_OneAlwaysOn)
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

    public void OnPointerEnter(PointerEventData eventData) //Mouse highlight
    {
        if (Disabled) return;
        if (eventData.pointerDrag) return;                  //Enables drag on slider to have pressed colour
        if (_amSlider) _amSlider.interactable = true;
        SetAsHighlighted();
        AllowKeys = false;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (Disabled) return;
        if (eventData.pointerDrag) return;                      //Enables drag on slider to have pressed colour
        if (_amSlider) 
        { 
            _amSlider.interactable = false; 
            EventSystem.current.SetSelectedGameObject(gameObject); //Needed for Keyboard/Ctrl 
        }
        SetNotHighlighted();
        AllowKeys = true;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (Disabled) return;
        if (_isCancelOrBackButton) { OnCancel(); return; }
        AllowKeys = false;
        SwitchUIDisplay();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (Disabled) return;
        if (_amSlider) SwitchUIDisplay();
        SwitchedOnOrOff();
        _navigation._asButtonEvent?.Invoke();
    }

    public void MoveToNext() //KB/Ctrl highlight
    {
        if (!AllowKeys) return;
        EventSystem.current.SetSelectedGameObject(gameObject);
        _myBranchController.SaveLastSelected(this);
        SetAsHighlighted();
    }

    public void OnSubmit(BaseEventData eventData) //KB/Ctrl
    {
        if (Disabled) return;

        if (_isCancelOrBackButton) 
        {
            OnCancel();
            return; 
        }

        SwitchUIDisplay();
        _navigation._asButtonEvent?.Invoke();

        if (_amSlider)
        {
            if (_amSlider.interactable == true)
            {
                if (_amSlider) _amSlider.interactable = false;
                EventSystem.current.SetSelectedGameObject(gameObject); //Needed for Keyboard/Ctrl 
            }
            else
            {
                if (_amSlider) _amSlider.interactable = true;
            }
        }
    }

    public void OnMove(AxisEventData eventData) 
    {
        if (_amSlider)
        {
            if (_amSlider.interactable == true)
            {
                if (eventData.moveDir == MoveDirection.Left || eventData.moveDir == MoveDirection.Right)
                {
                    _audio.Play(UIEventTypes.Selected);
                }
            }
        }
        _navigation.ProcessMoves(eventData);
    }

    private void SwitchedOnOrOff()
    {
        if (IsOn == true)
        {
            IsOn = false;
        }
        else
        {
            IsOn = true;
        }
        _navigation._asToggleEvent?.Invoke(IsOn);
    }

    public void InitialiseStartUp()
    {
        if (Disabled) { HandleIfDisabled(Disabled); return; }

        AllowKeys = true;
        EventSystem.current.SetSelectedGameObject(gameObject);
        _myBranchController.LastSelected = this;

        if (_amSlider) 
        { 
            _amSlider.interactable = false;
        }

        if (_preseveSelection != PreserveSelection.Independent 
            && _preseveSelection != PreserveSelection.GroupOnly_OneAlwaysOn)
        {
            Selected = false;
            if (_highlightFirstOption)
            {
                _colours.SetColourOnEnter(Selected);
                SetButton(UIEventTypes.Highlighted);
            }
            else
            {
                _colours.ResetToNormal();
            }
        }
        else
        {
            if (_highlightFirstOption)
            {
                _colours.SetColourOnEnter(Selected);
                SetButton(UIEventTypes.Highlighted);
            }
        }
    }

    public void SetAsHighlighted()
    {
        if (Disabled) return;
        _myBranchController.SetLastHighlighted(this);
        _myBranchController.LastSelected.SetNotHighlighted();
        _audio.Play(UIEventTypes.Highlighted);
        _colours.SetColourOnEnter(Selected);
        SetButton(UIEventTypes.Highlighted);
    }

    private void SetButton(UIEventTypes uIEventTypes)
    {
        _settings = uIEventTypes;
        SetUp.Invoke(_settings, Selected);
    }

    private void SwitchUIDisplay()
    {
        TurnOffLastSelected();
        _myBranchController.SaveLastSelected(this);

        if (Selected)
        {
            if (_preseveSelection == PreserveSelection.GroupOnly_OneAlwaysOn)
            {
                if (_myBranchController.LastSelected == this) return;
            }
            DisableChildLevel();
            SetPressed();
            _audio.Play(UIEventTypes.Cancelled);

        }
        else
        {
            ActivateChildLevel();
            SetPressed();
            _audio.Play(UIEventTypes.Selected);
        }
    }

    private void TurnOffLastSelected()
    {
        UILeaf lastElementSelected = _myBranchController.LastSelected;
        if(_preseveSelection == PreserveSelection.GroupOnly_OneAlwaysOn)
        {
            foreach (var item in _toggleGroupMembers)
            {
                item.DisableChildLevel();
                item.SetNotHighlighted();
            }
        }
        if (lastElementSelected != this)
        {
            if (lastElementSelected._preseveSelection != PreserveSelection.Independent 
                && lastElementSelected._preseveSelection != PreserveSelection.GroupOnly_OneAlwaysOn)
            {
                lastElementSelected.DisableChildLevel();
                lastElementSelected.SetNotHighlighted();
            }
        }
    }

    private void DisableChildLevel() 
    {
        Selected = false;
        _swapImageOrText.CycleToggleList(Selected);

        if (_navigation._childBranch)
        {
            _navigation._childBranch.TurnOffBranch();

            if (_navigation._childBranch.LastSelected != null)
            {
                if (_navigation._childBranch.LastSelected._preseveSelection != PreserveSelection.Independent)
                {
                    _navigation._childBranch.LastSelected.DisableChildLevel();
                    _navigation._childBranch.LastSelected.SetNotHighlighted();
                }
            }
        }
    }

    private void ActivateChildLevel()
    {
        Selected = true;
        _swapImageOrText.CycleToggleList(Selected);

        if (_navigation._childBranch)
        {
            _myBranchController.TurnOffOnMoveToChild();
            _navigation._childBranch.MoveToNextLevel(_myBranchController);
        }
    }

    private void SetPressed()
    {
        SetButton(UIEventTypes.Selected);
        StartCoroutine(_buttonSize.PressedSequence());
        _colours.ProcessPress(Selected);
    }

    public void RootCancel()
    {
        if (_preseveSelection != PreserveSelection.Independent && _preseveSelection != PreserveSelection.GroupOnly_OneAlwaysOn)
        {
            SetButton(UIEventTypes.Normal);
            _colours.ResetToNormal();
            DisableChildLevel();
        }
    }

    public void OnCancel()
    {
        if (_amSlider) { _amSlider.interactable = false; }

        if (_isCancelOrBackButton)
        {
            SetButton(UIEventTypes.Normal);
            _colours.ResetToNormal();
        }

        if (_preseveSelection != PreserveSelection.Independent && _preseveSelection != PreserveSelection.GroupOnly_OneAlwaysOn)
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
        _audio.Play(UIEventTypes.Cancelled);
        MyParentController.MoveBackALevel();
    }

    public void SetNotHighlighted()
    {
        if (_preseveSelection == PreserveSelection.Never)
        {
            DisableChildLevel();
        }

        if (Selected)
        {
            SetButton(UIEventTypes.Selected);
            _colours.SetColourOnExit(Selected);
        }
        else
        {
            SetButton(UIEventTypes.Normal);
            _colours.ResetToNormal();
        }
    }

    private void HandleIfDisabled(bool value)
    {
        if (_myBranchController.LastSelected == this)
        {
            if (_navigation._setNavigation != UINavigation.NavigationType.None)
            {
                if (_navigation.down) { _navigation.down.MoveToNext(); }
                else if (_navigation.right) { _navigation.right.MoveToNext(); }
            }
            else
            {
                OnCancel();
                Debug.Log("No where to go except down");
            }
        }

        DisableChildLevel();
        SetButton(UIEventTypes.Normal);
        _isDisabled = value;
        if (_isDisabled)
        {
            _colours.SetAsDisabled();  //******Working except when already on something that's disabling. Need to set event data etc
        }
        else
        {
            _colours.ResetToNormal();
        }
    }
}


