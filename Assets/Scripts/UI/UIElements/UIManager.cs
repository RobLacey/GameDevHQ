using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class UIManager : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] LevelNode _uiTopLevel;
    [SerializeField] InGame _clickOff;
    [SerializeField] EscapeKey _escapeKeyToCancel;
    [SerializeField] GroupList[] _groupList;

    //Variables
    GameObject _uiElement;
    int _groupIndex = 0;
    UIElementSetUp[] _allButtonControllers;
    enum InGame { ReturnToLastSelected, Disable, ClearUI }
    enum EscapeKey { OneLevel, BackToRootLevel }

    [Serializable]
    public class GroupList
    {
        public UIGroupID _uIGroupID;
        public LevelNode _GroupTopLevel;
    }

    private void Awake()
    {
        _allButtonControllers = FindObjectsOfType<UIElementSetUp>();
    }

    private void Start()
    {
        _uiElement = _uiTopLevel.DefaultStartPosition.gameObject;
        _uiTopLevel.MoveToNextLevel();
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            OnCancel();
        }

        if (Input.GetKeyUp(KeyCode.U))
        {
            OnHoverOver();
            _groupList[_groupIndex]._GroupTopLevel = _uiElement.GetComponentInParent<LevelNode>();
            _groupIndex++;
            if (_groupIndex > _groupList.Length - 1)
            {
                _groupIndex = 0;
            }
            _groupList[_groupIndex]._GroupTopLevel.MoveToLevel_GroupSwitch();
        }
    }

    public void SetLastUIObject(GameObject uiObject) 
    {
        _uiElement = uiObject;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_clickOff == InGame.ReturnToLastSelected)
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                EventSystem.current.SetSelectedGameObject(_uiElement);
            }
        }    
    }

    public void OnHoverOver(GameObject eventData = null)
    {
        foreach (var item in _allButtonControllers)
        {
            if (item.gameObject != eventData)
            {
                item.SetNotHighlighted();
            }
        }
    }

    public void OnCancel()
    {
        if (_escapeKeyToCancel == EscapeKey.OneLevel)
        {
            UIElementSetUp temp = _uiElement.GetComponent<UIElementSetUp>();
            temp.ButtonStatus = UIEventTypes.Cancelled;
        }

        if (_escapeKeyToCancel == EscapeKey.BackToRootLevel)
        {
            _uiTopLevel.LastMovedFrom.DisableChildLevel();
            _uiTopLevel.LastMovedFrom.ButtonStatus = UIEventTypes.Highlighted;
        }
    }
}
