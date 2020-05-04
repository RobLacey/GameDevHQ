using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using NaughtyAttributes;

[RequireComponent(typeof(ColourLerp))]
public class UILeaf : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler,ISelectHandler, 
                                     IMoveHandler, IPointerUpHandler, ISubmitHandler, IPointerExitHandler
{
    [Header("Main Settings")] [HorizontalLine(4, color: EColor.Blue, order = 1)]
    [SerializeField] [Label("Move To When Clicked")] UIBranch _childBranch;
    [SerializeField] [Label("Is Cancel or Back Button")] bool _isCancelOrBackButton;
    [SerializeField] bool _highlightFirstChoice = true;
    [SerializeField] [Label("Preserve When Selected")] PreserveSelection _preseveSelection;
    [SerializeField] PreserveColour _preserveColour = PreserveColour.No;
    [Header("Settings (Click Arrows To Expand)")] [HorizontalLine(4, color: EColor.Blue, order = 1)]
    [SerializeField] [Label("Colour Settings")] UIColour _colours;
    [SerializeField] [Label("Invert Colour Options")] UIInvertColours _invertColourCorrection;
    [SerializeField] [Label("Change Size Settings")] UISize _buttonSize;
    [SerializeField] [Label("Audio Settings")] public UIAudio _audio;
    [SerializeField] [Label("Attached Accessories Settings")] UIAccessories _accessories;
    [SerializeField] [Label("Swap Image Or Text Settings")] UISwapper _swapImageOrText;

    //Variables
    UIBranch _masterLevelNode;
    Slider _amSlider;
    UIEventTypes _settings = UIEventTypes.Normal;

    //Delegates
    Action<UIEventTypes, bool> SetUp;

    //Properties & Enums
    enum PreserveSelection { Never_OnlyAsAFlickSwitch, UntilNewSelection, Always_IsAToggle }
    enum PreserveColour { Yes, No }
    public UIBranch MyParentController { get; set; }
    public bool AllowKeys { get; set; } = true;
    public bool Selected { get; set; }

    private void Awake()
    {
        _amSlider = GetComponent<Slider>();
        _colours.MyColourLerper = GetComponent<ColourLerp>();
        _masterLevelNode = GetComponentInParent<UIBranch>();
        _colours.OnAwake();
        _audio.OnAwake(GetComponentInParent<AudioSource>());
        if (_amSlider) _amSlider.interactable = false;
    }
    private void OnEnable()
    {
        SetUp += _accessories.OnAwake();
        SetUp += _buttonSize.OnAwake(transform);
        SetUp += _invertColourCorrection.OnAwake();
        SetUp += _swapImageOrText.OnAwake();
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
        if (eventData.pointerDrag) return; //Enables drag on slider to have pressed colour
        if (_amSlider) _amSlider.interactable = true;
        _masterLevelNode.MouseOverLast.SetNotHighlighted();
        _masterLevelNode.MouseOverLast = this;
        _audio.Play(UIEventTypes.Highlighted);
        _colours.SetColourOnEnter(Selected);
        SetButton(UIEventTypes.Highlighted);
        AllowKeys = false;
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        if (eventData.pointerDrag) return; //Enables drag on slider to have pressed colour
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
        if (_isCancelOrBackButton) { OnCancel(); return; }
        AllowKeys = false;
        SwitchUIDisplay();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_amSlider) SwitchUIDisplay();
    }

    public void OnSelect(BaseEventData eventData) //KB/Ctrl highlight
    {
        if (!AllowKeys) return;
        _masterLevelNode.MouseOverLast.SetNotHighlighted();
        _masterLevelNode.MouseOverLast = this;
        _audio.Play(UIEventTypes.Highlighted);
        _colours.SetColourOnEnter(Selected);
        SetButton(UIEventTypes.Highlighted);
    }

    public void OnSubmit(BaseEventData eventData) //KB/Ctrl
    {
        if (_isCancelOrBackButton) 
        {
            OnCancel();
            return; 
        }

        SwitchUIDisplay();

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
    }

    public void InitialiseStartUp()
    {
        AllowKeys = true;

        if (_amSlider) 
        { 
            _amSlider.interactable = false;
        }

        if (_preseveSelection != PreserveSelection.Always_IsAToggle)
        {
            Selected = false;
        }

        if (_highlightFirstChoice)
        {
            _colours.SetColourOnEnter(Selected);
            SetButton(UIEventTypes.Highlighted);
        }
        else
        {
            _colours.ResetToNormal();
        }
    }

    public void SetButton(UIEventTypes uIEventTypes)
    {
        _settings = uIEventTypes;
        SetUp.Invoke(_settings, Selected);
    }

    private void SwitchUIDisplay()
    {
        TurnOffLastSelected();
        _masterLevelNode.SaveLastSelected(this);
        _swapImageOrText.CycleToggleList();

        if (Selected)
        {
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
        UILeaf lastElementSelected;

        lastElementSelected = _masterLevelNode.LastSelected;

        if (lastElementSelected != this)
        {
            if (lastElementSelected._preseveSelection != PreserveSelection.Always_IsAToggle)
            {
                lastElementSelected.DisableChildLevel();
                lastElementSelected.SetNotHighlighted();
            }
        }
    }

    private void DisableChildLevel() 
    {
        Selected = false;

        if (_childBranch)
        {
            _childBranch.TurnOffBranch();

            if (_childBranch.LastSelected != null)
            {
                if (_childBranch.LastSelected._preseveSelection != PreserveSelection.Always_IsAToggle)
                {
                    _childBranch.LastSelected.DisableChildLevel();
                    _childBranch.LastSelected.SetNotHighlighted();
                }
            }
        }
    }

    private void ActivateChildLevel()
    {
        Selected = true;

        if (_childBranch)
        {
            _masterLevelNode.TurnOffOnMoveToChild();
            _childBranch.MoveToNextLevel(_masterLevelNode);
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
        if (_preseveSelection == PreserveSelection.Always_IsAToggle)
        {
            SetButton(UIEventTypes.Selected);
        }
        else
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

        if (_preseveSelection != PreserveSelection.Always_IsAToggle)
        {
            Selected = false;
        }

        if (MyParentController) 
        {
            MyParentController.LastSelected.DisableChildLevel();
            _audio.Play(UIEventTypes.Cancelled);
            MyParentController.LastSelected.SetButton(UIEventTypes.Normal);
            MyParentController.MoveToNextLevel();
            SetButton(UIEventTypes.Normal);
        }
    }

    public void SetNotHighlighted()
    {
        if (_amSlider) { _amSlider.interactable = false; }

        if (_preseveSelection == PreserveSelection.Never_OnlyAsAFlickSwitch)
        {
            DisableChildLevel();
        }
        if (_preserveColour == PreserveColour.Yes)
        {
            _colours.SetColourOnExit(Selected);
        }
        else
        {
            _colours.ResetToNormal();
        }

        if (Selected)
        {
            SetButton(UIEventTypes.Selected);
        }
        else
        {
            SetButton(UIEventTypes.Normal);
        }
    }
}


