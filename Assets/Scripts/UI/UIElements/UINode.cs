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
    [SerializeField] [HideIf("_isCancelOrBackButton")] bool _highlightFirstOption = true;
    [SerializeField] [HideIf("_isCancelOrBackButton")] [ValidateInput("SetChildBranch")] [Label("Button/Toggle Function")] 
    ButtonFunction _buttonFunction;
    [SerializeField] [HideIf(EConditionOperator.Or, "GroupSettings", "_isCancelOrBackButton")] 
    ToggleGroup _toggleGroupID = ToggleGroup.None;
    [SerializeField] [HideIf(EConditionOperator.Or, "GroupSettings", "_isCancelOrBackButton")] bool _startAsSelected;

    [Header("Settings (Click Arrows To Expand)")]
    [HorizontalLine(4, color: EColor.Blue, order = 1)]
    [EnumFlags] [Label("UI Functions To Use")] [SerializeField] public Setting _functionToUse;
    [SerializeField] [Label("Navigation And On Click Calls")] [ShowIf("UseNavigation")] public UINavigation _navigation;
    [SerializeField] [Label("Colour Settings")] [ShowIf("NeedColour")] UIColour _colours;
    [SerializeField] [Label("Invert Colour when Highlighted or Selected")] [ShowIf("NeedInvert")] UIInvertColours _invertColourCorrection;
    [SerializeField] [Label("Swap Images, Text or SetUp Toogle Image List")] [ShowIf("NeedSwap")] UISwapper _swapImageOrText;
    [SerializeField] [Label("Size And Position Effect Settings")] [ShowIf("NeedSize")] UISizeAndPosition _sizeAndPos;
    [SerializeField] [Label("Accessories, Outline Or Shadow Settings")] [ShowIf("NeedAccessories")] UIAccessories _accessories;
    [SerializeField] [Label("Audio Settings")] [ShowIf("NeedAudio")] public UIAudio _audio;
    [SerializeField] [Label("Tooltip Settings")] [ShowIf("NeedTooltip")] UITooltip _tooltips;

    //Variables
    Slider _amSlider;
    UIEventTypes _eventType = UIEventTypes.Normal;
    List<UINode> _toggleGroupMembers = new List<UINode>();
    bool _isDisabled;
    RectTransform _myRect;

    //Delegates
    Action<UIEventTypes, bool, Setting> SetUp;
    public static event Action<EscapeKey> Canceller;

    //Properties & Enums
    public UIBranch MyBranchController { get; set; }
    public EscapeKey EscapeKeyFunction { get { return _escapeKeyFunction; } }
   // public UIBranch MyParentController { get; set; }
    public bool IsSelected { get; set; }
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

    [Button] private void DisableObject() { IsDisabled = true; }
    [Button] private void EnableObject() { IsDisabled = false; }
    [Button] private void NotSelected() { SetNotSelected_NoEffects(); }
    [Button] private void AsSelected() { SetSelected_NoEffects(); }

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
    private bool UseNavigation() { return (_functionToUse & Setting.NavigationAndOnClick) != 0; }
    private bool NeedColour() { return (_functionToUse & Setting.Colours) != 0;  }
    private bool NeedSize(){ return (_functionToUse & Setting.SizeAndPosition) != 0; } 
    private bool NeedInvert(){ return (_functionToUse & Setting.InvertColourCorrection) != 0; } 
    private bool NeedSwap() { return (_functionToUse & Setting.SwapImageOrText) != 0; } 
    private bool NeedAccessories(){ return (_functionToUse & Setting.Accessories) != 0; } 
    private bool NeedAudio(){ return (_functionToUse & Setting.Audio) != 0; } 
    private bool NeedTooltip(){ return (_functionToUse & Setting.TooplTip) != 0; } 

    private bool GroupSettings()
    {
        if (_buttonFunction == ButtonFunction.ToggleGroup)
        {
            return false;
        }
        return true;
    }

    #endregion

    private void Awake()
    {
        _myRect = GetComponent<RectTransform>();
        _amSlider = GetComponent<Slider>();
        MyBranchController = GetComponentInParent<UIBranch>();
        _colours.OnAwake(gameObject.GetInstanceID());
        _audio.OnAwake(GetComponentInParent<AudioSource>());
        _tooltips.OnAwake(_functionToUse, gameObject.name);
        if (_amSlider) _amSlider.interactable = false;
    }

    private void Start()
    {
        SetChildsParentBranch();
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
        SetUp += _swapImageOrText.OnAwake(IsSelected);
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
        if (_buttonFunction != ButtonFunction.ToggleGroup) return;

        foreach (var node in MyBranchController.ThisGroupsUINodes)
        {
            if (node._buttonFunction == ButtonFunction.ToggleGroup)
            {
                if (node != this && _toggleGroupID == node._toggleGroupID)
                {
                    _toggleGroupMembers.Add(node);
                }

                if (_startAsSelected)
                {
                    IsSelected = true;
                    SetNotHighlighted();
                }
            }
        }
    }

    private void SetChildsParentBranch()
    {
        if (MyBranchController.MyBranchType == BranchType.Independent) return;

        if (_navigation._childBranch)
        {
            _navigation._childBranch.SetCurrentBranchAsParent(MyBranchController.ScreenType, MyBranchController);
            _navigation._childBranch.SetCurrentBranchAsParent(MyBranchController.ScreenType, MyBranchController);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (IsDisabled) return;
        if (eventData.pointerDrag) return;                  //Enables drag on slider to have pressed colour
        _audio.Play(UIEventTypes.Highlighted, _functionToUse);
        MyBranchController.SetLastHighlighted(this);
        SetAsHighlighted();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (IsDisabled) return;
        if (eventData.pointerDrag) return;                      //Enables drag on slider to have pressed colour
        SetNotHighlighted();
    }

    public void OnPointerDown(PointerEventData eventData = null)
    {
        if (IsDisabled) return;
        if (_isCancelOrBackButton)
        {
            Canceller?.Invoke(_escapeKeyFunction);
            return;
        }

        MyBranchController.SetLastSelected(this);
        SwitchUIDisplay();

        if (!_amSlider)
        {
            _navigation._asButtonEvent?.Invoke();
            _navigation._asToggleEvent?.Invoke(IsSelected);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (IsDisabled) return;
        if (_amSlider) 
        {
            _navigation._asButtonEvent?.Invoke();
            _navigation._asToggleEvent?.Invoke(IsSelected);
            SwitchUIDisplay(); 
        }
    }

    public void OnMove(AxisEventData eventData)
    {
        if (_amSlider)
        {
            if (IsSelected)
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

    public void OnSubmit(BaseEventData eventData)
    {
        if (IsDisabled) return;

        MyBranchController.SetLastSelected(this);

        if (_isCancelOrBackButton)
        {
            Canceller?.Invoke(_escapeKeyFunction);
            return;
        }
        SwitchUIDisplay();

        if (_amSlider)        //TODO Needs to be test witha slider
        { 
            _amSlider.interactable = IsSelected;
            if (!IsSelected)
            {
                _navigation._asButtonEvent?.Invoke();
                _navigation._asToggleEvent?.Invoke(IsSelected);
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
        if (IsDisabled) { HandleIfDisabled(IsDisabled); return; }

        MyBranchController.SetLastHighlighted(this);

        //if (_buttonFunction != ButtonFunction.Toggle_NotLinked
          //  && _buttonFunction != ButtonFunction.ToggleGroup)
       // {
            //IsSelected = false;
            if (_highlightFirstOption && MyBranchController.AllowKeys)
            {
                SetAsHighlighted();
            }
            else
            {
                SetNotHighlighted();
            }
        //}
        //else
        //{
        //    if (_highlightFirstOption && MyBranchController.AllowKeys)
        //    {
        //        Debug.Log("2");

        //        SetAsHighlighted();
        //    }
        //}
    }

    public void MoveToNext() //Review if can be combined with above or vise versa
    {
        if (!MyBranchController.AllowKeys) return;
        //MyBranchController.LastHighlighted.SetNotHighlighted();
        MyBranchController.SetLastHighlighted(this);
        _audio.Play(UIEventTypes.Highlighted, _functionToUse);
        SetAsHighlighted();
    }

    private void SwitchUIDisplay()
    {
        ToggleGroupElements();

        if (IsSelected)
        {
            if (_buttonFunction == ButtonFunction.ToggleGroup)
            {
                if (MyBranchController.LastHighlighted == this) return;
            }

            if (_buttonFunction == ButtonFunction.Toggle_NotLinked) { IsSelected = false; }

            Deactivate();
            DoPressedAction(UIEventTypes.Cancelled);

        }
        else
        {
            Activate();
            DoPressedAction(UIEventTypes.Selected);
        }
    }

    private void ToggleGroupElements()
    {
        if (_buttonFunction == ButtonFunction.ToggleGroup)
        {
            foreach (var item in _toggleGroupMembers)
            {
                item.IsSelected = false;
                item.Deactivate();
                item.SetNotHighlighted();
            }
        }
    }

    public void Deactivate()
    {
        if (_buttonFunction == ButtonFunction.Switch_NeverHold || _buttonFunction == ButtonFunction.Standard_Hold)
        {
            IsSelected = false;
            TurnOffChildren();
        }

        if (_amSlider) { QuitSlider(); }
        _swapImageOrText.CycleToggle(IsSelected, _functionToUse);
    }

    private void Activate()
    {
        if(_buttonFunction != ButtonFunction.Switch_NeverHold) IsSelected = true;
        if (_amSlider) { _amSlider.interactable = IsSelected; }
        _swapImageOrText.CycleToggle(IsSelected, _functionToUse);
        _tooltips.HideToolTip(_functionToUse);
        StopAllCoroutines();
        //MyBranchController.SetLastSelected(this);

        if (_buttonFunction == ButtonFunction.Switch_NeverHold || _buttonFunction == ButtonFunction.Standard_Hold)
        {
            if (_navigation._childBranch && (_functionToUse & Setting.NavigationAndOnClick) != 0)
            {
                MoveToChildBranch();
            }
        }
    }

    private void TurnOffChildren()
    {
        if (_navigation._childBranch && (_functionToUse & Setting.NavigationAndOnClick) != 0)
        {
            if (_navigation._childBranch.MoveToNext == MoveNext.OnClick)
            {
                _navigation._childBranch.StartOutTweens(false);
                TurnoffProcess();
            }
            else 
            {
                _navigation._childBranch.StartOutTweens(false, () => TurnoffProcess());
            }

            // if (_navigation._childBranch.LastHighlighted != null)
            //{
            //    _navigation._childBranch.LastHighlighted.SetNotHighlighted();
            //    _navigation._childBranch.LastSelected.Deactivate();
            //}
        }
    }

    private void TurnoffProcess()
    {
        _navigation._childBranch.LastHighlighted.SetNotHighlighted();
        _navigation._childBranch.LastSelected.Deactivate();
    }

    private void MoveToChildBranch() //**** Redo - Might neeed to make tweens work
    {
        if (MyBranchController.MoveToNext == MoveNext.AtTweenEnd)
        {
            MoveToChildAfterTween();
        }
        else
        {
            MoveToChildOnClick();
        }
    }


    private void MoveToChildAfterTween()
    {
        if (_navigation._childBranch.ScreenType == ScreenType.ToFullScreen)
        {
            MyBranchController.StartOutTweens(true, () => ToFullScreen_AfterTween());
        }
        else if (_navigation._childBranch.ScreenType == ScreenType.Normal)
        {
            if (_navigation._childBranch.MyBranchType == BranchType.Internal)
            {
                _navigation._childBranch.MoveToNextLevel(MyBranchController);
            }
            else
            {
                MyBranchController.StartOutTweens(true, () => ToBranchProcess_AfterTween());
            }
        }
    }

    private void MoveToChildOnClick()
    {
        if (_navigation._childBranch.ScreenType == ScreenType.ToFullScreen)
        {
            MyBranchController.StartOutTweens(true, () => ToBranchProcess_OnClick());
        }
        _navigation._childBranch.MoveToNextLevel(MyBranchController);
    }

    private void ToFullScreen_AfterTween()
    {
        _navigation._childBranch.TurnOffOnMoveToChild(MyBranchController);
        ToBranchProcess_AfterTween();
    }

    private void ToBranchProcess_AfterTween()
    {
        if (!MyBranchController.DontTurnOff)  { MyBranchController.MyCanvas.enabled = false; }
        _navigation._childBranch.MoveToNextLevel(MyBranchController);
    }

    //private void ToFullScreen_OnClick()
    //{
    //    if (!MyBranchController.DontTurnOff) { MyBranchController.MyCanvas.enabled = false; }
    //    _navigation._childBranch.TurnOffOnMoveToChild(MyBranchController);
    //}

    private void ToBranchProcess_OnClick()
    {
        if (!MyBranchController.DontTurnOff) { MyBranchController.MyCanvas.enabled = false; }
    }

    //public void OnCancel()
    //{
    //    _audio.Play(UIEventTypes.Cancelled, _functionToUse);
    //    _navigation._childBranch.StartOutTweens(false, () => MoveBackALevel());
    //}

    //private void MoveBackALevel() //*****Redo - Check tweens work on move back
    //{
    //    if (_navigation._doTween == TweenOnMove.NoTween || MyBranchController.DontTweenOnReturn)
    //    {
    //        MyBranchController.TweenOnChange = false;
    //        MyBranchController.MoveToNextLevel();
    //    }
    //    else
    //    {
    //        MyBranchController.MoveToNextLevel();
    //    }
    //}

    public void SetAsHighlighted()
    {
        if (IsDisabled) return;
        _colours.SetColourOnEnter(IsSelected, _functionToUse);
        ActivateFunctions(UIEventTypes.Highlighted);
        StartCoroutine(StartToopTip());
    }

    public void SetNotHighlighted()
    {
        StopAllCoroutines();
        _tooltips.HideToolTip(_functionToUse);

        if (IsSelected)
        {
            ActivateFunctions(UIEventTypes.Selected);
            _colours.SetColourOnExit(IsSelected, _functionToUse);
        }
        else
        {
            ActivateFunctions(UIEventTypes.Normal);
            _colours.ResetToNormal(_functionToUse);
        }
    }

    private void ActivateFunctions(UIEventTypes uIEventTypes)
    {
        _eventType = uIEventTypes;
        SetUp.Invoke(_eventType, IsSelected, _functionToUse);
    }

    private void DoPressedAction(UIEventTypes uIEventTypes)
    {
        ActivateFunctions(UIEventTypes.Selected);
        StartCoroutine(_sizeAndPos.PressedSequence(_functionToUse));
        _colours.ProcessPress(IsSelected, _functionToUse);
        _audio.Play(uIEventTypes, _functionToUse);
    }
    //public void HotKeyActivation()
    //{
    //    Activate();
    //    DoPressedAction(UIEventTypes.Selected);
    //}


    public void SetSelected_NoEffects()
    {
        IsSelected = true;
        SetNotHighlighted();
        _swapImageOrText.CycleToggle(IsSelected, _functionToUse);
    }

    public void SetNotSelected_NoEffects()
    {
        IsSelected = false;
        SetNotHighlighted();
        _swapImageOrText.CycleToggle(IsSelected, _functionToUse);
    }

    private void HandleIfDisabled(bool value) //TODO Review Code
    {
        MoveCursorToNextFree();
        Deactivate();
        ActivateFunctions(UIEventTypes.Normal);
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
        if (MyBranchController.LastHighlighted == this)
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
                //OnCancel();
                Debug.Log("No where to go except down");
            }
        }
    }

    private void QuitSlider()
    {
        _navigation._asButtonEvent?.Invoke();
        _amSlider.interactable = IsSelected;
    }

    private IEnumerator StartToopTip()
    {
        if ((_functionToUse & Setting.TooplTip) != 0)
        {
            yield return new WaitForSeconds(_tooltips._delay);
            _tooltips.IsActive = true;
            StartCoroutine(_tooltips.ToolTipBuild(_myRect));
            StartCoroutine(_tooltips.StartTooltip(MyBranchController.AllowKeys));
        }
        yield return null;
    }
}


