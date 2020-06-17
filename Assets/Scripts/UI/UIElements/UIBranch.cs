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
    [SerializeField] [HideIf("IsIndie")] bool _dontTurnOff;
    [SerializeField] [ShowIf("IsHome")] [Label("Tween on Return To Home")] bool _tweenOnHome;
    [SerializeField] [Label("Save Selection On Exit")] [HideIf("IsIndie")] bool _saveExitSelection;
    [SerializeField] [Label("Move To Next Branch...")] WhenToMove _moveType = WhenToMove.OnClick;
    [SerializeField] [HideIf(EConditionOperator.Or, "IsIndie", "IsHome")] EscapeKey _escapeKeyFunction = EscapeKey.GlobalSetting;
    [SerializeField] [ValidateInput("IsEmpty", "If left Blank it will Auto-assign first UINode in hierarchy/Group")]
    UINode _userDefinedStartPosition;
    [SerializeField] [HideIf("IsIndie")] [Label("Branch Group List (Leave blank if NO groups needed)")]
    [ReorderableList] List<GroupList> _groupsList;

    //Internal Callses & Editor Scripts
    #region Internal Classes & Editor Scripts

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
    UITweener _UITweener;
    int _groupIndex = 0;
    bool _moveToChild = false;
    Action _onFinishedTrigger;
    UIIndependent _isAnIndieBranch;

    //Properties
    public UINode DefaultStartPosition { get { return _userDefinedStartPosition; }
        private set { _userDefinedStartPosition = value; } }
    public Canvas MyCanvas { get; set; }
    public UINode LastHighlighted { get; set; }
    public UINode LastSelected { get; set; }
    public UIBranch MyParentBranch { get; set; }
    public bool DontSetAsActive { get; set; } = false;
    public UINode[] ThisGroupsUINodes { get; private set; }
    public bool AllowKeys { get; set; }
    public CanvasGroup MyCanvasGroup { get; set; }
    public EscapeKey EscapeKeySetting { get { return _escapeKeyFunction; } }
    public bool TweenOnChange { get; set; } = true;
    public BranchType MyBranchType { get { return _branchType; } }
    public WhenToMove WhenToMove { get { return _moveType; } }
    public bool DontTurnOff { get { return _dontTurnOff; } } 
    public bool TweenOnHome { get { return _tweenOnHome; } }
    public bool FromHotkey { get; set; }
    public ScreenType ScreenType { get { return _screenType; } } 
    public UIMasterController UIMaster { get; private set; } 
    public UIIndependent IsAnIndie { get{ return _isAnIndieBranch; } } 


    private void Awake()
    {
        ThisGroupsUINodes = gameObject.GetComponentsInChildren<UINode>();
        MyCanvasGroup = GetComponent<CanvasGroup>();
        _UITweener = GetComponent<UITweener>();
        UIMaster = FindObjectOfType<UIMasterController>();
        MyCanvas = GetComponent<Canvas>();
        SetNewParentBranch(this);
        SetStartPositions();
        _UITweener.OnAwake(MyCanvasGroup);
    }

    private void OnEnable()
    {
        UIMasterController.AllowKeys += (x) => AllowKeys = x;
    }

    private void OnDisable()
    {
        UIMasterController.AllowKeys -= (x) => AllowKeys = x;
    }

    private void Start()
    {
        if (_branchType == BranchType.HomeScreenUI)
        {
            _escapeKeyFunction = EscapeKey.None;
            MyCanvas.enabled = true;
        }
        else
        {
            MyCanvas.enabled = false;
            _tweenOnHome = true;
        }
        MyCanvasGroup.blocksRaycasts = false;
        if (_branchType == BranchType.Independent)
        {
            _isAnIndieBranch = new UIIndependent(this, FindObjectsOfType<UIBranch>());
            _escapeKeyFunction = EscapeKey.None;
        }
    }

    private void SetStartPositions()
    {
        _groupIndex = UIBranchGroups.SetGroupIndex(DefaultStartPosition, _groupsList);

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
        LastHighlighted = DefaultStartPosition;
        LastSelected = DefaultStartPosition;
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
            InTweenCallback();
        }
        TweenOnChange = true;
    }

    private void BasicSetUp(UIBranch newParentController = null)
    {
        MyCanvas.enabled = true;

        if (MyBranchType == BranchType.Independent) UIMaster.ResetHierachy();

        if (MyBranchType == BranchType.HomeScreenUI && UIMaster.OnHomeScreen == false)
        {
            TweenOnChange = TweenOnHome;
            UIHomeGroup.RestoreHomeScreen();
        }

        if (!_saveExitSelection)
        {
            _groupIndex = UIBranchGroups.SetGroupIndex(DefaultStartPosition, _groupsList);
            LastHighlighted.SetNotHighlighted();
            LastHighlighted = DefaultStartPosition;
        }
        SetNewParentBranch(newParentController);
    }

    public void ResetHomeScreenBranch(UIBranch lastSelected)
    {
        if (TweenOnHome)
        {
            if (lastSelected != this)
            {
                DontSetAsActive = true;
            }
            ActivateINTweens();
        }
        MyCanvas.enabled = true;
    }

    public void SetNewParentBranch(UIBranch newParentController) 
    {
        if (newParentController != null && newParentController.MyBranchType != BranchType.Independent)
        {
            MyParentBranch = newParentController;
        }
    }

    public void SaveLastHighlighted(UINode newNode)
    {
        UIMaster.SetLastHighlighted(newNode);
        LastHighlighted = newNode;
    }

    public void SaveLastSelected(UINode lastSelected)
    {
        UIMaster.SetLastSelected(lastSelected);
        LastSelected = lastSelected;
    }

    public void OutTweensToChild(Action action = null)
    {
        _onFinishedTrigger = action;
        _moveToChild = !_dontTurnOff;
        _UITweener.StopAllCoroutines();
         MyCanvasGroup.blocksRaycasts = false;
        _UITweener.DeactivateTweens(() => OutTweenCallback());
    }

    public void OutTweenToParent(Action action = null)
    {
        _onFinishedTrigger = action;
        _moveToChild = true;
        _UITweener.StopAllCoroutines();
         MyCanvasGroup.blocksRaycasts = false;
        _UITweener.DeactivateTweens(() => OutTweenCallback());
    }

    private void OutTweenCallback()
    {
        if (_moveToChild) { MyCanvas.enabled = false; }

        MyCanvasGroup.blocksRaycasts = true;
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

        if (!DontSetAsActive) 
        {
            SaveLastHighlighted(LastHighlighted);
            LastHighlighted.SetUpNodeWhenActive(); 
        }
        DontSetAsActive = false;
    }

    public void SwitchBranchGroup()
    {
        _groupIndex = UIBranchGroups.SwitchBranchGroup(_groupsList, _groupIndex);
    }

    [Button]
    public void EnterIndieScreen()
    {
        _isAnIndieBranch.StartIndie();
    }
}
