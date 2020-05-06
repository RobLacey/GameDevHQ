using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using NaughtyAttributes;

public class UITrunk : MonoBehaviour, IPointerClickHandler
{
    [Header("Main Settings")] 
    [Label("UI Starting Branch")] [Required("MUST have a starting branch")]
    [SerializeField] UIBranch _uiTopLevel;
    [Label("Clicked Off UI Action")]
    [SerializeField] InGame _clickOff;
    [SerializeField] EscapeKey _escapeKeyFunction;
    [Header("UI Groups")]
    [Label("List of UI Groups")] [Tooltip("Add a group if keyboard/Controller group switching is needed")] 
    [SerializeField] GroupList[] _groupList;
    

    //Variables
    int _groupIndex = 0;
    UIBranch[] _allUIBranches;
    UILeaf _uiElementLastSelected;

    public UILeaf UIElementLastHighlighted { get; set; }
    public UIGroupID ActiveGroup { get; set; }
    enum InGame { ReturnToLastSelected, Disabled, ClearUI }
    enum EscapeKey { BackOneLevel, BackToRootLevel }

    [Serializable]
    public class GroupList
    {
        public UIGroupID _groupIDNumber;
        public UIBranch _groupStartLevel;
    }

    private void Awake()
    {
        _allUIBranches = FindObjectsOfType<UIBranch>();
        foreach (var item in _groupList)
        {
            item._groupStartLevel.MyUIGroup = item._groupIDNumber;
        }
    }

    private void Start()
    {
        _uiElementLastSelected = _uiTopLevel.DefaultStartPosition;
        UIElementLastHighlighted = _uiElementLastSelected;
        _uiTopLevel.MoveToNextLevel(_uiTopLevel);
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
            _groupList[_groupIndex]._groupStartLevel.MoveToNextLevel();
        }
    }

    public void SetLastUIObject(UILeaf uiObject, UIGroupID uIGroupID) 
    {
        if (uIGroupID != ActiveGroup)
        {
            RootCancelProcess();
            ActiveGroup = uIGroupID;
            if (uiObject.AllowKeys)
            {
                _uiElementLastSelected._audio.Play(UIEventTypes.Selected);
            }
        }
        _uiElementLastSelected = uiObject;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_clickOff == InGame.ReturnToLastSelected)
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                EventSystem.current.SetSelectedGameObject(_uiElementLastSelected.gameObject);
                _uiElementLastSelected.SetAsHighlighted();
            }
        }    
    }

    private void OnCancel()
    {
        _uiElementLastSelected.GetComponentInParent<UIBranch>().IsCancelling = true;
        if (_uiElementLastSelected.GetComponentInParent<UIBranch>().KillAllOtherUI)
        {
            RestoreAllOtherUI();
        }

        if (_escapeKeyFunction == EscapeKey.BackOneLevel)
        {
            if (_uiElementLastSelected.MyParentController)
            {
                _uiElementLastSelected.OnCancel();
            }
        }

        if (_escapeKeyFunction == EscapeKey.BackToRootLevel)
        {
            UILeaf temp = RootCancelProcess();
            temp.GetComponentInParent<UIBranch>().MoveBackALevel();
            temp._audio.Play(UIEventTypes.Cancelled);
        }
    }

    private UILeaf RootCancelProcess()
    {
        foreach (var item in _groupList)
        {
            if (item._groupIDNumber == ActiveGroup)
            {
                item._groupStartLevel.LastSelected.RootCancel();
                return item._groupStartLevel.LastSelected;
            }
        }
        return null;
    }

    public void ToFullScreen(UIBranch currentBranch)
    {
        foreach (var item in _allUIBranches)
        {
            if (currentBranch != item)
            {
                item.MyCanvas.enabled = false;
            }
        }
    }

    private void RestoreAllOtherUI()
    {
        foreach (var item in _groupList)
        {
            item._groupStartLevel.MoveBackALevel();
        }
    }
}
