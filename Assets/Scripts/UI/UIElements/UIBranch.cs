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
    [SerializeField] BranchType _branchType = BranchType.StandardUI;
    [SerializeField] [ShowIf("IsTimedPopUp")] float _timer = 1f;
    [SerializeField] ScreenType _screenType = ScreenType.FullScreen;
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
    UITweener _uiTweener;
    int _groupIndex;
    Action _onFinishedTrigger;
    UIHub _uIHub;

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
    }

    public void OnAwake(UIHub uiHub, UIHomeGroup homeGroup)
    {
        _uIHub = uiHub;
        HomeGroup = homeGroup;
    }

    private void Start()
    {
        MyCanvasGroup.blocksRaycasts = false;
        CheckIfHomeScreen();
        CheckIfPause();
        CheckIfPopUp();
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
        PauseMenuClass = new UIPopUp(this, _uIHub.AllBranches, _uIHub);
        _escapeKeyFunction = EscapeKey.BackOneLevel;
    }

    private void CheckIfPopUp()
    {
        if (!IsAPopUpBranch()) return;
        PopUpClass = new UIPopUp(this, _uIHub.AllBranches, _uIHub);
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
    }

    public void MoveToThisBranch(UIBranch newParentController = null)
    {
        BasicSetUp(newParentController);
        
        if (TweenOnChange)
        {
            ActivateInTweens();
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
        ClearOrRestoreHomeScreen();

        if (_saveExitSelection == IsActive.No)
        {
            _groupIndex = UIBranchGroups.SetGroupIndex(DefaultStartPosition, _groupsList);
            LastHighlighted = DefaultStartPosition;
        }
        SetNewParentBranch(newParentController);
    }

    private void ClearOrRestoreHomeScreen()
    {
        if (MyBranchType == BranchType.HomeScreenUI && _uIHub.OnHomeScreen == false)
        {
            TweenOnChange = TweenOnHome;
            HomeGroup.RestoreHomeScreen();
        }

        if (ScreenType != ScreenType.FullScreen) return;
        if (_uIHub.OnHomeScreen && !IsAPopUpBranch() && !IsPause())
        {
            HomeGroup.ClearHomeScreen(this, _turnOffPopUps);
        }
    }

    public void SetNewParentBranch(UIBranch newParentController) 
    {
        if(newParentController is null) return;
        if (!newParentController.IsAPopUpBranch()) MyParentBranch = newParentController;
    }

    public void ResetHomeScreenBranch(UIBranch lastBranch)
    {
        if (TweenOnHome)
        {
            if (lastBranch != this)
            {
                DontSetAsActive = true;
            }
            ActivateInTweens();
        }
        MyCanvas.enabled = true;
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

    public void SwitchBranchGroup(SwitchType switchType)
    {
        _groupIndex = UIBranchGroups.SwitchBranchGroup(_groupsList, _groupIndex, switchType);
    }

    // ReSharper disable once UnusedMember.Global
    public void StartPopUpScreen() => PopUpClass.StartPopUp();
}
