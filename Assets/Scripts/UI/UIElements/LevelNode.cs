using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(AudioSource))]
public class LevelNode : MonoBehaviour
{
    [SerializeField] UIElementSetUp _defaultStartPosition;
    [SerializeField] bool _isTopLevel;
    [SerializeField] bool _turnOffOnMoveToChild;
    [SerializeField] bool _saveSelectedOnExit;
    [SerializeField] UIElementSetUp _lastMovedFrom;

    //Variables
    UIElementSetUp[] _selectables;
    UIManager _UICancelStopper;

    //Properties
    public UIElementSetUp DefaultStartPosition { get { return _defaultStartPosition; } }
    public Canvas MyCanvas { get; set; }
    public UIElementSetUp LastMovedFrom { get { return _lastMovedFrom; } }

    private void Awake()
    {
        _UICancelStopper = FindObjectOfType<UIManager>();
        MyCanvas = GetComponent<Canvas>();
        _lastMovedFrom = _defaultStartPosition;
        _selectables = GetComponentsInChildren<UIElementSetUp>();
        if (!_isTopLevel)
        {
            MyCanvas.enabled = false;
        }
        else
        {
            MyCanvas.enabled = true;
        }
    }

    public void MoveToNextLevel(LevelNode buttonMaster = null)
    {
        MyCanvas.enabled = true;
        EventSystem.current.SetSelectedGameObject(_lastMovedFrom.gameObject); //Need for Keyboard and Controller
        _UICancelStopper.SetLastUIObject(_lastMovedFrom.gameObject);
        ResetLevel();
        _lastMovedFrom.ButtonStatus = UIEventTypes.Highlighted;

        if (buttonMaster != null)
        {
            foreach (var item in _selectables)
            {
                item.MyParentController = buttonMaster;
            }
        }
    }

    public void MoveToLevel_GroupSwitch()
    {
        EventSystem.current.SetSelectedGameObject(_lastMovedFrom.gameObject); //Need for Keyboard and Controller
        _UICancelStopper.SetLastUIObject(_lastMovedFrom.gameObject);
        _lastMovedFrom.ButtonStatus = UIEventTypes.Highlighted;
    }

    public void ResetLevel()
    {
        foreach (var item in _selectables)
        {
            item.ResetUI();
        }
    }

    public void OnMovingToChildLevel()
    {
        if (_turnOffOnMoveToChild){ MyCanvas.enabled = false; }
    }

    public void SaveLastSelected(UIElementSetUp lastSelected)
    {
        _UICancelStopper.SetLastUIObject(lastSelected.gameObject);

        if (_saveSelectedOnExit)
        {
            _lastMovedFrom = lastSelected; //Fix This
        }
    }
}
