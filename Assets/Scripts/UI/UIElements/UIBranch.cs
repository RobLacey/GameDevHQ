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
    [SerializeField] BranchType _branchType = BranchType.StandardUI;
    [SerializeField] [HideIf("IsHome")] bool _clearHomeScreen;
    [SerializeField] [ShowIf("IsIndie")] bool _restoreHomeScreen;
    [SerializeField] [HideIf("IsIndie")] bool _neverTweenOnReturn;
    [SerializeField] [HideIf(EConditionOperator.Or, "IsHome", "IsIndie")] bool _turnOff_MoveToChild;
    [SerializeField] [Label("Save Selection On Exit")] [HideIf("IsIndie")] bool _saveExitSelection;
    [SerializeField] [Label("Move To Next Branch...")] MoveNext _moveType = MoveNext.OnClick;
    [SerializeField] [HideIf("IsIndie")] EscapeKey _escapeKeyFunction = EscapeKey.GlobalSetting;
    [SerializeField] [ValidateInput("IsEmpty", "If left Blank it will Auto-assign first UINode in hierarchy/Group")] 
    UINode _userDefinedStartPosition;
    [SerializeField] [HideIf("IsIndie")] [Label("Branch Group List (Leave blank if NO groups needed)")] 
    [ReorderableList] List<GroupList> _groupsList;

    //Internal Callses & Editor Scripts
    #region Internal Classes & Editor Scripts
    [Serializable]
    public class GroupList
    {
        public UINode _startNode;
        public UINode[] _nodes;
    }

    private bool IsEmpty(UINode uINode) { return uINode != null; }
    public bool IsStandard() { return _branchType == BranchType.StandardUI; }
    public bool IsHome() { return _branchType == BranchType.HomeScreenUI; }
    public bool IsIndie() { return _branchType == BranchType.Independent; }

    #endregion

    //Variables
    UINode[] _childUILeafs;
    UIMasterController _UiMasterController;
    UITweener _UITweener;
    int _groupIndex = 0;
    bool _moveToChild = false;
    Action _onFinishedTrigger;

    

    //Properties
    public UINode DefaultStartPosition { get { return _userDefinedStartPosition; } 
                                         set { _userDefinedStartPosition = value; } }
    public Canvas MyCanvas { get; set; }
    public UINode LastSelected { get; set; }
    public bool DontSetAsActive { get; set; } = false;
    public UINode[] ThisGroupsUINodes { get { return _childUILeafs; } }
    public bool AllowKeys { get; set; }
    public CanvasGroup MyCanvasGroup { get; set; }
    public EscapeKey EscapeKeySetting { get { return _escapeKeyFunction; } }
    public bool DontAnimateOnChange { get; set; } = false;
    public bool ClearHomeScreen { get { return _clearHomeScreen; } }
    public BranchType MyBranchType { get { return _branchType; } }
    public MoveNext MoveToNext { get { return _moveType;  } }

    private void Awake()
    {
        if (_branchType == BranchType.Independent) _escapeKeyFunction = EscapeKey.None;
        _childUILeafs = gameObject.GetComponentsInChildren<UINode>();
        MyCanvasGroup = GetComponent<CanvasGroup>();
        _UITweener = GetComponent<UITweener>();
        _UiMasterController = FindObjectOfType<UIMasterController>();
        MyCanvas = GetComponent<Canvas>();
        SetCurrentBranchAsParent(this);
        SetStartPositions();

        if (_branchType != BranchType.HomeScreenUI)
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

    private void SetStartPositions()
    {
        SetGroupIndex();

        if (_groupsList.Count != 0 && DefaultStartPosition == null)
        {
            DefaultStartPosition = _groupsList[0]._startNode;
        }
        else if(_groupsList.Count == 0 && DefaultStartPosition == null)
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
        if (DefaultStartPosition && _groupsList.Count > 0)
        {
            int index = 0;
            for (int i = 0; i < _groupsList.Count; i++)
            {
                foreach (var item in _groupsList[i]._nodes)
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
        BasicSetUp();

        if (_neverTweenOnReturn)
        {
            LastSelected.InitialiseStartUp();
        }
        else
        {
            ActivateINTweens();
        }
    }

    public void MoveToNextLevel(UIBranch newParentController = null)
    {
        BasicSetUp(newParentController);

        if (!DontAnimateOnChange)
        {
            ActivateINTweens();
        }
        else
        {
            LastSelected.InitialiseStartUp();
        }
        DontAnimateOnChange = false;
    }

    private void BasicSetUp(UIBranch newParentController = null)
    {
        MyCanvas.enabled = true;
        _UiMasterController.SetLastUIObject(LastSelected);

        if (!_saveExitSelection)
        {
            SetGroupIndex();
            LastSelected.SetNotHighlighted();
            LastSelected = DefaultStartPosition;
        }

        if (_branchType != BranchType.Independent)
        {
            SetCurrentBranchAsParent(newParentController);
        }
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

    public void SetCurrentBranchAsParent(UIBranch newParentController = null) //Needed in case menu is called from different places
    {
        if (newParentController != null)
        {
            foreach (var item in _childUILeafs)
            {
                item.MyParentController = newParentController;
            }
            foreach (var groups in _groupsList)
            {
                foreach (var item in groups._nodes)
                {
                    item.MyParentController = newParentController;
                }
            }
        }
    }

    public void TurnOffOnMoveToChild(bool clearHomeScreen) 
    {
        if (_branchType == BranchType.HomeScreenUI && clearHomeScreen)
        {
            _UiMasterController.ClearHomeScreen();
        }
    }

    public void SetLastSelected(UINode lastSelected)
    {
        _UiMasterController.SetLastUIObject(lastSelected);
        LastSelected = lastSelected;
    }

    public void StartOutTweens(bool toChild, Action action = null) 
    {
        _onFinishedTrigger = action;

        if (toChild)
        {
            _moveToChild = _turnOff_MoveToChild;
        }
        else
        {
            _moveToChild = true;
        }
        _UITweener.StopAllCoroutines();
        MyCanvasGroup.blocksRaycasts = false;
        _UITweener.DeactivateTweens(() => OutTweenCallback());
    }

    private void OutTweenCallback() 
    {
        if (_moveToChild)
        {
            MyCanvas.enabled = false;
        }
        _onFinishedTrigger?.Invoke();
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
        if (!DontSetAsActive) { LastSelected.InitialiseStartUp(); }
        DontSetAsActive = false;
    }

    public void SwitchGroup()
    {
        _groupsList[_groupIndex]._startNode.SetNotHighlighted();
        if (_groupIndex == _groupsList.Count - 1)
        {
            _groupIndex = 0;
        }
        else
        {
            _groupIndex++;
        }
        _groupsList[_groupIndex]._startNode.MoveToNext();
    }

    public void HotKeyTrigger()
    {
        if (_branchType == BranchType.Independent)
        {
            EnterIndieScreen();
        }
        else
        {
            foreach (var item in LastSelected.MyParentController.ThisGroupsUINodes)
            {
                if(item._navigation._childBranch == this)
                {
                    if (_clearHomeScreen)
                    {
                        _UiMasterController.ClearHomeScreen();
                    }
                    item.MyBranchController.LastSelected.SetNotHighlighted();
                    item.MyBranchController.SetLastSelected(item);
                    item.HotKeyActivation();
                }
            }
        }
    }

    [Button]
    public void EnterIndieScreen()
    {
        if (_clearHomeScreen)
        {
            _UiMasterController.ClearHomeScreen();
        }
        MoveToNextLevel();
    }

    public void LeaveIndieScreen(UIBranch goingTo)
    {
        RestoreHomeScreen(goingTo);
    }

    public void RestoreHomeScreen(UIBranch goingTo)
    {
        if (_UiMasterController.IsItPartOfRootMenu(goingTo))
        {
            _UiMasterController.RestoreHomeWithChecks();
        }
    }
}
