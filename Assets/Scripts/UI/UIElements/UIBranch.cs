using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;
using UnityEngine.Events;

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(GraphicRaycaster))]
[RequireComponent(typeof(UITweener))]

public partial class UIBranch : MonoBehaviour
{
    [Header("Main Settings")]
    [HorizontalLine(4, color: EColor.Blue, order = 1)]
    [SerializeField]
    internal BranchType _branchType = BranchType.Standard;
    [SerializeField] [ShowIf("IsTimedPopUp")] float _timer = 1f;
    [SerializeField] [HideIf(EConditionOperator.Or, "IsOptionalPopUp", "IsTimedPopUp", "IsHome")]
    internal ScreenType _screenType = ScreenType.FullScreen;
    [SerializeField] [ShowIf("TurnOffPopUps")] IsActive _turnOffPopUps = IsActive.No;
    [SerializeField] [HideIf("IsAPopUpBranch")]
    internal IsActive _stayOn = IsActive.No;
    [SerializeField] [ShowIf("IsHome")] [Label("Tween on Return To Home")]
    protected internal IsActive _tweenOnHome = IsActive.No;
    [SerializeField] [Label("Save Position On Exit")] [HideIf("IsAPopUpBranch")]
    internal IsActive _saveExitSelection = IsActive.Yes;
    [SerializeField] [Label("Move To Next Branch...")] WhenToMove _moveType = WhenToMove.Immediately;
    [SerializeField] 
    [HideIf(EConditionOperator.Or, "IsAPopUpBranch", "IsHome", "IsPauseMenuBranch")]
    internal EscapeKey _escapeKeyFunction = EscapeKey.GlobalSetting;
    [SerializeField] 
    [ValidateInput("IsEmpty", "If left Blank it will auto-assign the first UINode in hierarchy/Group")]
    UINode _userDefinedStartPosition;
    [SerializeField] [ShowIf("IsPauseMenuBranch")] private UnityEvent _whenPausePressed;
    [SerializeField] 
    [HideIf(EConditionOperator.Or, "IsAPopUpBranch", "IsHome")] 
    [Label("Branch Group List (Leave blank if NO groups needed)")]
    [ReorderableList]
    internal List<GroupList> _groupsList;
    [SerializeField] BranchEvents _branchEvents;

    //Variables
    private UITweener _uiTweener;
    int _groupIndex;
    Action _onFinishedTrigger;
    private bool _onHomeScreen = true;
    internal bool _tweenOnChange = true;
    internal bool _setAsActive = true;
    private UIDataEvents _uiDataEvents;
    private UIControlsEvents _uiControlsEvents;
    protected internal Canvas _myCanvas;
    protected internal CanvasGroup _myCanvasGroup;
    private BranchBase _branch;
    private bool _activePopUp;
    
    private void SaveIfOnHomeScreen(bool onHomeScreen) => _onHomeScreen = onHomeScreen;

    private void SaveHighlighted(UINode newNode)
        => LastHighlighted = SearchThisBranchesNodes(newNode, LastHighlighted);
    private void SaveSelected(UINode newNode) 
        => LastSelected = SearchThisBranchesNodes(newNode, LastSelected);

    //Delegates
    public static event Action<UIBranch> DoActiveBranch;

  //InternalClasses
    [Serializable]
    private class BranchEvents
    {
        public UnityEvent _onBranchEnter;
        public UnityEvent _onBranchExit;
    }

    private void Awake()
    {
        ThisGroupsUiNodes = gameObject.GetComponentsInChildren<UINode>();
        _myCanvasGroup = GetComponent<CanvasGroup>();
        _uiTweener = GetComponent<UITweener>();
        _myCanvas = GetComponent<Canvas>();
        _uiDataEvents = new UIDataEvents();
        _uiControlsEvents = new UIControlsEvents();
        //_uiPopUpEvents = new UIPopUpEvents();
        _uiTweener.OnAwake();
        MyParentBranch = this;
        _branch = BranchFactory.AssignType(this, _branchType, FindObjectsOfType<UIBranch>());
         CheckIfTimedPopUp();
        SetStartPositions();
    }

    private void OnEnable()
    {
        _uiDataEvents.SubscribeToOnHomeScreen(SaveIfOnHomeScreen);
        _uiDataEvents.SubscribeToHighlightedNode(SaveHighlighted);
        _uiDataEvents.SubscribeToSelectedNode(SaveSelected);
        _uiDataEvents.SubscribeToBackOneLevel(MoveBackToThisBranch);
        _uiControlsEvents.SubscribeSwitchGroups(SwitchBranchGroup);
    }
    
