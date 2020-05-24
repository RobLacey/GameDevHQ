using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(GraphicRaycaster))]
[RequireComponent(typeof(UITweener))]

public class UIBranch : MonoBehaviour
{
    [Header("Main Settings")]
    [HorizontalLine(4, color: EColor.Blue, order = 1)]
    [SerializeField] [Label("Home Screen Object")] bool _onHomeScreen;
    [SerializeField] [HideIf("_onHomeScreen")] bool _clearHomeScreen;
    [SerializeField] bool _neverTweenOnReturn;
    [SerializeField] [HideIf("_onHomeScreen")] bool _turnOff_MoveToChild;
    [SerializeField] [Label("Save Selection On Exit")] bool _saveExitSelection;
    [SerializeField] EscapeKey _escapeKeyFunction = EscapeKey.GlobalSetting;
    [SerializeField] [ValidateInput("IsEmpty", "If left Blank it will Auto-assign first UINode in hierarchy/Group")] 
    UINode _userDefinedStartPosition;
    [SerializeField] [Label("Branch Group List (Leave blank if NO groups needed)")] [ReorderableList] List<GroupList> _groupList;

    //Internal Callses & Editor Scripts
    #region Internal Classes & Editor Scripts
    [Serializable]
    public class GroupList
    {
        public UINode _startNode;
        public UINode[] _nodes;
    }

    private bool IsEmpty(UINode uINode) { return uINode != null; }

    #endregion

    //Variables
    UINode[] _childUILeafs;
    UIMasterController _UITrunk;
    UITweener _UITweener;
    int _groupIndex = 0;

    //Properties
    public UINode DefaultStartPosition { get { return _userDefinedStartPosition; } 
                                         set { _userDefinedStartPosition = value; } }
    public Canvas MyCanvas { get; set; }
    public UINode LastSelected { get; set; }
    public bool DontSetAsActive { get; set; } = false;
    public UINode[] ThisGroupsUILeafs { get { return _childUILeafs; } }
    public bool AllowKeys { get; set; }
    public CanvasGroup MyCanvasGroup { get; set; }
    public EscapeKey EscapeKeySetting { get { return _escapeKeyFunction; } }
    public bool DontAnimateOnChange { get; set; } = false;
    public bool ClearHomeScreen { get { return _clearHomeScreen; } }

    private void Awake()
    {
        GetChildUILeafs();
        MyCanvasGroup = GetComponent<CanvasGroup>();
        _UITweener = GetComponent<UITweener>();
        _UITrunk = FindObjectOfType<UIMasterController>();
        MyCanvas = GetComponent<Canvas>();
        SetCurrentBranchAsParent(this);
        SetStartPositions();
        if (!_onHomeScreen)
        {
            MyCanvas.enabled = false;
        }
        else
        {
            MyCanvas.enabled = true;
        }
        _UITweener.OnAwake(MyCanvasGroup);
        MyCanvasGroup.blocksRaycasts = false;
        _UITweener.IsRunning = true;
    }

    private void OnEnable() { _UITweener.IsRunning = true; }
    private void OnDisable() { _UITweener.IsRunning = false; }

