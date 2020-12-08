using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;
using UnityEngine.Events;
using UnityEngine.EventSystems;


[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(GraphicRaycaster))]
[RequireComponent(typeof(UITweener))]

public partial class UIBranch : MonoBehaviour, IStartPopUp, IEventUser, IActiveBranch, IBranch, IEventDispatcher,
                                IPointerEnterHandler, IPointerExitHandler, ICancelHoverOver
{
    [SerializeField]
    private BranchType _branchType = BranchType.Standard;
    [SerializeField] 
    [HideIf(EConditionOperator.Or, "IsOptional", "IsTimedPopUp")] [Label("Auto Open/Close")]
    private AutoOpenClose _autoOpenClose = AutoOpenClose.No;
    [SerializeField] 
    [ShowIf("IsTimedPopUp")] private float _timer = 1f;
    [SerializeField] 
    [HideIf(EConditionOperator.Or, "IsOptional", "IsTimedPopUp", "IsHomeScreenBranch")]
    private ScreenType _screenType = ScreenType.FullScreen;
    [SerializeField] 
    [HideIf(EConditionOperator.Or, "IsAPopUpBranch", "IsFullScreen")] 
    private IsActive _stayVisible = IsActive.No;
    [SerializeField] 
    [ShowIf("IsOptional")] private StoreAndRestorePopUps _storeOrResetOptional = StoreAndRestorePopUps.Reset;
    [SerializeField] 
    [ShowIf(EConditionOperator.Or, "IsHomeScreenBranch", "IsStored")] 
    [Label("On Return To Home Screen")]
    private DoTween _tweenOnHome = DoTween.Tween;
    [SerializeField] 
    [Label("Save Position On Exit")] [HideIf("IsAPopUpBranch")] private IsActive _saveExitSelection = IsActive.Yes;
    [SerializeField] 
    [Label("Move To Next Branch...")] private WhenToMove _moveType = WhenToMove.Immediately;
    [SerializeField] 
    [ShowIf(EConditionOperator.Or, "IsStandardBranch")]
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
    private bool _activePopUp, _isTabBranch;
    
    //Properties
    public AutoOpenClose AutoOpenClose
    {
        get => _autoOpenClose;
        set => _autoOpenClose = value;
    }

    public bool PointerOverBranch { get; set; }

    //Enum
    private enum StoreAndRestorePopUps { StoreAndRestore, Reset }
    
    //Set / Getters
    public bool CanAutoClose() => AutoOpenClose == AutoOpenClose.Both || AutoOpenClose == AutoOpenClose.Close;
    public bool CanAutoOpen() => AutoOpenClose == AutoOpenClose.Both || AutoOpenClose == AutoOpenClose.Open;
    private void SaveIfOnHomeScreen(IOnHomeScreen args) => _onHomeScreen = args.OnHomeScreen;
    private void SaveHighlighted(IHighlightedNode args)
    {
        LastHighlighted = NodeSearch.Find(args.Highlighted)
                                    .DefaultReturn(LastSelected)
                                    .RunOn(ThisGroupsUiNodes);
    }
    private void SaveSelected(ISelectedNode args)
    {
        LastSelected = NodeSearch.Find(args.UINode)
                                 .DefaultReturn(LastSelected)
                                 .RunOn(ThisGroupsUiNodes);
    }    
    /// <summary>
    /// Call To to start any PopUps through I StartPopUp
    /// </summary>

    public void StartPopUp() => OnStartPopUp?.Invoke();
    public void DoNotTween() => _tweenOnChange = false;
    public void DontSetBranchAsActive() => _canActivateBranch = false;
    public IBranch[] FindAllBranches() => FindObjectsOfType<UIBranch>().ToArray<IBranch>(); //TODO Write Up this


    
    //Delegates & Events
    public event Action OnStartPopUp; 
    private Action OnFinishTweenCallBack { get; set; }
    private  Action<IActiveBranch> SetActiveBranch { get; set; }
    private  Action<ICancelHoverOver> CancelHooverOver { get; set; }

    //InternalClasses
    [Serializable]
    private class BranchEvents
    {
        public UnityEvent OnBranchEnter;
        public UnityEvent OnBranchExit;
    }

    //Main
    private void Awake()
    {
        ThisGroupsUiNodes = SetBranchesChildNodes.GetChildNodes(this);
        MyCanvasGroup = GetComponent<CanvasGroup>();
        MyCanvasGroup.blocksRaycasts = false;
        _uiTweener = GetComponent<UITweener>();
        MyCanvas = GetComponent<Canvas>();
        MyParentBranch = this;
        SetStartPositions();
        SetNodesChildrenToThisBranch();
    }

    private void SetStartPositions()
    {
        SetDefaultStartPosition();
        LastHighlighted = DefaultStartOnThisNode;
        LastSelected = DefaultStartOnThisNode;
        if(_groupsList.Count <= 1) return;
        _groupIndex = BranchGroups.SetGroupIndex(DefaultStartOnThisNode, _groupsList);
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
        
        void FindStartPosition() 
            => _startOnThisNode = transform.GetComponentsInChildren<UINode>().First();
    }

    private void SetNodesChildrenToThisBranch()
    {
        foreach (var node in ThisGroupsUiNodes)
        {
            if (node.HasChildBranch is null) continue;
            node.HasChildBranch.MyParentBranch = this;
        }
    }

    private void OnEnable()
    {
        FetchEvents();
        ObserveEvents();
        BranchBase = BranchFactory.Factory.PassThisBranch(this).CreateType(_branchType);
        BranchBase.OnEnable();
    }

    private void OnDisable()
    {
        RemoveEvents();
        BranchBase.OnDisable();
    }

    public void FetchEvents()
    {
        SetActiveBranch = EVent.Do.Fetch<IActiveBranch>();
        CancelHooverOver = EVent.Do.Fetch<ICancelHoverOver>();
    }

    public void ObserveEvents()
    {
        EVent.Do.Subscribe<ISwitchGroupPressed>(SwitchBranchGroup);
        EVent.Do.Subscribe<IHighlightedNode>(SaveHighlighted);
        EVent.Do.Subscribe<ISelectedNode>(SaveSelected);
        EVent.Do.Subscribe<IOnHomeScreen>(SaveIfOnHomeScreen);
    }

    public void RemoveEvents()
    {
        EVent.Do.Unsubscribe<ISwitchGroupPressed>(SwitchBranchGroup);
        EVent.Do.Unsubscribe<IHighlightedNode>(SaveHighlighted);
        EVent.Do.Unsubscribe<ISelectedNode>(SaveSelected);
        EVent.Do.Unsubscribe<IOnHomeScreen>(SaveIfOnHomeScreen);
    }
    
    public void MoveToThisBranch(IBranch newParentBranch = null)
    {
        BranchBase.SetUpBranch(newParentBranch);
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
    
    private void SetAsActiveBranch() => SetActiveBranch?.Invoke(this);

    private void SwitchBranchGroup(ISwitchGroupPressed args)
    {
        if (_onHomeScreen || !CanvasIsEnabled || _groupsList.Count <= 1) return;
        _groupIndex = BranchGroups.SwitchBranchGroup(_groupsList, _groupIndex, args.SwitchType);
    }

    public void SetHighlightedNode()
    {
        if (_saveExitSelection == IsActive.Yes) return;
        _groupIndex = BranchGroups.SetGroupIndex(DefaultStartOnThisNode, _groupsList);
        LastHighlighted = DefaultStartOnThisNode;
    }

    public void NavigateToChildBranch(IBranch moveToo)
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
        if (_stayVisible == IsActive.No || outTweenType == OutTweenType.Cancel)
            BranchBase.SetBlockRaycast(BlockRaycast.No);
    }

    private void ProcessTween(OutTweenType outTweenType)
    {
        _uiTweener.StopAllCoroutines();
        _uiTweener.DeactivateTweens(callBack:() => OutTweenCallback(outTweenType));
    }

    private void OutTweenCallback(OutTweenType outTweenType)
    {
        if(_stayVisible == IsActive.No || outTweenType == OutTweenType.Cancel)
            BranchBase.SetCanvas(ActiveCanvas.No);
        if(!IsPauseMenuBranch()) 
            BranchBase.ActivateStoredPosition();
        OnFinishTweenCallBack?.Invoke();
    }

    private void ActivateInTweens() => _uiTweener.ActivateTweens(callBack: InTweenCallback);

    private void InTweenCallback()
    {
        if (_canActivateBranch)
        {
           LastHighlighted.SetNodeAsActive();
        }
        BranchBase.SetBlockRaycast(BlockRaycast.Yes);
        _branchEvents?.OnBranchEnter.Invoke();
        _canActivateBranch = true;
    }

    public void OnPointerEnter(PointerEventData eventData) => PointerOverBranch = true;

    public void OnPointerExit(PointerEventData eventData)
    {
        PointerOverBranch = false;
        if (!CanAutoClose() || MyParentBranch.PointerOverBranch) return;
        
        EscapeKeyType = EscapeKey.BackToHome;
        CancelHooverOver?.Invoke(this);
    }
}

