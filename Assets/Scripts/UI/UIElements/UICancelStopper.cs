using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UICancelStopper : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] ButtonMaster _uiTopLevel;
    [SerializeField] GameObject _gameObject;
    [SerializeField] List<ButtonMaster> _openList = new List<ButtonMaster>();

    //Variables
    GameObject _uiElement;

    private void Awake()
    {
        ButtonMaster[] allButtonMasters = FindObjectsOfType<ButtonMaster>();
        foreach (var item in allButtonMasters)
        {
            item.MyCanvas.enabled = false;
        }
        _uiElement = _uiTopLevel.DefaultStartPosition.gameObject;
        _uiTopLevel.FirstSelected();
        TrackOpenMenus(_uiTopLevel);
    }

    private void Update()
    {
        _gameObject = EventSystem.current.currentSelectedGameObject;
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

    public void TrackOpenMenus(ButtonMaster value)
    {
        if (_openList.Contains(value))
        {
            for (int index = _openList.Count - 1; index > 0; index--)
            {
                if (_openList[index] == value)
                {
                    return;
                }
                else
                {
                    Debug.Log(_openList[index]);
                    _openList[index].MoveToParentLevel(_uiElement.GetComponent<ButtonController>());
                    _openList.RemoveAt(index);
                }
            }
        }
        else
        {
            _openList.Add(value);
        }
    }
}
