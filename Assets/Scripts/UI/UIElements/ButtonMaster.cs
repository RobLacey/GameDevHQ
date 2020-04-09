using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(AudioSource))]
public class ButtonMaster : MonoBehaviour
{
    [SerializeField] ButtonController _defaultStartPosition;
    [SerializeField] bool _turnOffOnMoveToChild;
    [SerializeField] bool _saveSelectedOnExit;

    ButtonController[] _selectables;
    AudioSource _audioSource;
    ButtonController _lastMovedFrom;
    UICancelStopper _uICancelStopper;
    Canvas _myCanvas;

    public ButtonController DefaultStartPosition { get { return _defaultStartPosition; } }
    public Canvas MyCanvas { get { return _myCanvas; } set{ _myCanvas = value; } }

    private void Awake()
    {
        _myCanvas = GetComponent<Canvas>();
        _uICancelStopper = FindObjectOfType<UICancelStopper>();
        _lastMovedFrom = _defaultStartPosition;
        _selectables = GetComponentsInChildren<ButtonController>();
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null) {Debug.Log("No AudioSource + " + gameObject); return; }

        foreach (var item in _selectables)
        {
            item._audio.MyAudiosource = _audioSource;
        }
    }

    public void FirstSelected()
    {
        _myCanvas.enabled = true;
        //EventSystem.current.SetSelectedGameObject(_lastMovedFrom.gameObject);
        _lastMovedFrom.SetHighlight_Nothing_Selected();
    }

    public void ClearOtherSelections(GameObject eventData)
    {
        // Add in clearing a list of children to reach current level or clear all if another button pressed
        foreach (var item in _selectables)
        {
            if (item.gameObject != eventData)
            {
                item.ClearUISelection();
            }
        }
    }

    public void OnHoverOver(GameObject eventData)
    {
        foreach (var item in _selectables)
        {
            if (item.gameObject != eventData)
            {
                item.NotHighlighted();
            }
        }
    }

    public void MoveToChildLevel(ButtonController lastSelected)
    {
        //_uICancelStopper.OpenList = this;
        //Debug.Log(gameObject.name + " : Move to Move to child");
        _lastMovedFrom = lastSelected;
        if (_turnOffOnMoveToChild)
        {
            _myCanvas.enabled = false;
        }
    }

    public void MoveToParentLevel(ButtonController lastSelected)
    {
        //_uICancelStopper.OpenList = this;
        //Debug.Log(gameObject.name + " : Move to parent");
        if (_saveSelectedOnExit)
        {
            _lastMovedFrom = lastSelected;
        }
        else
        {
            _lastMovedFrom = _defaultStartPosition;
        }
        _myCanvas.enabled = false;
    }
}
