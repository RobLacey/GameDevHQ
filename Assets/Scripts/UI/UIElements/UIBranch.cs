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

public partial class UIBranch : MonoBehaviour, IStartPopUp, IEventUser, IActiveBranch
{
    [SerializeField]
    private BranchType _branchType = BranchType.Standard;
    [SerializeField] 
    [ShowIf("IsTimedPopUp")] private float _timer = 1f;
    [SerializeField] 
    [HideIf(EConditionOperator.Or, "IsOptional", "IsTimedPopUp", "IsHomeScreenBranch")]
    private ScreenType _screenType = ScreenType.FullScreen;
    [SerializeField] 
    [HideIf("IsAPopUpBranch")] 
    private IsActive _stayOn = IsActive.No;
    [SerializeField] 
    [ShowIf("IsOptional")] private StoreAndRestorePopUps _storeOrResetOptional = StoreAndRestorePopUps.Reset;
    [SerializeField] 
    [ShowIf(EConditionOperator.Or, "IsHomeScreenBranch", "IsStored")] 
    [Label("Tween on Return To Home")]
    private IsActive _tweenOnHome = IsActive.No;
    [SerializeField] 
    [Label("Save Position On Exit")] [HideIf("IsAPopUpBranch")] private IsActive _saveExitSelection = IsActive.Yes;
    [SerializeField] 
    [Label("Move To Next Branch...")] private WhenToMove _moveType = WhenToMove.Immediately;
    [SerializeField] 
    [HideIf(EConditionOperator.Or, "IsAPopUpBranch", "IsHomeScreenBranch", "IsPauseMenuBranch", "IsInternalBranch")]
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
    private bool _onHomeScreen = true, _tweenOnChange = true, _canActivateBranch = true;
    private bool _activePopUp;
    private bool _isTabBranch;

    //Enum
    private enum StoreAndRestorePopUps { StoreAndRestore, Reset }
    
    //Properties
    private void SaveIfOnHomeScreen(IOnHomeScreen args) => _onHomeScreen = args.OnHomeScreen;
    private void SaveHighlighted(IHighlightedNode args)
    {
        LastHighlighted = NodeSearch.Search.Find(args.Highlighted)
                                    .DefaultReturn(LastSelected)
                                    .RunOn(ThisGroupsUiNodes);
    }
    private void SaveSelected(ISelectedNode args)
    {
        LastSelected = NodeSearch.Search.Find(args.Selected)
                                 .DefaultReturn(LastSelected)
                                 .RunOn(ThisGroupsUiNodes);
    }    
    public void SetNoTween() => _tweenOnChange = false;
    public void DontSetBranchAsActive() => _canActivateBranch = false;
    public UIBranch ActiveBranch => this;

    /// <summary>
    /// Call To to start any PopUps through I StartPopUp
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public void StartPopUp() => OnStartPopUp?.Invoke();
    
    //Delegates & Events
    public event Action OnStartPopUp; 
    private event Action OnFinishTweenCallBack;
    private static CustomEvent<IActiveBranch> SetActiveBranch { get; }  = new CustomEvent<IActiveBranch>();

    //InternalClasses
    [Serializable]
    private class BranchEvents
    {
        public UnityEvent OnBranchEnter;
        public UnityEvent OnBranchExit;
    }

    public void ObserveEvents()
    {
        EventLocator.Subscribe<ISwitchGroupPressed>(SwitchBranchGroup, this);
        EventLocator.Subscribe<IHighlightedNode>(SaveHighlighted, this);
        EventLocator.Subscribe<ISelectedNode>(SaveSelected, this);
        EventLocator.Subscribe<IOnHomeScreen>(SaveIfOnHomeScreen, this);
        EventLocator.Subscribe<IHotKeyPressed>(FromHotKey, this);
    }

    public void RemoveFromEvents()
    {
        EventLocator.Unsubscribe<ISwitchGroupPressed>(SwitchBranchGroup);
        EventLocator.Unsubscribe<IHighlightedNode>(SaveHighlighted);
        EventLocator.Unsubscribe<ISelectedNode>(SaveSelected);
        EventLocator.Unsubscribe<IOnHomeScreen>(SaveIfOnHomeScreen);
        EventLocator.Unsubscribe<IHotKeyPressed>(FromHotKey);
    }
    
