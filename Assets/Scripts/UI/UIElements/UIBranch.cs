using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;
using UnityEngine.Events;

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(GraphicRaycaster))]
[RequireComponent(typeof(UITweener))]

public partial class UIBranch : MonoBehaviour, IStartPopUp
{
    [SerializeField]
    private BranchType _branchType = BranchType.Standard;
    [SerializeField] 
    [ShowIf("IsTimedPopUp")] private float _timer = 1f;
    [SerializeField] 
    [HideIf(EConditionOperator.Or, "IsOptionalPopUp", "IsTimedPopUp", "IsHomeScreenBranch")]
    private ScreenType _screenType = ScreenType.FullScreen;
    [SerializeField] 
    [HideIf("IsAPopUpBranch")] 
    private IsActive _stayOn = IsActive.No;
    [SerializeField] 
    [ShowIf("IsOptional")] private StoreAndRestorePopUps _clearOrResetOptional = StoreAndRestorePopUps.Reset;
    [SerializeField] 
    [ShowIf(EConditionOperator.Or, "IsHomeScreenBranch", "IsOptional")] 
    [Label("Tween on Return To Home")]
    private IsActive _tweenOnHome = IsActive.No;
    [SerializeField] 
    [Label("Save Position On Exit")] [HideIf("IsAPopUpBranch")] private IsActive _saveExitSelection = IsActive.Yes;
    [SerializeField] 
    [Label("Move To Next Branch...")] private WhenToMove _moveType = WhenToMove.Immediately;
    [SerializeField] 
    [HideIf(EConditionOperator.Or, "IsAPopUpBranch", "IsHomeScreenBranch", "IsPauseMenuBranch")]
    private EscapeKey _escapeKeyFunction = EscapeKey.GlobalSetting;
    [SerializeField]
    [ValidateInput("IsEmpty", "If left Blank it will auto-assign the first UINode in hierarchy/Group")]
    private UINode _startOnThisNode;
    [SerializeField] 
    // ReSharper disable once NotAccessedField.Local
    [ShowIf("IsPauseMenuBranch")] private UnityEvent _whenPausePressed;
    [SerializeField]
    [HideIf(EConditionOperator.Or, "IsAPopUpBranch", "IsHomeScreenBranch")] 
    [Label("Branch Group List (Leave blank if NO groups needed)")]
    [ReorderableList] private List<GroupList> _groupsList;
    [SerializeField] private BranchEvents _branchEvents;

    //Variables
    private UITweener _uiTweener;
    private int _groupIndex;
    private UIDataEvents _uiDataEvents;
    private UIControlsEvents _uiControlsEvents;
    private bool _onHomeScreen = true, _tweenOnChange = true, _canActivateBranch = true;
    private bool _activePopUp;

    //Enum
    private enum StoreAndRestorePopUps { StoreAndRestore, Reset }
    
    //Properties
    private void SaveIfOnHomeScreen(bool onHomeScreen) => _onHomeScreen = onHomeScreen;
    private void SaveHighlighted(INode newNode)
        => LastHighlighted = SearchThisBranchesNodes(newNode.ReturnNode, LastHighlighted.ReturnNode);
    private void SaveSelected(INode newNode)
        => LastSelected = SearchThisBranchesNodes(newNode.ReturnNode, LastSelected.ReturnNode);

