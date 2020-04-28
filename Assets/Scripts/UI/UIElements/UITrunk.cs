using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class UITrunk : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] UIBranch _uiTopLevel;
    [SerializeField] UILeaf _uiElement;
    [SerializeField] InGame _clickOff;
    [SerializeField] EscapeKey _escapeKeyToCancel;
    [SerializeField] GroupList[] _groupList;

    //Variables
    int _groupIndex = 0;
    UILeaf[] _allButtonControllers;
    bool hasActiveChild;

    public UIGroupID ActiveGroup { get; set; }
    enum InGame { ReturnToLastSelected, Disable, ClearUI }
    enum EscapeKey { OneLevel, BackToRootLevel }

    [Serializable]
    public class GroupList
    {
        public UIGroupID _uIGroupID;
        public UIBranch _GroupTopLevel;
    }

    private void Awake()
    {
        _allButtonControllers = FindObjectsOfType<UILeaf>();
        foreach (var item in _groupList)
        {
            item._GroupTopLevel.MyUIGroup = item._uIGroupID;
        }
    }

    private void Start()
    {
        _uiElement = _uiTopLevel.DefaultStartPosition;
        _uiTopLevel.MoveToNextLevel();
        _uiTopLevel.MouseOverLast = _uiElement;
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            OnCancel();
        }

        if (Input.GetKeyUp(KeyCode.U))
        {
            _groupIndex++;
            if (_groupIndex > _groupList.Length - 1)
            {
                _groupIndex = 0;
            }
            _groupList[_groupIndex]._GroupTopLevel.MoveToNextLevel();
        }
    }

    public void SetLastUIObject(UILeaf uiObject, UIGroupID uIGroupID) 
    {
        if (uIGroupID != ActiveGroup)
        {
            _uiElement.SetNotHighlighted();
            RootCancelProcess();
            ActiveGroup = uIGroupID;
            if (uiObject.AllowKeys)
            {
                _uiElement._audio.Play(UIEventTypes.Selected);
            }
        }
        _uiElement = uiObject;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_clickOff == InGame.ReturnToLastSelected)
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                EventSystem.current.SetSelectedGameObject(_uiElement.gameObject);
            }
        }    
    }

    private void OnCancel()
    {
        if (_uiElement.GetComponentInParent<UIBranch>().KillAllOtherUI)
        {
            RestoreAllOtherUI();
        }

        if (_escapeKeyToCancel == EscapeKey.OneLevel)
        {
            OneLevelCancelProcess();
        }

        if (_escapeKeyToCancel == EscapeKey.BackToRootLevel)
        {
            UILeaf temp = RootCancelProcess();
            temp.GetComponentInParent<UIBranch>().MoveToNextLevel();
            temp._audio.Play(UIEventTypes.Cancelled);
        }
    }

    private void OneLevelCancelProcess()
    {
        UILeaf temp = _uiElement.GetComponent<UILeaf>();
        if (temp.MyParentController)
        {
            temp.OnCancel();
        }
    }

    private UILeaf RootCancelProcess()
    {
        foreach (var item in _groupList)
        {
            if (item._uIGroupID == ActiveGroup)
            {
                item._GroupTopLevel.MouseOverLast.RootCancel();
                item._GroupTopLevel.LastSelected.RootCancel();
                return item._GroupTopLevel.LastSelected;
            }
        }
        return null;
    }

    public void KillAllOtherUI()
    {
        foreach (var item in _groupList)
        {
            item._GroupTopLevel.MyCanvas.enabled = false;
        }
    }

    public void RestoreAllOtherUI()
    {
        foreach (var item in _groupList)
        {
            item._GroupTopLevel.MyCanvas.enabled = true;
        }
    }
}
