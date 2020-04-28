using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(GraphicRaycaster))]

public class UIBranch : MonoBehaviour
{
    [SerializeField] UILeaf _userDefinedStartPosition;
    [SerializeField] bool _isTopLevel;
    [SerializeField] bool _turnOffOnMoveToChild;
    [SerializeField] bool _saveExitSelection;
    [SerializeField] bool _killAllOtherUI;
    [SerializeField] UILeaf _lastMouseOver;


    //Variables
    UILeaf[] _selectables;
    UITrunk _UICancelStopper;

    //Properties
    public UILeaf DefaultStartPosition { get { return _userDefinedStartPosition; } }
    public Canvas MyCanvas { get; set; }
    public UILeaf MouseOverLast { get { return _lastMouseOver; } set { _lastMouseOver = value; } }

    public UILeaf LastMovedFrom { get; set; }
    public UIGroupID MyUIGroup { get; set; }
    public bool CanSaveLastSelection { get { return _saveExitSelection; } }
    public bool KillAllOtherUI { get { return _killAllOtherUI; } }

    private void Awake()
    {
        _UICancelStopper = FindObjectOfType<UITrunk>();
        MyCanvas = GetComponent<Canvas>();
        _selectables = GetComponentsInChildren<UILeaf>();

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
        LastMovedFrom = _userDefinedStartPosition;

        MouseOverLast = _userDefinedStartPosition;

        if (!_isTopLevel)
        {
            MyCanvas.enabled = false;
        }
        else
        {
            MyCanvas.enabled = true;
        }
    }

    public void MoveToNextLevel(UIBranch newParentController = null)
    {
        if (_killAllOtherUI)
        {
            _UICancelStopper.KillAllOtherUI();
        }
        MyCanvas.enabled = true;

        if (_userDefinedStartPosition != null)
        {
            if (!_saveExitSelection)
            {
                LastMovedFrom = DefaultStartPosition;
                LastMovedFrom.SetNotHighlighted();
            }
            MouseOverLast.SetNotHighlighted();
            MouseOverLast = LastMovedFrom;
            LastMovedFrom.AllowKeys = false;
            EventSystem.current.SetSelectedGameObject(LastMovedFrom.gameObject);
            LastMovedFrom.InitialiseStartUp();
        }

        if (newParentController != null)
        {
            MyUIGroup = newParentController.MyUIGroup;
            foreach (var item in _selectables)
            {
                item.MyParentController = newParentController;
            }
        }
        _UICancelStopper.SetLastUIObject(LastMovedFrom, MyUIGroup);
    }

    public void TurnOffOnMoveToChild()
    {
        if (_turnOffOnMoveToChild){ MyCanvas.enabled = false; }
    }

    public void SaveLastSelected(UILeaf lastSelected)
    {
        _UICancelStopper.SetLastUIObject(lastSelected, MyUIGroup);
        LastMovedFrom = lastSelected;
    }
}
