using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class ButtonMaster : MonoBehaviour
{
    [SerializeField] ButtonController _defaultStartPosition;
    [SerializeField] bool _turnOffOnMoveToChild;
    [SerializeField] bool _saveSelectedOnExit;

    ButtonController[] _selectables;
    [SerializeField] ButtonController _lastMovedFrom;

    public ButtonController DefaultStartPosition { get { return _defaultStartPosition; } }
    public Canvas MyCanvas { get; set; }

    private void Awake()
    {
        MyCanvas = GetComponent<Canvas>();
        _lastMovedFrom = _defaultStartPosition;
        _selectables = GetComponentsInChildren<ButtonController>();
    }

    public void FirstSelected(ButtonMaster buttonMaster = null)
    {
        MyCanvas.enabled = true;
        EventSystem.current.SetSelectedGameObject(_lastMovedFrom.gameObject);
        _lastMovedFrom.SetHighlight_Nothing_Selected();
        if (buttonMaster != null)
        {
            foreach (var item in _selectables)
            {
                item.MyParentController = buttonMaster;
            }
        }
    }

    public void ClearOtherSelections(GameObject eventData = null)
    {
        foreach (var item in _selectables)
        {
            if (item.gameObject != eventData)
            {
                item.ClearUISelection();
            }
        }
    }

    public void OnHoverOver(GameObject eventData = null)
    {
        foreach (var item in _selectables)
        {
            if (item.gameObject != eventData)
            {
                item.NotHighlighted();
            }
        }
    }

    public void MoveToChildLevel()
    {
        if (_turnOffOnMoveToChild)
        {
            MyCanvas.enabled = false;
        }
    }

    public void MoveToParentLevel()
    {
        ClearOtherSelections();
        MyCanvas.enabled = false;
    }

    public void SetLastSelected(ButtonController lastSelected)
    {
        if (_saveSelectedOnExit)
        {
            _lastMovedFrom = lastSelected; //Fix This
        }
    }
}