    private void GetChildUILeafs() //Only gets Childrenn directly below. Ingnore ones inside other game objects
    {
        List<UINode> temp = new List<UINode>();
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).GetComponent<UINode>())
            {
                temp.Add(transform.GetChild(i).GetComponent<UINode>());
            }
        }
        _childUILeafs = temp.ToArray();
    }

    private void SetStartPositions()
    {
        SetGroupIndex();

        if (_groupList.Count != 0 && DefaultStartPosition == null)
        {
            Debug.Log(gameObject);
            DefaultStartPosition = _groupList[0]._startNode;
        }
        else if(_groupList.Count == 0 && DefaultStartPosition == null)
        {
            foreach (Transform item in transform)
            {
                if (item.GetComponent<UINode>())
                {
                    DefaultStartPosition = item.GetComponent<UINode>();
                    break;
                }
            }
        }

        if (DefaultStartPosition == null) Debug.Log("NO Default Position Found : " + gameObject.name);
        LastSelected = DefaultStartPosition;
    }

    private void SetGroupIndex()
    {
        if (DefaultStartPosition && _groupList.Count > 0)
        {
            int index = 0;
            for (int i = 0; i < _groupList.Count; i++)
            {
                foreach (var item in _groupList[i]._nodes)
                {
                    if (item == DefaultStartPosition)
                    {
                        _groupIndex = index;
                        break;
                    }
                }
            index++;
            }
        }
    }

    public void MoveBackALevel()
    {
        MyCanvas.enabled = true;
        _UITrunk.SetLastUIObject(LastSelected);
        SetCurrentBranchAsParent();

        if (_neverTweenOnReturn)
        {
            InitialiseFirstUIElement();
        }
        else
        {
            ActivateINTweens();
        }
    }

    public void MoveToNextLevel(UIBranch newParentController = null)
    {
        MyCanvas.enabled = true;
        _UITrunk.SetLastUIObject(LastSelected);
        SetCurrentBranchAsParent(newParentController);

        if (!DontAnimateOnChange)
        {
            ActivateINTweens();
        }
        else
        {
            InitialiseFirstUIElement();
        }
        DontAnimateOnChange = false;
    }

    public void RestoreFromFullscreen()
    {
        MyCanvas.enabled = true;
        DontSetAsActive = true;
        if (!_neverTweenOnReturn)
        {
            ActivateINTweens();
        }
    }

    private void SetCurrentBranchAsParent(UIBranch newParentController = null) //Needed in case menu is called from different places
    {
        if (newParentController != null)
        {
            foreach (var item in _childUILeafs)
            {
                item.MyParentController = newParentController;
            }
            foreach (var groups in _groupList)
            {
                foreach (var item in groups._nodes)
                {
                    item.MyParentController = newParentController;
                }
            }
        }
    }

    private void InitialiseFirstUIElement()
    {
        if (DefaultStartPosition != null)
        {
            if (!_saveExitSelection)
            {
                SetGroupIndex();
                LastSelected.SetNotHighlighted();
                LastSelected = DefaultStartPosition;
            }
            LastSelected.InitialiseStartUp();
        }
    }

    public void TurnOffOnMoveToChild(bool clearHomeScreen) 
    {
        if (_onHomeScreen && clearHomeScreen)
        {
            _UITrunk.ClearHomeScreen();
        }

        if (_turnOff_MoveToChild)
        {
            MyCanvas.enabled = false;
        }
    }

    public void SaveLastSelected(UINode lastSelected)
    {
        _UITrunk.SetLastUIObject(lastSelected);
        LastSelected = lastSelected;
    }

    public void StartOutTweens()
    {
        _UITweener.StopAllCoroutines();
        MyCanvasGroup.blocksRaycasts = false;
        _UITweener.DeactivateTweens(() => OutTweenCallback());
    }

    private void OutTweenCallback() 
    {
        if (!_onHomeScreen)
        {
            MyCanvas.enabled = false;
        }
        MyCanvasGroup.blocksRaycasts = true;
    }

    public void ActivateINTweens()
    {
        MyCanvasGroup.blocksRaycasts = false;
        _UITweener.ActivateTweens(()=> InTweenCallback());
    }

    private void InTweenCallback()
    {
        MyCanvasGroup.blocksRaycasts = true;
        if (!DontSetAsActive) { InitialiseFirstUIElement(); }
        DontSetAsActive = false;
    }

    public void SwitchGroup()
    {
        _groupList[_groupIndex]._startNode.SetNotHighlighted();
        if (_groupIndex == _groupList.Count - 1)
        {
            _groupIndex = 0;
        }
        else
        {
            _groupIndex++;
        }
        _groupList[_groupIndex]._startNode.MoveToNext();
    }
}
