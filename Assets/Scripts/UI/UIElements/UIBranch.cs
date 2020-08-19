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
    [HideIf(EConditionOperator.Or, "IsAPopUpBranch", "IsHome", "IsPause")] 
    EscapeKey _escapeKeyFunction = EscapeKey.GlobalSetting;
    [SerializeField] 
    [ValidateInput("IsEmpty", "If left Blank it will auto-assign the first UINode in hierarchy/Group")]
    UINode _userDefinedStartPosition;
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
    private bool _noActiveResolvePopUps = true;
    private bool _noPopUps = true;
    protected bool _onHomeScreen = true;
    internal bool _tweenOnChange = true;
    internal bool _setAsActive = true;
    private UIDataEvents _uiDataEvents;
    private UIControlsEvents _uiControlsEvents;
    private UIPopUpEvents _uiPopUpEvents;
    protected internal Canvas _myCanvas;
    protected internal CanvasGroup _myCanvasGroup;
    private IPopUp _popUpBranch;
    private BranchBase _branchBase;
    private bool _activePopUp;
    
    private void SaveNoPopUps(bool noActivePopUps) => _noPopUps = noActivePopUps;
    private void SetResolvePopUpCount(bool activeResolvePopUps) => _noActiveResolvePopUps = activeResolvePopUps;

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
        _uiPopUpEvents = new UIPopUpEvents();
        _uiTweener.OnAwake();
        MyParentBranch = this;
        CreateIfIsAPauseMenu();
        CheckIfResolvePopUp();
        CheckIfOptionalPopUp();
        CheckIfTimedPopUp();
        CheckIfStandardBranch();
        CheckIfHomeScreenBranch();
        SetStartPositions();
    }

    private void OnEnable()
    {
        _uiDataEvents.SubscribeToOnHomeScreen(SaveIfOnHomeScreen);
        _uiDataEvents.SubscribeToHighlightedNode(SaveHighlighted);
        _uiDataEvents.SubscribeToSelectedNode(SaveSelected);
        _uiDataEvents.SubscribeToBackOneLevel(MoveBackToThisBranch);
        _uiPopUpEvents.SubscribeNoResolvePopUps(SetResolvePopUpCount);
        _uiPopUpEvents.SubscribeNoPopUps(SaveNoPopUps);
        _uiControlsEvents.SubscribeSwitchGroups(SwitchBranchGroup);
    }
    
    private void CreateIfIsAPauseMenu()
    {
        if (_branchType != BranchType.PauseMenu) return;
        _branchBase = new PauseMenu(this, FindObjectsOfType<UIBranch>());
        _escapeKeyFunction = EscapeKey.BackOneLevel;
        _tweenOnHome = IsActive.No;
    }

    private void CheckIfResolvePopUp()
    {
        if (!IsResolvePopUp) return;
        _popUpBranch = new UIPopUp(this, FindObjectsOfType<UIBranch>());
        _escapeKeyFunction = EscapeKey.BackOneLevel;
        _tweenOnHome = IsActive.No;
    }
    
    private void CheckIfOptionalPopUp()
    {
        if (!IsOptionalPopUp) return;
        _popUpBranch = new UIPopUp(this, FindObjectsOfType<UIBranch>());
        _screenType = ScreenType.Normal;
        _escapeKeyFunction = EscapeKey.BackOneLevel;
        _tweenOnHome = IsActive.No;
    }
    
    private void CheckIfTimedPopUp()
    {
        if (!IsTimedPopUp) return;
        _popUpBranch = new Timed(this);
        _screenType = ScreenType.Normal;
        _escapeKeyFunction = EscapeKey.BackOneLevel;
        _tweenOnHome = IsActive.No;
    }
    
    private void CheckIfStandardBranch()
    {
        if (_branchType != BranchType.Standard) return;
        _branchBase = new StandardBranchBase(this);
        _tweenOnHome = IsActive.No;
    }
    
    private void CheckIfHomeScreenBranch()
    {
        if (_branchType != BranchType.HomeScreen) return;
        _branchBase = new HomeScreenBranchBase(this);
        _screenType = ScreenType.Normal;
        _escapeKeyFunction = EscapeKey.None;
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

    public void MoveToNextPopUp(UIBranch nextBranch) //Todo Fix In PopUp
    {
        _popUpBranch.MoveToNextPopUp(nextBranch);
        //TODO RestoreBranches
    }

    public void MoveToBranchFromPopUp() // TODO Fix in PopUp
    {
        if (IsOptionalPopUp && !_noActiveResolvePopUps)
        {
            MoveToThisBranchDontSetAsActive();
        }
        else
        {
            MoveToThisBranch();
        }
    }

    public void MoveToThisBranchDontSetAsActive()
    {
        _setAsActive = false;
        MoveToThisBranch();
    }

    public void MoveToThisBranch(UIBranch newParentController = null)
    {
            _branchBase.BasicSetUp(newParentController);
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

    // private void BasicSetUp(UIBranch newParentController = null)
    // {
    //     ActivateBranch();
    //     ClearOrRestoreHomeScreen();
    //     
    //     if (_saveExitSelection == IsActive.No)
    //     {
    //         _groupIndex = UIBranchGroups.SetGroupIndex(DefaultStartPosition, _groupsList);
    //         LastHighlighted = DefaultStartPosition;
    //     }
    //     SetNewParentBranch(newParentController);
    // }

    // public void SetNewParentBranch(UIBranch newParentController) 
    // {
    //     if(newParentController is null) return;
    //     
    //     if (!newParentController.IsAPopUpBranch()) 
    //         MyParentBranch = newParentController;
    // }

    // private void ClearOrRestoreHomeScreen()
    // {
    //     if (_branch != null)
    //     {
    //         _branch.CanClearOrRestoreScreen();
    //     }
    // }
    
    // private void ClearBranchForNavigation(UIBranch ignoreThisBranch)
    // {
    //         if (_branch != null) //Remove
    //        {
    //             _branch.ClearBranch(ignoreThisBranch);
    //        }
    //        else //Remove once PopUp do
    //        {
    //            // if (ignoreThisBranch == this || !CanvasIsEnabled) return;
    //            // _myCanvas.enabled = false;
    //            // _myCanvasGroup.blocksRaycasts = false;
    //        }
    //
    // }
    
    public void ActivateBranch()
    {
        if (_branchBase != null)
        {
            _branchBase.ActivateBranch();
        }
        else
        {
            // if (_noActiveResolvePopUps)
            //     _myCanvasGroup.blocksRaycasts = true;
            //
            // _myCanvas.enabled = true;
        }
    }

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

    /// <summary>
    /// Call To to start any PopUps
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public void StartPopUp() => _popUpBranch?.StartPopUp();
}

