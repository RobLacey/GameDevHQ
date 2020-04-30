using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(GraphicRaycaster))]
[RequireComponent(typeof(UITweener))]

public class UIBranch : MonoBehaviour
{
    [SerializeField] UILeaf _userDefinedStartPosition;
    [SerializeField] bool _onScreenAtStart;
    [SerializeField] bool _turnOffOnMoveToChild;
    [SerializeField] bool _saveExitSelection;
    [SerializeField] bool _killAllOtherUI;
    [SerializeField] PositionTween _positionTransition = PositionTween.NoTween;
    [SerializeField] ScaleTween _scaleTransition = ScaleTween.NoTween;
    [SerializeField] FadeTween _fadeTransition = FadeTween.NoTween;

    //Variables
    UILeaf[] _selectables;
    UITrunk _UICancelStopper;
    UIBranch _currentChildrensParent;
    UITweener _UITweener;

    //Properties
    public UILeaf DefaultStartPosition { get { return _userDefinedStartPosition; } }
    public Canvas MyCanvas { get; set; }
    public UILeaf MouseOverLast { get; set; }
    public UILeaf LastSelected { get; set; }
    public UIGroupID MyUIGroup { get; set; }
    public bool CanSaveLastSelection { get { return _saveExitSelection; } }
    public bool KillAllOtherUI { get { return _killAllOtherUI; } }

    private void Awake()
    {
        _UITweener = GetComponent<UITweener>();
        _UICancelStopper = FindObjectOfType<UITrunk>();
        MyCanvas = GetComponent<Canvas>();
        _selectables = GetComponentsInChildren<UILeaf>();
    }

    private void Start()
    {
        SetStartPositions();
        SetUpTweener();

        if (!_onScreenAtStart)
        {
            MyCanvas.enabled = false;
        }
        else
        {
            MyCanvas.enabled = true;
        }
    }

    private void SetUpTweener()
    {
        if (_UITweener)
        {
            if (_positionTransition != PositionTween.NoTween)
            {
                _UITweener.SetUpPositionTweens(_positionTransition);
            }

            if (_scaleTransition != ScaleTween.NoTween)
            { 
                _UITweener.SetUpScaleTweens(_scaleTransition);
            }
        }
    }

    private void SetStartPositions()
    {
        if (_userDefinedStartPosition == null)
        {
            foreach (Transform item in transform)
            {
                if (item.GetComponent<UILeaf>())
                {
                    _userDefinedStartPosition = item.GetComponent<UILeaf>();
                    break;
                }
            }
        }
        LastSelected = _userDefinedStartPosition;
        MouseOverLast = _userDefinedStartPosition;
    }

    public void MoveToNextLevel(UIBranch newParentController = null)
    {
        if (_killAllOtherUI) { _UICancelStopper.KillAllOtherUI(); }

        MyCanvas.enabled = true;
        InitialiseFirstUIElement();
        SetCurrentBranchAsParent(newParentController); // TODO Review this as might not be needed id everything is parented at start
        _UICancelStopper.SetLastUIObject(LastSelected, MyUIGroup);

        if (_positionTransition != PositionTween.NoTween)
        {
            if (_UITweener) //*****loose These when everything setup corretcly
            {
                _UITweener.ActivatePositionTweens(_positionTransition, true);
            } 
        }

        if (_scaleTransition != ScaleTween.NoTween)
        {
            if (_UITweener) //*****loose These when everything setup corretcly
            {
                _UITweener.ActivateScaleTweens(_scaleTransition, true);
            } 
        }
    }

    private void SetCurrentBranchAsParent(UIBranch newParentController) //Needed in case menu is called from different places
    {
        if (newParentController != null && _currentChildrensParent != newParentController)
        {
            _currentChildrensParent = newParentController;
            MyUIGroup = newParentController.MyUIGroup;
            foreach (var item in _selectables)
            {
                item.MyParentController = newParentController;
            }
        }
    }

    private void InitialiseFirstUIElement()
    {
        if (_userDefinedStartPosition != null)
        {
            if (!_saveExitSelection)
            {
                LastSelected = DefaultStartPosition;
                LastSelected.SetNotHighlighted();
            }

            MouseOverLast.SetNotHighlighted();
            MouseOverLast = LastSelected;
            LastSelected.AllowKeys = false;
            EventSystem.current.SetSelectedGameObject(LastSelected.gameObject);
            LastSelected.InitialiseStartUp();
        }
    }

    public void TurnOffOnMoveToChild()
    {
        if (_turnOffOnMoveToChild){ MyCanvas.enabled = false; }
    }

    public void SaveLastSelected(UILeaf lastSelected)
    {
        _UICancelStopper.SetLastUIObject(lastSelected, MyUIGroup);
        LastSelected = lastSelected;
    }

    public void TurnOffBranch()
    {
        if (_positionTransition == PositionTween.NoTween && _scaleTransition == ScaleTween.NoTween)
        {
            MyCanvas.enabled = false;
            return;
        }

        if (_UITweener)//***Loose this eventually
        {
            _UITweener.ActivateScaleTweens(_scaleTransition, false, () => MyCanvas.enabled = false); //Add bigger method to fix scale issue
            _UITweener.ActivatePositionTweens(_positionTransition, false, () => MyCanvas.enabled = false); //Add bigger method to fix scale issue
        }
    }
}
