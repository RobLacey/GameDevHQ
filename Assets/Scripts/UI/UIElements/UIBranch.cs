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
    [SerializeField] ScreenType _screenType = ScreenType.ToFullScreen;
   // [SerializeField] [Label("Don't IN Tween On Return")] [HideIf("IsIndie")] bool _neverInTweenOnReturn;
    [SerializeField] [HideIf("IsIndie")] bool _dontTurnOff;
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
    ScreenType _parentsScreenType;

    //Properties
    public UINode DefaultStartPosition { get { return _userDefinedStartPosition; }
        set { _userDefinedStartPosition = value; } }
    public Canvas MyCanvas { get; set; }
    public UINode LastHighlighted { get; set; }
    public UINode LastSelected { get; set; }
    public UIBranch MyParentController { get; set; }
    public bool DontSetAsActive { get; set; } = false;
    public UINode[] ThisGroupsUINodes { get { return _childUILeafs; } }
    public bool AllowKeys { get; set; }
    public CanvasGroup MyCanvasGroup { get; set; }
    public EscapeKey EscapeKeySetting { get { return _escapeKeyFunction; } }
    public bool TweenOnChange { get; set; } = true;
   // public bool InTweenOnReturn { get { return _neverInTweenOnReturn; } }
    public BranchType MyBranchType { get { return _branchType; } }
    public MoveNext MoveToNext { get { return _moveType; } }
    public bool DontTurnOff { get { return _dontTurnOff; } } //****Review
    public ScreenType ScreenType { get { return _screenType; } } 


    private void Awake()
    {
        if (_branchType == BranchType.Independent) _escapeKeyFunction = EscapeKey.None;
        _childUILeafs = gameObject.GetComponentsInChildren<UINode>();
        MyCanvasGroup = GetComponent<CanvasGroup>();
        _UITweener = GetComponent<UITweener>();
        _UiMasterController = FindObjectOfType<UIMasterController>();
        MyCanvas = GetComponent<Canvas>();
        SetCurrentBranchAsParent(_screenType, this);
        SetStartPositions();
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

        if (TweenOnChange)
        {
            ActivateINTweens();
        }
        else
        {
            LastHighlighted.InitialiseStartUp();
        }
        TweenOnChange = true;
    }

    private void BasicSetUp(UIBranch newParentController = null)
    {
        MyCanvas.enabled = true;

        if (_parentsScreenType == ScreenType.Normal 
            && _screenType != ScreenType.ToFullScreen) _UiMasterController.RestoreHomeScreen();

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
                SetCurrentBranchAsParent(newParentController.ScreenType, newParentController);
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
    }

    public void SetCurrentBranchAsParent(ScreenType screenType, UIBranch newParentController = null) 
    {
        _parentsScreenType = screenType;
        if (newParentController != null)
        {
            MyParentController = newParentController;
        }
    }

    public void TurnOffOnMoveToChild(UIBranch ignoreBranch = null)
    {
        _UiMasterController.ClearHomeScreen(ignoreBranch);
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

    public void StartOutTweens(bool movingToChild, Action action = null)
    {
        _onFinishedTrigger = action;

        if (movingToChild)
        {
            _moveToChild = !_dontTurnOff;
        }
        else
        {
            _moveToChild = true;
        }
        _UITweener.StopAllCoroutines();
        //MyCanvasGroup.blocksRaycasts = false;
        _UITweener.DeactivateTweens(() => OutTweenCallback());
    }

    private void OutTweenCallback()
    {
        if (_moveToChild)
        {
            MyCanvas.enabled = false;
        }
        _onFinishedTrigger?.Invoke();
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

    public void HotKeyTrigger()
    {
        if (_branchType == BranchType.Independent)
        {
            TurnOffOnMoveToChild(this);
            MoveToNextLevel();
        }
        else
        {
            if (_screenType == ScreenType.ToFullScreen)
            {
                if (MyCanvas.enabled == true) return;
                TurnOffOnMoveToChild(this);
            }
            foreach (var item in MyParentController.ThisGroupsUINodes) //****Check this functions corretcly
            {
                if (item._navigation._childBranch == this)
                {
                    MyParentController.SetLastHighlighted(item);
                    Debug.Log(_UiMasterController.LastSelected.MyBranchController);
                    if (_UiMasterController.LastSelected.MyBranchController.ScreenType == ScreenType.ToFullScreen)
                    {
                        _UiMasterController.LastSelected._navigation._childBranch.MyCanvas.enabled = false;
                        _UiMasterController.LastSelected.Deactivate();
                        _UiMasterController.LastSelected.MyBranchController
                        .MyParentController.LastSelected._navigation._childBranch.StartOutTweens(false, () => item.OnPointerDown());
                    }
                    else
                    {
                        //_UiMasterController.LastSelected._navigation._childBranch.StartOutTweens(false, ()=> item.OnPointerDown());
                        _UiMasterController.LastSelected._navigation._childBranch.StartOutTweens(false, ()=> item.OnPointerDown());
                    }
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
