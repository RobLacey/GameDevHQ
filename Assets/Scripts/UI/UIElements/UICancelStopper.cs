using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UICancelStopper : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] ButtonMaster _uiTopLevel;

    //Variables
    GameObject _uiElement;

    List<ButtonMaster> _openList = new List<ButtonMaster>();

    private void Awake()
    {
        ButtonMaster[] allButtonMasters = FindObjectsOfType<ButtonMaster>();
        foreach (var item in allButtonMasters)
        {
            item.MyCanvas.enabled = false;
        }
        _uiElement = _uiTopLevel.DefaultStartPosition.gameObject;
        _uiTopLevel.FirstSelected();
        AddToTrackedList(_uiTopLevel);
    }

    public void SetLastUIObject(GameObject uiObject)
    {
        _uiElement = uiObject;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            EventSystem.current.SetSelectedGameObject(_uiElement);
        }
    }

    public void AddToTrackedList(ButtonMaster value)
    {
        RemoveFromTrackedList(value);

        if (!_openList.Contains(value))
        {
            _openList.Add(value);
        }
    }

    public void RemoveFromTrackedList(ButtonMaster value)
    {
        if (_openList.Contains(value))
        {
            ManageTrackedList(value);                
        }
    }

    private void ManageTrackedList(ButtonMaster value)
    {
        for (int index = _openList.Count - 1; index > 0; index--)
        {
            if (_openList[index] == value)
            {
                return;
            }
            else
            {
                _openList[index].MoveToParentLevel();
                _openList.RemoveAt(index);
            }
        }
    }
}