    //Delegates
    public static event Action<UIBranch> DoActiveBranch;
    public Action _onStartPopUp; 
    private Action _onFinishedTrigger;

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
        _uiDataEvents = new UIDataEvents();
        _uiControlsEvents = new UIControlsEvents();
        Branch = BranchFactory.AssignType(this, _branchType, FindObjectsOfType<UIBranch>());
        MyParentBranch = this;
        SetStartPositions();
    }

    private void OnEnable()
    {
        _uiDataEvents.SubscribeToOnHomeScreen(SaveIfOnHomeScreen);
        _uiDataEvents.SubscribeToHighlightedNode(SaveHighlighted);
        _uiDataEvents.SubscribeToSelectedNode(SaveSelected);
        _uiControlsEvents.SubscribeSwitchGroups(SwitchBranchGroup);
        _uiControlsEvents.SubscribeMoveToChildBranch(MoveToAChildBranch);
        _uiControlsEvents.SubscribeTurnOffChildBranches(TurnOffChildBranches);
    }

    private void Start() => SetNodesChildrenToThisBranch();

    private void SetNodesChildrenToThisBranch()
    {
        foreach (var node in ThisGroupsUiNodes)
        {
            if (!node.HasChildBranch) continue;
            node.HasChildBranch.SetNewParentBranch(this);
        }
    }

    private void SetStartPositions()
    {
        SetDefaultStartPosition();
        LastHighlighted = DefaultStartOnThisNode;
        LastSelected = DefaultStartOnThisNode;
        if(_groupsList.Count <= 1) return;
        _groupIndex = UIBranchGroups.SetGroupIndex(DefaultStartOnThisNode, _groupsList);
    }

    private void SetDefaultStartPosition()
    {
        if (_startOnThisNode) return;
        if (_groupsList.Count > 0)
        {
            _startOnThisNode = _groupsList[0]._startNode;
        }
        else
        {
            FindStartPosition();
        }
    }

    private void FindStartPosition() => _startOnThisNode = transform.GetComponentsInChildren<UINode>().First();

    public void MoveToBranchWithoutTween()
    {
        MyCanvasGroup.blocksRaycasts = true;
        _tweenOnChange = false;
        MoveToThisBranch();
    }

    public void SetNoTween() => _tweenOnChange = false;
    
    public void DontSetBranchAsActive() => _canActivateBranch = false;

    public void MoveToThisBranch(UIBranch newParentBranch = null)
    {
        Branch.SetUpBranch(newParentBranch);
        if (_canActivateBranch) SetAsActiveBranch();
        
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

    public void SetNewParentBranch(UIBranch newParentController) 
    {
        if(newParentController is null) return;
        MyParentBranch = newParentController;
    }

    private void SetAsActiveBranch() => DoActiveBranch?.Invoke(this);

    private void SwitchBranchGroup(SwitchType switchType)
    {
        if (_onHomeScreen || !CanvasIsEnabled || _groupsList.Count <= 1) return;
        _groupIndex = UIBranchGroups.SwitchBranchGroup(_groupsList, _groupIndex, switchType);
    }

    public void ResetBranchesStartPosition()
    {
        if (_saveExitSelection == IsActive.Yes) return;
        _groupIndex = UIBranchGroups.SetGroupIndex(DefaultStartOnThisNode, _groupsList);
        LastHighlighted = DefaultStartOnThisNode;
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

    private void MoveToAChildBranch((UIBranch moveFrom, UIBranch moveToo) data)
    {
        if(data.moveFrom != this) return;
        StopReturnFlashFromFullScreen(data);
        
        if (data.moveToo.IsInternalBranch())
        {
            ToChildBranchProcess();
        }
        else
        {
            StartOutTweenProcess(OutTweenType.MoveToChild, ToChildBranchProcess);
        }
        void ToChildBranchProcess() => data.moveToo.MoveToThisBranch(this);
    }

    private static void StopReturnFlashFromFullScreen((UIBranch moveFrom, UIBranch moveToo) data)
    {
        if (data.moveToo._screenType == ScreenType.FullScreen)
            data.moveFrom.LastSelected.SetNodeAsNotSelected_NoEffects();
    }

    private void TurnOffChildBranches(UIBranch branchToClose)
    {
        if(branchToClose != this || !CanvasIsEnabled) return;
        StartOutTweenProcess(OutTweenType.Cancel);
    }

    public void StartOutTweenProcess(OutTweenType outTweenType, Action endOfTweenCallback = null)
    {
        if (WhenToMove == WhenToMove.AfterEndOfTween)
        {
            StartOutTween(outTweenType, endOfTweenCallback);
        }
        else
        {
            StartOutTween(outTweenType);
            endOfTweenCallback?.Invoke();
        }
    }

    private void StartOutTween(OutTweenType outTweenType, Action endOfTweenCallBack = null)
    {
        SetUpBranchForTween(outTweenType, endOfTweenCallBack);
        ProcessTween(outTweenType);
    }

    private void SetUpBranchForTween(OutTweenType outTweenType, Action endOfTweenCallBack)
    {
        _branchEvents?._onBranchExit.Invoke();
        _onFinishedTrigger = endOfTweenCallBack;
        if (_stayOn == IsActive.No || outTweenType == OutTweenType.Cancel)
            MyCanvasGroup.blocksRaycasts = false;
    }

    private void ProcessTween(OutTweenType outTweenType)
    {
        _uiTweener.StopAllCoroutines();
        void CallBack() => OutTweenCallback(outTweenType);
        _uiTweener.DeactivateTweens(CallBack);
    }

    private void OutTweenCallback(OutTweenType outTweenType)
    {
        if(_stayOn == IsActive.No || outTweenType == OutTweenType.Cancel)
            MyCanvas.enabled = false;
        
        _onFinishedTrigger?.Invoke();
    }

    private void ActivateInTweens() => _uiTweener.ActivateTweens(InTweenCallback);

    private void InTweenCallback()
    {
        if (_canActivateBranch)
            LastHighlighted.SetNodeAsActive();

        _branchEvents?._onBranchEnter.Invoke();
        _canActivateBranch = true;
    }

    /// <summary>
    /// Call To to start any PopUps through I StartPopUp
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public void StartPopUp() => _onStartPopUp?.Invoke();
}

