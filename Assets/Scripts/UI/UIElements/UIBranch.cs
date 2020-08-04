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

public partial class UIBranch : MonoBehaviour, IHUbData
{
    [Header("Main Settings")]
    [HorizontalLine(4, color: EColor.Blue, order = 1)]
    [SerializeField] BranchType _branchType = BranchType.StandardUI;
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
    // ReSharper disable once IdentifierTypo
    private UITweener _uiTweener;
    int _groupIndex;
    Action _onFinishedTrigger;
    private IGameToMenuSwitching _gameToMenuSwitching;
    private bool _noActiveResolvePopUps = true;
    private bool _onHomeScreen = true;

    public bool GameIsPaused { get; private set; }

    public IPopUp PopUpBranch { get; private set; }
    private void SetResolveCount(bool activeResolvePopUps) => _noActiveResolvePopUps = activeResolvePopUps;

    public void IsGamePaused(bool paused) => GameIsPaused = paused;

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
        MyCanvasGroup = GetComponent<CanvasGroup>();
        _uiTweener = GetComponent<UITweener>();
        MyCanvas = GetComponent<Canvas>();
        _uiTweener.OnAwake();
        SetNewParentBranch(this);
        SetStartPositions();
        CheckIfPause();
        CheckIfResolvePopUp();
        CheckIfNonResolvePopUp();
        CheckIfTimedPopUp();
    }

    private void OnEnable()
    {
        UIHub.GamePaused += IsGamePaused;
        UIHomeGroup.DoOnHomeScreen += SaveIfOnHomeScreen;
        UIBranch.DoActiveBranch += SaveActiveBranch;
        UINode.DoHighlighted += SaveHighlighted;
        UINode.DoSelected += SaveSelected;
        PopUpController.NoResolvePopUps += SetResolveCount;
    }

    private void OnDisable()
    {
        UIHub.GamePaused -= IsGamePaused;
        UIHomeGroup.DoOnHomeScreen -= SaveIfOnHomeScreen;
        UIBranch.DoActiveBranch -= SaveActiveBranch;
        UINode.DoHighlighted -= SaveHighlighted;
        UINode.DoSelected -= SaveSelected;
        PopUpController.NoResolvePopUps -= SetResolveCount;
        PopUpBranch?.OnDisable();
        PauseMenuClass?.OnDisable();
    }

    public void OnAwake(IGameToMenuSwitching gameToMenuSwitching, UIHomeGroup homeGroup)
    {
        _gameToMenuSwitching = gameToMenuSwitching;
        HomeGroup = homeGroup;
    }

    private void Start()
    {
        MyCanvasGroup.blocksRaycasts = false;
        CheckIfHomeScreen();
    }

    private void CheckIfHomeScreen()
    {
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
    }

    private void CheckIfPause()
    {
        if (_branchType != BranchType.PauseMenu) return;
        PauseMenuClass = new PauseMenu(this, FindObjectsOfType<UIBranch>());
        _escapeKeyFunction = EscapeKey.BackOneLevel;
    }

    private void CheckIfResolvePopUp()
    {
        if (!IsResolvePopUp) return;
        PopUpBranch = new Resolve(this, FindObjectsOfType<UIBranch>(), _gameToMenuSwitching);
        _escapeKeyFunction = EscapeKey.BackOneLevel;
    }
    
    private void CheckIfNonResolvePopUp()
    {
        if (!IsNonResolvePopUp) return;
        PopUpBranch = new NonResolve(this, FindObjectsOfType<UIBranch>(), _gameToMenuSwitching);
        _screenType = ScreenType.Normal;
        _escapeKeyFunction = EscapeKey.BackOneLevel;
    }
    
    private void CheckIfTimedPopUp()
    {
        if (!IsTimedPopUp) return;
        PopUpBranch = new Timed(this);
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
            if (childIsANode)
            {
                DefaultStartPosition = childIsANode;
                break;
            }
        }
    }

    public void MoveBackToThisBranch()
    {
        if (StayOn) 
            TweenOnChange = false;
        MyParentBranch.LastSelected.ThisNodeIsSelected();
        MoveToThisBranch();
    }

    public void MoveToBranchWithoutTween()
    {
        TweenOnChange = false;
        MoveToThisBranch();
    }

    public void MoveToThisBranch(UIBranch newParentController = null)
    {
        BasicSetUp(newParentController);

        if (TweenOnChange) //TODO Replace when ActiveBranch is done
        {
            ActivateInTweens();
        }
        else
        {
            InTweenCallback();
        }
        TweenOnChange = true;
    }
    
    public void SetAsActiveBranch()
    {
        DoActiveBranch?.Invoke(this);
    }

    private void BasicSetUp(UIBranch newParentController = null)
    {
        MyCanvas.enabled = true;
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
        if (!newParentController.IsAPopUpBranch()) MyParentBranch = newParentController;
    }

    private void ClearOrRestoreHomeScreen()
    {
        if (MyBranchType == BranchType.HomeScreenUI && !_onHomeScreen)
        {
            TweenOnChange = TweenOnHome;
            HomeGroup.RestoreHomeScreen();
        }

        if (ScreenType != ScreenType.FullScreen) return;
        if (_onHomeScreen && !IsAPopUpBranch() && !IsPauseMenuBranch())
        {
            HomeGroup.ClearHomeScreen(this, _turnOffPopUps);
        }
    }

    public void ResetHomeScreenBranch()
    {
        if (TweenOnHome)
        {
            ActivateInTweens();
        }
        MyCanvas.enabled = true;
    }

    public void SwitchBranchGroup(SwitchType switchType) 
        => _groupIndex = UIBranchGroups.SwitchBranchGroup(_groupsList, _groupIndex, switchType);

    // ReSharper disable once UnusedMember.Global
    public void StartPopUpScreen() => PopUpBranch?.StartPopUp();

    private void SaveHighlighted(UINode newNode)
    {
        if(ActiveBranch != this) return;
        LastHighlighted = SearchThisBranchesNodes(newNode, LastHighlighted);
    }

    private void SaveSelected(UINode newNode) 
        => LastSelected = SearchThisBranchesNodes(newNode, LastSelected);

    private void SaveActiveBranch(UIBranch newBranch) => ActiveBranch = newBranch;

    private void SaveIfOnHomeScreen(bool onHomeScreen) => _onHomeScreen = onHomeScreen;

    private UINode SearchThisBranchesNodes(UINode newNode, UINode defaultNode)
    {
        for (var i = ThisGroupsUiNodes.Length - 1; i >= 0; i--)
        {
            if (ThisGroupsUiNodes[i] != newNode) continue;
            return newNode;
        }
        return defaultNode;
    }

    public bool CheckAndDisableBranchCanvas(ScreenType myBranchScreenType)
    {
        if (MyCanvas.enabled)
        {
            if (myBranchScreenType == ScreenType.FullScreen)
            {
                MyCanvas.enabled = false;
            }

            if (GameIsPaused || !IsResolvePopUp)
            {
                MyCanvasGroup.blocksRaycasts = false;
            }
            return true;
        }
        return false;
    }
}