    private void CheckIfTimedPopUp()
    {
        if (_branchType != BranchType.TimedPopUp) return;
        //_popUpBranch = new Timed(this);
        _screenType = ScreenType.Normal;
        _escapeKeyFunction = EscapeKey.BackOneLevel;
        _tweenOnHome = IsActive.No;
    }

    private void SetStartPositions()
    {
        _groupIndex = UIBranchGroups.SetGroupIndex(DefaultStartPosition, _groupsList);
        SetDefaultStartPosition();
        LastHighlighted = DefaultStartPosition;
        LastSelected = DefaultStartPosition;
    }

    private void SetDefaultStartPosition()
    {
        if (DefaultStartPosition)return;
        if (_groupsList.Count > 0)
        {
            DefaultStartPosition = _groupsList[0]._startNode;
        }
        else
        {
            FindStartPosition();
        }
    }

    private void FindStartPosition()
    {
        foreach (Transform child in transform)
        {
            var childIsANode = child.GetComponent<UINode>();
            if (!childIsANode) continue;
            DefaultStartPosition = childIsANode;
            break;
        }
    }

    private void MoveBackToThisBranch(UIBranch lastBranch)
    {
        if (lastBranch != this) return;
        // if (_stayOn == IsActive.Yes) 
        //     _tweenOnChange = false;
        LastSelected.SetNotSelected_NoEffects();
        MyParentBranch.LastSelected.ThisNodeIsSelected();
        //if (!_noPopUps) _setAsActive = false;
        MoveToThisBranch();
    }

    public void MoveToBranchWithoutTween()
    {
        _myCanvasGroup.blocksRaycasts = true;
        _tweenOnChange = false;
        MoveToThisBranch();
    }

    public void MoveToNextPopUp(UIBranch nextPopUp) //Todo Fix In PopUp
    {
        _branch.ActivateNextPopUp?.Invoke(nextPopUp);
        //TODO RestoreBranches
    }

    public void MoveToThisBranch(UIBranch newParentController = null)
    {
        _branch.SetUpBranch(newParentController);
        if(_setAsActive) SetAsActiveBranch();

        if (_tweenOnChange)
        {
            ActivateInTweens();
        }
        else
        {
            InTweenCallback();
        }
        _tweenOnChange = true;
    }

    internal void SetAsActiveBranch() => DoActiveBranch?.Invoke(this);
    
    public void ActivateBranch() => _branch.ActivateBranch();

    private void SwitchBranchGroup(SwitchType switchType)
    {
        if (_onHomeScreen || !_myCanvas.enabled) return;
        if (_groupsList.Count > 1)
            _groupIndex = UIBranchGroups.SwitchBranchGroup(_groupsList, _groupIndex, switchType);
    }

    public void ResetBranchStartPosition()
    {
        _groupIndex = UIBranchGroups.SetGroupIndex(DefaultStartPosition, _groupsList);
        LastHighlighted = DefaultStartPosition;
    }

    private UINode SearchThisBranchesNodes(UINode newNode, UINode defaultNode)
    {
        for (var i = ThisGroupsUiNodes.Length - 1; i >= 0; i--)
        {
            if (ThisGroupsUiNodes[i] != newNode) continue;
            return newNode;
        }
        return defaultNode;
    }
    
    public void StartOutTweenProcess(Action endAction = null)
    {
        if (WhenToMove == WhenToMove.AfterEndOfTween)
        {
            StartOutTween(endAction);
        }
        else
        {
            StartOutTween();
            endAction?.Invoke();
        }
    }

    public void StartOutTween(Action action = null)
    {
        _branchEvents?._onBranchExit.Invoke();
        _onFinishedTrigger = action;
        _uiTweener.StopAllCoroutines();
        _myCanvasGroup.blocksRaycasts = false;
        _uiTweener.DeactivateTweens(OutTweenCallback);
    }

    private void OutTweenCallback()
    {
        _myCanvas.enabled = false;
        _onFinishedTrigger?.Invoke();
    }

    protected internal void ActivateInTweens()
    {
        _uiTweener.ActivateTweens(InTweenCallback);
    }
    
    private void InTweenCallback()
    {
        if (_setAsActive)
        {
            LastHighlighted.SetNodeAsActive();
        }

        _branchEvents?._onBranchEnter.Invoke();
        _setAsActive = true;
    }

    /// <summary>
    /// Call To to start any PopUps
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public void StartPopUp() => _branch.OnStartPopUp?.Invoke();
}

