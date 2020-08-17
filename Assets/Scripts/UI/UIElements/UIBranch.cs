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
    [SerializeField] private BranchType _branchType = BranchType.Standard;
    [SerializeField] [ShowIf("IsTimedPopUp")] float _timer = 1f;
    [SerializeField] [HideIf(EConditionOperator.Or, "IsNonResolvePopUp", "IsTimedPopUp")] 
    ScreenType _screenType = ScreenType.FullScreen;
    [SerializeField] [ShowIf("TurnOffPopUps")] IsActive _turnOffPopUps = IsActive.No;
    [SerializeField] [HideIf("IsAPopUpBranch")] IsActive _stayOn = IsActive.No;
    [SerializeField] [ShowIf("IsHome")] [Label("Tween on Return To Home")] IsActive _tweenOnHome = IsActive.No;
    [SerializeField] [Label("Save Position On Exit")] [HideIf("IsAPopUpBranch")] IsActive _saveExitSelection = IsActive.Yes;
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
    [ReorderableList] List<GroupList> _groupsList;
    [SerializeField] BranchEvents _branchEvents;

    //Variables
    private UITweener _uiTweener;
    int _groupIndex;
    Action _onFinishedTrigger;
    private bool _noActiveResolvePopUps = true;
    private bool _noPopUps = true;
    private bool _onHomeScreen = true;
    private bool _tweenOnChange = true;
    private bool _setAsActive = true;
    private UIDataEvents _uiDataEvents;
    private UIControlsEvents _uiControlsEvents;
    private UIPopUpEvents _uiPopUpEvents;
    private Canvas _myCanvas;
    private CanvasGroup _myCanvasGroup;
    private IPopUp _popUpBranch;
    private bool _activePopUp;

    private void SaveNoPopUps(bool noActivePopUps) => _noPopUps = noActivePopUps;
    private void SetResolvePopUpCount(bool activeResolvePopUps) => _noActiveResolvePopUps = activeResolvePopUps;

    private void SaveIfOnHomeScreen(bool onHomeScreen)
    {
        _onHomeScreen = onHomeScreen;
        if (_onHomeScreen && (_branchType == BranchType.HomeScreen || _activePopUp))
        {
            ResetHomeScreenBranch();
        }
        else
        {
            ClearBranch();
        }
    }

    private void SaveHighlighted(UINode newNode)
        => LastHighlighted = SearchThisBranchesNodes(newNode, LastHighlighted);
    private void SaveSelected(UINode newNode) 
        => LastSelected = SearchThisBranchesNodes(newNode, LastSelected);

    //Delegates
    public static event Action<UIBranch> DoActiveBranch;
    public static event Action<bool> SetIsOnHomeScreen; // Subscribe To track if on Home Screen

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
        SetNewParentBranch(this);
        SetStartPositions();
        CreateIfIsAPauseMenu();
        CheckIfResolvePopUp();
        CheckIfNonResolvePopUp();
        CheckIfTimedPopUp();
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

    private void Start()
    {
        _myCanvasGroup.blocksRaycasts = false;
        //SetIsOnHomeScreen?.Invoke(true);
        CheckIfHomeScreen();
    }

    private void CheckIfHomeScreen()
    {
        if (_branchType == BranchType.HomeScreen)
        {
            _escapeKeyFunction = EscapeKey.None;
            _myCanvas.enabled = true;
        }
        else
        {
            _myCanvas.enabled = false;
            if(!IsAPopUpBranch()) _tweenOnHome = IsActive.Yes;
        }
    }

    private void CreateIfIsAPauseMenu()
    {
        if (_branchType != BranchType.PauseMenu) return;
        var unused = new PauseMenu(this, FindObjectsOfType<UIBranch>());
        _escapeKeyFunction = EscapeKey.BackOneLevel;
    }

    private void CheckIfResolvePopUp()
    {
        if (!IsResolvePopUp) return;
        _popUpBranch = new UIPopUp(this, FindObjectsOfType<UIBranch>());
        _escapeKeyFunction = EscapeKey.BackOneLevel;
    }
    
    private void CheckIfNonResolvePopUp()
    {
        if (!IsOptionalPopUp) return;
        _popUpBranch = new UIPopUp(this, FindObjectsOfType<UIBranch>());
        _screenType = ScreenType.Normal;
        _escapeKeyFunction = EscapeKey.BackOneLevel;
    }
    
    private void CheckIfTimedPopUp()
    {
        if (!IsTimedPopUp) return;
        _popUpBranch = new Timed(this);
        _screenType = ScreenType.Normal;
        _escapeKeyFunction = EscapeKey.BackOneLevel;
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
        if (_stayOn == IsActive.Yes) 
            _tweenOnChange = false;
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

    public void MoveToNextPopUp(UIBranch nextBranch)
    {
        _popUpBranch.MoveToNextPopUp(nextBranch);
    }

    public void MoveToBranchFromPopUp()
    {
        if (IsOptionalPopUp && !_noActiveResolvePopUps)
        {
            MoveToThisBranchDontSetAsActive();
        }
        MoveToThisBranch();
    }

    public void MoveToThisBranchDontSetAsActive()
    {
        _setAsActive = false;
        MoveToThisBranch();
    }

    public void MoveToThisBranch(UIBranch newParentController = null)
    {
        BasicSetUp(newParentController);
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
        IgnoreThisBranch = false;
    }

    private void SetAsActiveBranch() => DoActiveBranch?.Invoke(this);

    private void BasicSetUp(UIBranch newParentController = null)
    {
        _myCanvas.enabled = true;
        IgnoreThisBranch = true;
        ClearOrRestoreHomeScreen();
        
        if (_saveExitSelection == IsActive.No)
        {
            _groupIndex = UIBranchGroups.SetGroupIndex(DefaultStartPosition, _groupsList);
            LastHighlighted = DefaultStartPosition;
        }
        SetNewParentBranch(newParentController);
    }

    public void SetNewParentBranch(UIBranch newParentController) 
    {
        if(newParentController is null) return;
        
        if (!newParentController.IsAPopUpBranch()) 
            MyParentBranch = newParentController;
    }

    private void ClearOrRestoreHomeScreen()
    {
        if (MyBranchType == BranchType.HomeScreen)
        {
            RestoreHomeScreen();
        }
        else
        {
            ClearHomeScreen();
        }
    }

    private void RestoreHomeScreen()
    {
        if (!_onHomeScreen)
        {
            SetIsOnHomeScreen?.Invoke(true);
            _tweenOnChange = _tweenOnHome == IsActive.Yes;
        }
    }

    private void ClearHomeScreen()
    {
        if (!CanvasIsEnabled || !_onHomeScreen) return;
        
        if (CanClearScreen())
        {
            SetIsOnHomeScreen?.Invoke(false);
        }
    }

    private bool CanClearScreen() //TODO Make Pause and PopUps work
    {
        return ScreenType == ScreenType.FullScreen;
    }

    private void ClearBranch()
    {
        //if (_stayOn == IsActive.Yes) return; //TODO Add This back
        if(!CanvasIsEnabled || IgnoreThisBranch) return;
        if (IsAPopUpBranch() && CanvasIsEnabled) 
            _activePopUp = true;
        //if(IsOptionalPopUp && !TurnOffPopUPs) return;
        
        _myCanvasGroup.blocksRaycasts = false;
        _myCanvas.enabled = false;
    }

    private void ResetHomeScreenBranch()
    {
        if (_tweenOnHome == IsActive.Yes)
            ActivateInTweens();
        _myCanvas.enabled = true;
        _myCanvasGroup.blocksRaycasts = true;
        _activePopUp = false;
    }

    // public void ClearBranchForNavigation()
    // {
    //     if (_stayOn == IsActive.No)
    //         ClearBranch();
    // }
    
    public bool ClearActiveBranches(UIBranch branch, ScreenType screenType)
    {

        if (branch == this || !CanvasIsEnabled) return false;
        CheckIfActiveAndDisableBranch(screenType);
        return true;
    }

    private void CheckIfActiveAndDisableBranch(ScreenType myBranchScreenType)
    {
        //if (!CanvasIsEnabled) return;
        if (myBranchScreenType == ScreenType.FullScreen)
            _myCanvas.enabled = false;
        _myCanvasGroup.blocksRaycasts = false;
    }

    public void ActivateBranch()
    {
        if (_noActiveResolvePopUps)
            _myCanvasGroup.blocksRaycasts = true;
        
        _myCanvas.enabled = true;
    }

    private void SwitchBranchGroup(SwitchType switchType)
    {
        if (_onHomeScreen || !_myCanvas.enabled) return;
        if (_groupsList.Count > 1)
            _groupIndex = UIBranchGroups.SwitchBranchGroup(_groupsList, _groupIndex, switchType);
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

