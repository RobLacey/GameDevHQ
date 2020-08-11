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
    [SerializeField] private BranchType _branchType = BranchType.StandardUI;
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
    private bool _onHomeScreen = true;
    private bool _tweenOnChange = true;
    private bool _setAsActive = true;
    private UIHomeGroup _homeGroup;
    private UIData _uiData;
    private Canvas _myCanvas;
    private CanvasGroup _myCanvasGroup;
    private IPopUp _popUpBranch;

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
        _uiData = new UIData();
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
        _uiData.SubscribeToOnHomeScreen(SaveIfOnHomeScreen);
        _uiData.SubscribeToHighlightedNode(SaveHighlighted);
        _uiData.SubscribeToSelectedNode(SaveSelected);
        _uiData.SubscribeNoResolvePopUps(SetResolvePopUpCount);
    }

    private void OnDisable()
    {
        _uiData.OnDisable();
        _popUpBranch?.OnDisable();
    }

    public void OnAwake(UIHomeGroup homeGroup) => _homeGroup = homeGroup;

    private void Start()
    {
        _myCanvasGroup.blocksRaycasts = false;
        CheckIfHomeScreen();
    }

    private void CheckIfHomeScreen()
    {
        if (_branchType == BranchType.HomeScreenUI)
        {
            _escapeKeyFunction = EscapeKey.None;
            _myCanvas.enabled = true;
        }
        else
        {
            _myCanvas.enabled = false;
            _tweenOnHome = IsActive.Yes;
        }
    }

    private void CreateIfIsAPauseMenu()
    {
        if (_branchType != BranchType.PauseMenu) return;
        new PauseMenu(this, FindObjectsOfType<UIBranch>());
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

    public void MoveBackToThisBranch()
    {
        if (_stayOn == IsActive.Yes) 
            _tweenOnChange = false;
        MyParentBranch.LastSelected.ThisNodeIsSelected();
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

    public void MoveToThisBranchDontSetAsActive()
    {
        _setAsActive = false;
        MoveToThisBranch();
    }

    public void MoveToThisBranch(UIBranch newParentController = null)
    {
        SetAsActiveBranch();
        BasicSetUp(newParentController);

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
    
    public void SetAsActiveBranch() => DoActiveBranch?.Invoke(this);

    private void BasicSetUp(UIBranch newParentController = null)
    {
        _myCanvas.enabled = true;
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
        RestoreHomeScreen();
        ClearHomeScreen();
    }

    private void ClearHomeScreen()
    {
        if (ScreenType != ScreenType.FullScreen) return;
        
        if (_onHomeScreen && !IsAPopUpBranch() && !IsPauseMenuBranch())
            _homeGroup.ClearHomeScreen(this, _turnOffPopUps);
    }

    private void RestoreHomeScreen()
    {
        if (MyBranchType != BranchType.HomeScreenUI || _onHomeScreen) return;
        _tweenOnChange = _tweenOnHome == IsActive.Yes;
        _homeGroup.RestoreHomeScreen();
    }

    public void ResetHomeScreenBranch()
    {
        if (_tweenOnHome == IsActive.Yes)
            ActivateInTweens();
        _myCanvas.enabled = true;
        _myCanvasGroup.blocksRaycasts = true;
    }

    public void ClearBranch()
    {
        _myCanvasGroup.blocksRaycasts = false;
        _myCanvas.enabled = false;
    }
    
    public void ClearBranchForNavigation()
    {
        if (_stayOn == IsActive.No)
            ClearBranch();
    }

    public void ActivateBranch()
    {
        if (_noActiveResolvePopUps)
            _myCanvasGroup.blocksRaycasts = true;
        
        _myCanvas.enabled = true;
    }

    public void SwitchBranchGroup(SwitchType switchType)
    {
        if(_groupsList.Count > 1)
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

    public bool CheckIfActiveAndDisableBranch(ScreenType myBranchScreenType)
    {
        if (!_myCanvas.enabled) return false;
        
        if (myBranchScreenType == ScreenType.FullScreen)
            _myCanvas.enabled = false;

        _myCanvasGroup.blocksRaycasts = false;
        return true;
    }

    /// <summary>
    /// Call To to start any PopUps
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public void StartPopUp() => _popUpBranch?.StartPopUp();
}

