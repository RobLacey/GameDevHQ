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
    [SerializeField] [Label("Don't IN Tween On Return")] [HideIf("IsIndie")] bool _neverInTweenOnReturn;
    [SerializeField] [HideIf(EConditionOperator.Or, "IsHome", "IsIndie")] bool _turnOff_MoveToChild;
    [SerializeField] [Label("Save Selection On Exit")] [HideIf("IsIndie")] bool _saveExitSelection;
    [SerializeField] [Label("Move To Next Branch...")] MoveNext _moveType = MoveNext.OnClick;
    [SerializeField] [HideIf(EConditionOperator.Or, "IsIndie", "IsHome")] EscapeKey _escapeKeyFunction = EscapeKey.GlobalSetting;
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
    public bool IsHome()
    {
        if (_branchType == BranchType.HomeScreenUI)
        {
            _escapeKeyFunction = EscapeKey.None;
            return true;
        }
        return false;
    }
    public bool IsIndie() { return _branchType == BranchType.Independent; }

    #endregion

    //Variables
    UINode[] _childUILeafs;
    UIMasterController _UiMasterController;
    UITweener _UITweener;
    int _groupIndex = 0;
    bool _moveToChild = false;
    Action _onFinishedTrigger;
    List<UIBranch> _clearedBranches = new List<UIBranch>();

    //Properties
    public UINode DefaultStartPosition { get { return _userDefinedStartPosition; }
        set { _userDefinedStartPosition = value; } }
    public Canvas MyCanvas { get; set; }
    public UINode LastHighlighted { get; set; }
    public UINode LastSelected { get; set; }
    public bool DontSetAsActive { get; set; } = false;
    public UINode[] ThisGroupsUINodes { get { return _childUILeafs; } }
    public bool AllowKeys { get; set; }
    public CanvasGroup MyCanvasGroup { get; set; }
    public EscapeKey EscapeKeySetting { get { return _escapeKeyFunction; } }
    public bool TweenOnChange { get; set; } = false;
    public bool TweenOnReturn { get { return _neverInTweenOnReturn; } }
    public BranchType MyBranchType { get { return _branchType; } }
    public MoveNext MoveToNext { get { return _moveType; } }
    public bool TurnOffOnMove { get { return _turnOff_MoveToChild; } } //****Review

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
        else if (_groupsList.Count == 0 && DefaultStartPosition == null)
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
        LastHighlighted = DefaultStartPosition;
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

    public void MoveToNextLevel(UIBranch newParentController = null)
    {
        BasicSetUp(newParentController);

        if (!TweenOnChange)
        {
            ActivateINTweens();
        }
        else
        {
            LastSelected.InitialiseStartUp();
        }
        TweenOnChange = false;
    }

    private void BasicSetUp(UIBranch newParentController = null)
    {
        MyCanvas.enabled = true;

        if (_branchType == BranchType.HomeScreenUI) _UiMasterController.RestoreHomeScreen();

        if (!_saveExitSelection)
        {
            SetGroupIndex();
            LastHighlighted.SetNotHighlighted();
            LastHighlighted = DefaultStartPosition;
        }

        if (_branchType != BranchType.Independent)
        {
            if (newParentController == null) return;
            if (newParentController.MyBranchType != BranchType.Independent)
            {
                SetCurrentBranchAsParent(newParentController);
            }
        }
    }

    public void ResetHomeScreen(UIBranch lastSelected)
    {
        MyCanvas.enabled = true;
        if (lastSelected != this)
        {
            DontSetAsActive = true;
        }

        if (!_neverInTweenOnReturn)
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

    public void TurnOffOnMoveToChild(UIBranch ignoreBranch = null)
    {
        if (LastSelected._navigation._moveType == MoveType.MoveToExternalBranch)//no prop
        {
            _clearedBranches = _UiMasterController.ClearScreen(ignoreBranch);
        }
    }

    public void TurnOnMoveToParent()
    {
        if (LastSelected._navigation._moveType == MoveType.MoveToExternalBranch)//no prop
        {
            _UiMasterController.RestoreScreen(_clearedBranches);
        }
        _clearedBranches = new List<UIBranch>();
    }

    public void SetLastHighlighted(UINode newNode)
    {
        _UiMasterController.SetLastHighlighted(newNode);
        LastHighlighted = newNode;
    }

    public void SetLastSelected(UINode lastSelected)
    {
        _UiMasterController.SetLastSelected(lastSelected);
        LastSelected = lastSelected;
    }

    public void StartOutTweens(bool toChild, Action action = null)
    {
        _onFinishedTrigger = action;

        if (_branchType != BranchType.HomeScreenUI)
        {
            if (toChild)
            {
                _moveToChild = _turnOff_MoveToChild;
            }
            else
            {
                _moveToChild = true;
            }
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
        _UITweener.ActivateTweens(() => InTweenCallback());
    }

    private void InTweenCallback()
    {
        MyCanvasGroup.blocksRaycasts = true;
        if (!DontSetAsActive) { LastHighlighted.InitialiseStartUp(); }
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

    //Will instantly clear on any Fullscreen. No Waiing for out animation
    public void HotKeyTrigger()
    {
        if (_branchType == BranchType.Independent)
        {
            TurnOffOnMoveToChild(this);
            MoveToNextLevel();
        }
        else
        {
            foreach (var item in LastHighlighted.MyParentController.ThisGroupsUINodes) //****Check this functions corretcly
            {
                if (item._navigation._childBranch == this)
                {
                    item.MyBranchController.SetLastHighlighted(item);
                    item.OnPointerDown();
                }
            }
        }
    }

    [Button]
    public void EnterIndieScreen()
    {
        HotKeyTrigger();
    }
}
