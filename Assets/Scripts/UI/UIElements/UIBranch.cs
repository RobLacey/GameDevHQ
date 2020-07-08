using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;
using UnityEngine.Events;
using System.Linq;
using UnityEngine.Serialization;

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
    [SerializeField] [ShowIf("IsTimedPopUp")] float _timer = 1f;
    [SerializeField] ScreenType _screenType = ScreenType.ToFullScreen;
    [SerializeField] [ShowIf("TurnOffPopUps")] IsActive _turnOffPopUps = IsActive.No;
    [SerializeField] [HideIf("IsAPopUpBranch")] IsActive _stayOn = IsActive.No;
    [SerializeField] [ShowIf("IsHome")] [Label("Tween on Return To Home")] IsActive _tweenOnHome = IsActive.No;
    [SerializeField] [Label("Save Selection On Exit")] [HideIf("IsIndie")] IsActive _saveExitSelection = IsActive.Yes;
    //[SerializeField] Bool _highlightFirstOption = Bool.Yes;
    [SerializeField] [Label("Move To Next Branch...")] WhenToMove _moveType = WhenToMove.Immediately;
    [SerializeField] 
    [HideIf(EConditionOperator.Or, "IsAPopUpBranch", "IsHome", "IsPause")] 
    EscapeKey _escapeKeyFunction = EscapeKey.GlobalSetting;
    [SerializeField] [ValidateInput("IsEmpty", "If left Blank it will Auto-assign first UINode in hierarchy/Group")]
    UINode _userDefinedStartPosition;
    [SerializeField] [HideIf("IsAPopUpBranch")] [Label("Branch Group List (Leave blank if NO groups needed)")]
    [ReorderableList] List<GroupList> _groupsList;
    [SerializeField] BranchEvents _branchEvents;

    //Editor Scripts
    #region Editor Scripts

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

    private bool TurnOffPopUps => _screenType == ScreenType.ToFullScreen && !IsNonResolvePopUp;

    #endregion

    //Variables
    UITweener _UITweener;
    int _groupIndex = 0;
    Action _onFinishedTrigger;
    UIHub _uIHub;

  //InternalClasses
    [Serializable]
    private class BranchEvents
    {
        public UnityEvent OnBranchEnabled;
        public UnityEvent OnBranchDisabled;
    }

    //Properties
    public bool IsAPopUpBranch() { return _branchType == BranchType.PopUp_NonResolve 
                                           || _branchType == BranchType.PopUp_Resolve
                                           || _branchType == BranchType.PopUp_Timed; }
    public bool IsPause() { return _branchType == BranchType.PauseMenu; }
    public UINode DefaultStartPosition { get { return _userDefinedStartPosition; }
        private set { _userDefinedStartPosition = value; } }
    public Canvas MyCanvas { get; private set; }
    public UINode LastHighlighted { get; private set; }
    public UINode LastSelected { get; private set; }
    public UIBranch MyParentBranch { get; private set; }
    public bool DontSetAsActive { get; set; } = false;
    public UINode[] ThisGroupsUINodes { get; private set; }
    //public bool AllowKeys { get; private set; }
    public CanvasGroup MyCanvasGroup { get; private set; }
    public EscapeKey EscapeKeySetting { get { return _escapeKeyFunction; } }
    public bool TweenOnChange { get; set; } = true;
    public BranchType MyBranchType { get { return _branchType; } }
    public WhenToMove WhenToMove { get { return _moveType; } }
    public bool StayOn { get { return _stayOn == IsActive.Yes; } }
    private bool TweenOnHome => _tweenOnHome == IsActive.Yes;
    public bool FromHotkey { get; set; }
    public bool IsResolvePopUp { get { return _branchType == BranchType.PopUp_Resolve; } }
    public bool IsNonResolvePopUp { get { return _branchType == BranchType.PopUp_NonResolve; } }
    public bool IsTimedPopUp { get { return _branchType == BranchType.PopUp_Timed; } }
    public ScreenType ScreenType { get { return _screenType; } } 
    public UIPopUp PopUpClass { get; private set; } 
    public UIPopUp PauseMenuClass { get; private set; }
    public int GroupListCount { get { return _groupsList.Count; } }
    public float Timer { get { return _timer; } }
    public UIHomeGroup HomeGroup { get; private set; }
    public bool AllowKeys { get; set; } = false;


    private void Awake()
    {
        ThisGroupsUINodes = gameObject.GetComponentsInChildren<UINode>();
        MyCanvasGroup = GetComponent<CanvasGroup>();
        _UITweener = GetComponent<UITweener>();
        MyCanvas = GetComponent<Canvas>();
        _UITweener.OnAwake(MyCanvasGroup);
        SetNewParentBranch(this);
    }

    public void OnAwake(UIHub uiHub, UIHomeGroup homeGroup)
    {
        _uIHub = uiHub;
        HomeGroup = homeGroup;
    }

    private void Start()
    {
        SetStartPositions();

        if (_branchType == BranchType.HomeScreenUI)
        {
            _escapeKeyFunction = EscapeKey.None;
            MyCanvas.enabled = true;
        }
        else
        {
            MyCanvas.enabled = false;
            _tweenOnHome = IsActive.Yes;
        }
        MyCanvasGroup.blocksRaycasts = false;

        if (_branchType == BranchType.PauseMenu)
        {
            PauseMenuClass = new UIPopUp(this, _uIHub.AllBranches, _uIHub);
            _escapeKeyFunction = EscapeKey.BackOneLevel;
        }

        if (IsAPopUpBranch())
        {
            PopUpClass = new UIPopUp(this, _uIHub.AllBranches, _uIHub);
           _escapeKeyFunction = EscapeKey.BackOneLevel;
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

        if (MyBranchType == BranchType.HomeScreenUI && _uIHub.OnHomeScreen == false)
        {
            TweenOnChange = TweenOnHome;
            HomeGroup.RestoreHomeScreen();
        }

        if (ScreenType == ScreenType.ToFullScreen)
        {
            if (_uIHub.OnHomeScreen && !IsAPopUpBranch() && !IsPause())
            {
                HomeGroup.ClearHomeScreen(this, _turnOffPopUps);
            }
            // else if(_uIHub.ActivePopUpsNonResolve.Count > 0)
            // {
            //     foreach (var uiBranch in _uIHub.ActivePopUpsNonResolve)
            //     {
            //         uiBranch.MyCanvas.enabled = false;
            //     }
            // }
        }

        if (_saveExitSelection == IsActive.Yes)
        {
            _groupIndex = UIBranchGroups.SetGroupIndex(DefaultStartPosition, _groupsList);
            LastHighlighted = DefaultStartPosition;
        }
        SetNewParentBranch(newParentController);
    }

    public void ResetHomeScreenBranch(UIBranch lastBranch)
    {
        if (TweenOnHome)
        {
            if (lastBranch != this)
            {
                DontSetAsActive = true;
            }
            ActivateINTweens();
        }
        MyCanvas.enabled = true;
    }

    public void SetNewParentBranch(UIBranch newParentController) 
    {
        if (newParentController != null && !newParentController.IsAPopUpBranch())
        {
            MyParentBranch = newParentController;
        }
    }

    public void SaveLastHighlighted(UINode newNode)
    {
        _uIHub.SetLastHighlighted(newNode);
        LastHighlighted = newNode;
    }

    public void SaveLastSelected(UINode lastSelected)
    {
        _uIHub.SetLastSelected(lastSelected);
        LastSelected = lastSelected;
    }

    public void StartOutTween(Action action = null)
    {
        _branchEvents?.OnBranchDisabled.Invoke();
        _onFinishedTrigger = action;
        _UITweener.StopAllCoroutines();
         MyCanvasGroup.blocksRaycasts = false;
        _UITweener.DeactivateTweens(() => OutTweenCallback());
    }

    private void OutTweenCallback()
    {
        MyCanvas.enabled = false;
        _onFinishedTrigger?.Invoke();
    }

    public void ActivateINTweens()
    {
        MyCanvasGroup.blocksRaycasts = false;
        _UITweener.ActivateTweens(() => InTweenCallback());
    }

    private void InTweenCallback()
    {
        if (!IsAPopUpBranch() && _uIHub.CanStart) MyCanvasGroup.blocksRaycasts = true;
        if (IsAPopUpBranch()) PopUpClass.ManagePopUpResolve();

        if (!DontSetAsActive) 
        {
            SaveLastHighlighted(LastHighlighted);
            LastHighlighted.SetNodeAsActive(); 
        }
        _branchEvents?.OnBranchEnabled.Invoke();        
        DontSetAsActive = false;
    }

    public void SwitchBranchGroup()
    {
        _groupIndex = UIBranchGroups.SwitchBranchGroup(_groupsList, _groupIndex);
    }

    public void StartPopUpScreen()
    {
        PopUpClass.StartPopUp();
    }
}