    private void Awake()
    {
        ThisGroupsUiNodes = SetBranchesChildNodes.GetChildNodes(this);
        MyCanvasGroup = GetComponent<CanvasGroup>();
        MyCanvasGroup.blocksRaycasts = false;
        _uiTweener = GetComponent<UITweener>();
        MyCanvas = GetComponent<Canvas>();
        Branch = BranchFactory.Factory
                              .PassThisBranch(this) 
                              .PassAllBranches(FindObjectsOfType<UIBranch>()) 
                              .CreateType(_branchType);
        MyParentBranch = this;
        SetStartPositions();
        ObserveEvents();
    }

    private void OnDisable()
    {
        Branch.OnDisable();
        RemoveFromEvents();
    }

    private void Start() => SetNodesChildrenToThisBranch();

    private void SetNodesChildrenToThisBranch()
    {
        foreach (var node in ThisGroupsUiNodes)
        {
            if (!node.HasChildBranch) continue;
            node.HasChildBranch.MyParentBranch = this;
        }
    }

    private void SetStartPositions()
    {
        SetDefaultStartPosition();
        LastHighlighted = DefaultStartOnThisNode;
        LastSelected = DefaultStartOnThisNode;
        if(_groupsList.Count <= 1) return;
        _groupIndex = UIBranchGroups.SetGroupIndex(DefaultStartOnThisNode.ReturnNode, _groupsList);
    }

    private void SetDefaultStartPosition()
    {
        if (_startOnThisNode) return;
        if (_groupsList.Count > 0)
        {
            _startOnThisNode = _groupsList.First()._startNode;
        }
        else
        {
            FindStartPosition();
        }
    }

    private void FindStartPosition() 
        => _startOnThisNode = transform.GetComponentsInChildren<UINode>().First();

    private void FromHotKey(IHotKeyPressed args)
    {
        if (args.MyBranch == this)
        {
            MoveToThisBranch();
        }
    }
    public void MoveToBranchWithoutTween()
    {
        Branch.SetBlockRaycast(BlockRaycast.Yes);
        _tweenOnChange = false;
        MoveToThisBranch();
    }
    
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
    
    private void SetAsActiveBranch() => SetActiveBranch?.RaiseEvent(this);

    private void SwitchBranchGroup(ISwitchGroupPressed args)
    {
        if (_onHomeScreen || !CanvasIsEnabled || _groupsList.Count <= 1) return;
        _groupIndex = UIBranchGroups.SwitchBranchGroup(_groupsList, _groupIndex, args.SwitchType);
    }

    public void ResetBranchesStartPosition()
    {
        if (_saveExitSelection == IsActive.Yes) return;
        _groupIndex = UIBranchGroups.SetGroupIndex(DefaultStartOnThisNode.ReturnNode, _groupsList);
        LastHighlighted = DefaultStartOnThisNode;
    }

    public void NavigateToChildBranch(UIBranch moveToo)
    {
        if (moveToo.IsInternalBranch())
        {
            ToChildBranchProcess();
        }
        else
        {
            StartBranchExitProcess(OutTweenType.MoveToChild, ToChildBranchProcess);
        }
        void ToChildBranchProcess() => moveToo.MoveToThisBranch(newParentBranch: this);
    }
    
    public void StartBranchExitProcess(OutTweenType outTweenType, Action endOfTweenCallback = null)
    {
        if(!CanvasIsEnabled) return;
        
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
        SetUpBranchForTween(outTweenType);
        OnFinishTweenCallBack = endOfTweenCallBack;
        ProcessTween(outTweenType);
    }

    private void SetUpBranchForTween(OutTweenType outTweenType)
    {
        _branchEvents?.OnBranchExit.Invoke();
        if (_stayOn == IsActive.No || outTweenType == OutTweenType.Cancel)
            Branch.SetBlockRaycast(BlockRaycast.No);
    }

    private void ProcessTween(OutTweenType outTweenType)
    {
        _uiTweener.StopAllCoroutines();
        _uiTweener.DeactivateTweens(callBack:() => OutTweenCallback(outTweenType));
    }

    private void OutTweenCallback(OutTweenType outTweenType)
    {
        if(_stayOn == IsActive.No || outTweenType == OutTweenType.Cancel)
            Branch.SetCanvas(ActiveCanvas.No);
        if(!IsPauseMenuBranch()) 
            Branch.ActivateStoredPosition();
        OnFinishTweenCallBack?.Invoke();
    }

    private void ActivateInTweens() => _uiTweener.ActivateTweens(callBack: InTweenCallback);

    private void InTweenCallback()
    {
        if (_canActivateBranch)
        {
           LastHighlighted.SetNodeAsActive();
        }
        Branch.SetBlockRaycast(BlockRaycast.Yes);
        _branchEvents?.OnBranchEnter.Invoke();
        _canActivateBranch = true;
    }
}

