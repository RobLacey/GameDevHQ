using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using NaughtyAttributes;

public class UITrunk : MonoBehaviour, IPointerClickHandler
{
    [Header("Main Settings")] [HorizontalLine(4, color: EColor.Blue, order = 1)]
    [Label("UI Starting Level")] [Required("MUST have a starting level")]
    [SerializeField] UIBranch _uiTopLevel;
    [Label("Clicked Off UI Action")]
    [SerializeField] InGame _clickOff;
    [SerializeField] EscapeKey _escapeKeyFunction;
    [Header("UI Groups")] [HorizontalLine(4, color: EColor.Blue, order = 1)]
    [Label("List of UI Groups")] [Tooltip("Add a group if keyboard/Controller group switching is needed")] 
    [SerializeField] GroupList[] _groupList;
    

    //Variables
    UILeaf _uiElement;
    int _groupIndex = 0;
    UIBranch[] _allUIBranches;

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
        _uiElement = _uiTopLevel.DefaultStartPosition;
        _uiTopLevel.MoveToNextLevel();
        _uiTopLevel.ActivateEffects();
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
            _groupList[_groupIndex]._groupStartLevel.MoveToNextLevel();
        }
    }

    public void SetLastUIObject(UILeaf uiObject, UIGroupID uIGroupID) 
    {
        if (uIGroupID != ActiveGroup)
        {
            Debug.Log("Group Changed");
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

        if (_escapeKeyFunction == EscapeKey.BackOneLevel)
        {
            OneLevelCancelProcess();
        }

        if (_escapeKeyFunction == EscapeKey.BackToRootLevel)
        {
            UILeaf temp = RootCancelProcess();
            Debug.Log(temp);
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
            if (item._groupIDNumber == ActiveGroup)
            {
                item._groupStartLevel.MouseOverLast.SetNotHighlighted();
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

    public void RestoreAllOtherUI()
    {
        foreach (var item in _groupList)
        {
            item._groupStartLevel.MyCanvas.enabled = true;
        }
    }
}
