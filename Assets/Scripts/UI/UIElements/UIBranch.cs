using System;
using System.Collections.Generic;
using System.Linq;
using EZ.Events;
using EZ.Service;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;
using UIElements;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(GraphicRaycaster))]
[RequireComponent(typeof(UITweener))]


public partial class UIBranch : MonoBehaviour, IEZEventUser, IActiveBranch, IBranch, IEZEventDispatcher,
                                IPointerEnterHandler, IPointerExitHandler, IServiceUser, ICloseBranch
{
    [Header("Branch Main Settings")] [HorizontalLine(1f, EColor.Blue, order = 1)]
    [SerializeField]
    private BranchType _branchType = BranchType.Standard;

    [SerializeField]
    [ShowIf(HomeScreenBranch)] [Label("Is Control Bar")]
    private IsActive _controlBar = IsActive.No;
    
    [SerializeField]
    [Label("Start On (Optional)")] 
    private UINode _startOnThisNode;

    [SerializeField]
    [ShowIf(EConditionOperator.Or, TimedBranch, ResolveBranch)]
    private IsActive _onlyAllowOnHomeScreen = IsActive.Yes;

    [SerializeField] 
    [ShowIf(TimedBranch)] [Range(0f,20f)] 
    private float _timer = 5f;

    [SerializeField] 
    [ShowIf(OptionalBranch)]
    [Label("When Not On Home Screen")]
    private StoreAndRestorePopUps _storeOrResetOptional = StoreAndRestorePopUps.Close;
    
    [SerializeField]
    [Label("Move To Next Branch...")] [HideIf(InGamUIBranch)] 
    private WhenToMove _moveType = WhenToMove.Immediately;

    [SerializeField] 
    [ShowIf(StandardBranch)] 
    private OrderInCanvas _canvasOrderSetting = OrderInCanvas.Default;

    [SerializeField] 
    [ShowIf(ShowManualOrder)] 
    [OnValueChanged(SetUpCanvasOrder)] 
    private int _orderInCanvas;

    [SerializeField] 
    [HideIf(EConditionOperator.Or, OptionalBranch, TimedBranch, HomeScreenBranch, ControlBarBranch, InGamUIBranch)]
    [Label("Overlay Or Fullscreen")]
    private ScreenType _screenType = ScreenType.Overlay;
    
    [SerializeField] 
    [HideIf(EConditionOperator.Or, AnyPopUpBranch, Fullscreen, ControlBarBranch, InGamUIBranch)]
    [ValidateInput(ValidInAndOutTweens, MessageINAndOutTweens)]
    [Label("Visible When Child Active")]
    private IsActive _stayVisible = IsActive.No;

    [SerializeField] 
    [ShowIf(EConditionOperator.Or, HomeScreenButNotControl, Stored)] 
    [Label("On Return To Home Screen")]
    private DoTween _tweenOnHome = DoTween.Tween;

    [SerializeField] 
    [Label("Save Last Highlighted")] [HideIf(EConditionOperator.Or,AnyPopUpBranch, InGamUIBranch)] 
    private IsActive _saveExitSelection = IsActive.Yes;
    
    [SerializeField] 
    [Label("Always Show Highlighted")] [HideIf(EConditionOperator.Or,AnyPopUpBranch, InGamUIBranch, SaveLastHighlightedOff)]
    [Tooltip("Can be Overridden from each Node setting")]
    private IsActive _alwaysHighlighted = IsActive.No;
    
    [SerializeField] 
    [ShowIf(CanAutoOpenClose)] 
    [Label("Auto Close Branch")]
    private IsActive _autoClose = IsActive.No;

    [SerializeField]
    [ShowIf(CanAutoOpenClose)] 
    [Label("Auto Close Delay")]
    [Range(0f, 1f)]private float _autoCloseDelay = 0.25f;

    [SerializeField] 
    [ShowIf(EConditionOperator.Or, StandardBranch)]
    private EscapeKey _escapeKeyFunction = EscapeKey.GlobalSetting;

    [SerializeField]
    [HideIf(EConditionOperator.Or, AnyPopUpBranch, HomeScreenBranch, InGamUIBranch, Overlay)] 
    [Label("Branch Groups List (Leave blank if NO groups needed)")]
    [InfoBox("")]
    [ReorderableList] private List<GroupList> _groupsList;

    [Header("Events & Create New Buttons", order = 2)][HorizontalLine(1f, EColor.Blue, order = 3)] 
    [Space(20, order = 1)]
    [SerializeField] 
    private BranchEvents _branchEvents;
    
    
    //Buttons
    [Button("Create Node")]
    private void CreateNode() => new CreateNewObjects().CreateNode(transform);

    [Button("Create Branch")]
    private void CreateBranch() => new CreateNewObjects().CreateBranch(transform.parent)
                                                         .CreateNode();
    
    //Variables
    private UITweener _uiTweener;
    private bool _tweenOnChange = true, _canActivateBranch = true;
    private bool _activePopUp, _isTabBranch, _sceneIsChanging;
    private IBranchBase _branchTypeBaseClass;
    private IHub _myHub;

    //Delegates & Events
    private Action TweenFinishedCallBack { get; set; }
    private  Action<IActiveBranch> SetAsActiveBranch { get; set; }        
    private Action<ICloseBranch> CloseAndResetBranch { get; set; }

    

    //Getters & Setters
    private void SceneIsChanging(ISceneIsChanging args) => _sceneIsChanging = true;
    private void SaveHighlighted(IHighlightedNode args)
    {
        if(args.Highlighted.MyBranch.NotEqualTo(this)) return;
        if(LastHighlighted == args.Highlighted) return;
        
        ClearNodeIfAlwaysHighlightedIsOn();

        LastHighlighted = NodeSearch.Find(args.Highlighted)
                                    .DefaultReturn(LastSelected)
                                    .RunOn(ThisGroupsUiNodes);
    }

    private void ClearNodeIfAlwaysHighlightedIsOn()
    {
        var tempStore = _alwaysHighlighted;
        _alwaysHighlighted = IsActive.No;
        LastHighlighted.UnHighlightAlwaysOn();
        _alwaysHighlighted = tempStore;
    }

    private void SaveSelected(ISelectedNode args)
    {
        if(args.UINode.IsNull()) return;
        if(args.UINode.MyBranch.NotEqualTo(this)) return;

        LastSelected = NodeSearch.Find(args.UINode)
                                 .DefaultReturn(LastSelected)
                                 .RunOn(ThisGroupsUiNodes);
    }

    //Main
    private void Awake()
    {
        CheckForValidSetUp();
        ThisGroupsUiNodes = BranchChildNodeUtil.GetChildNodes(this);
        MyCanvasGroup = GetComponent<CanvasGroup>();
        MyCanvasGroup.blocksRaycasts = false;
        _uiTweener = GetComponent<UITweener>();
        MyCanvas = GetComponent<Canvas>();
        if(IsHomeScreenBranch() || IsInGameBranch())
            MyParentBranch = this;
        AutoOpenCloseClass = EZInject.Class.WithParams<IAutoOpenClose>(this); 
        _branchTypeBaseClass = BranchFactory.Factory.PassThisBranch(this).CreateType(_branchType);
        _branchTypeBaseClass.OnAwake();
    }

    private void CheckForValidSetUp()
    {
        if (AllowableInAndOutTweens(_stayVisible)) return;
        
        throw new Exception($"Can't have Stay Visible and also have IN AND OUT Tweens on : {this} " +
                  $"{Environment.NewLine} OutTween NOT Allowed");
    }

    public void OnEnable()
    {
        UseEZServiceLocator();
        FetchEvents();
        ObserveEvents();
        SetStartPositions();
        _branchTypeBaseClass.OnEnable();
        AutoOpenCloseClass.OnEnable();
    }

    private void SetStartPositions()
    {
        SetDefaultStartPosition();
        LastHighlighted = DefaultStartOnThisNode;
        LastSelected = DefaultStartOnThisNode;
        if(_groupsList.Count <= 1) return;
        GroupIndex = BranchGroups.SetGroupIndex(DefaultStartOnThisNode, _groupsList);
    }

    private void SetDefaultStartPosition()
    {
        if (_startOnThisNode)
        {
            DefaultStartOnThisNode = _startOnThisNode;
            return;
        }        
        if (_groupsList.Count > 0)
        {
            DefaultStartOnThisNode = _groupsList.First().StartNode;
        }
        else
        {
            DefaultStartOnThisNode = (UINode) ThisGroupsUiNodes.First();
        }
    }

    public void UseEZServiceLocator() => _myHub = EZService.Locator.Get<IHub>(this);

    public void FetchEvents()
    {
        CloseAndResetBranch = BranchEvent.Do.Fetch<ICloseBranch>();
        SetAsActiveBranch = HistoryEvents.Do.Fetch<IActiveBranch>();
    }

    public void ObserveEvents()
    {
        HistoryEvents.Do.Subscribe<ISceneIsChanging>(SceneIsChanging);
        HistoryEvents.Do.Subscribe<IHighlightedNode>(SaveHighlighted);
        HistoryEvents.Do.Subscribe<ISelectedNode>(SaveSelected);
    }

    public void UnObserveEvents()
    {
        HistoryEvents.Do.Unsubscribe<ISceneIsChanging>(SceneIsChanging);
        HistoryEvents.Do.Unsubscribe<IHighlightedNode>(SaveHighlighted);
        HistoryEvents.Do.Unsubscribe<ISelectedNode>(SaveSelected);
    }

    public IBranch TargetBranch => this;
    public GOUIModule ReturnGOUIModule => _branchTypeBaseClass.ReturnGOUIModule() as GOUIModule;

    public void OnDisable()
    {
        CloseAndResetBranch?.Invoke(this);

        UnObserveEvents();
        AutoOpenCloseClass.OnDisable();
        
        if(_sceneIsChanging) return;
        _branchTypeBaseClass.OnDisable();
        SetAsActiveBranch = null;
        _myHub = null;
    }

    public void OnDestroy()
    {
        if(!_sceneIsChanging)
            MyCanvas.enabled = false;
        UnObserveEvents();
        _branchTypeBaseClass.OnDestroy();
        SetAsActiveBranch = null;
        _myHub = null;
    }

    private void Start()
    {
        _branchTypeBaseClass.OnStart();
        CheckForControlBar();
    }

    private void CheckForControlBar() => BranchGroups.AddControlBarToGroupList(_groupsList, _myHub.HomeBranches, this);

    public void StartPopUp_RunTimeCall(bool fromPool)
    {
        if(fromPool)
            OnEnable();
        MoveToThisBranch();
    }
    
    public void MoveToThisBranch(IBranch newParentBranch = null)
    {
        if(!_branchTypeBaseClass.CanStartBranch()) return;
        
        _branchTypeBaseClass.SetUpBranch(newParentBranch);

        SetBranchAsActive();
        
        if (_tweenOnChange)
        {
            _uiTweener.StartInTweens(callBack: InTweenCallback);
        }
        else
        {
            InTweenCallback();
        }

        _tweenOnChange = true;
    }

    public void SetBranchAsActive()
    {
        if (_canActivateBranch)
            SetAsActiveBranch?.Invoke(this);
    }

    private void InTweenCallback()
    {
        _branchTypeBaseClass.EndOfBranchStart();
       
        SetHighlightedNode();
        
        _branchEvents.OnBranchEnter();
        _canActivateBranch = true;
    }

    private void SetHighlightedNode()
    {
        if(LastHighlighted.IsNull()) return;
        
        if (_canActivateBranch)
            LastHighlighted.SetNodeAsActive();

        GroupIndex = BranchGroups.SetGroupIndex(LastHighlighted, _groupsList);
    }

    public void StartBranchExitProcess(OutTweenType outTweenType, Action endOfTweenCallback = null)
    {
        if(!CanvasIsEnabled || DontExitBranch())
        {
            endOfTweenCallback?.Invoke();
            return;
        }
        
        if (WhenToMove == WhenToMove.AfterEndOfTween)
        {
            StartOutTween(endOfTweenCallback);
        }
        else
        {
            StartOutTween();
            endOfTweenCallback?.Invoke();
        }

        bool DontExitBranch() => !_branchTypeBaseClass.CanExitBranch(outTweenType);
    }

    private void StartOutTween(Action endOfTweenCallback = null)
    {
        TweenFinishedCallBack = endOfTweenCallback;
        
        _branchEvents.OnBranchExit();
        _branchTypeBaseClass.StartBranchExit();
        _uiTweener.StartOutTweens(OutTweenCallback);
        SetSaveLastSelected();
        
        void OutTweenCallback()
        {
            _branchTypeBaseClass.EndOfBranchExit();
            TweenFinishedCallBack?.Invoke();
        }
    }

    private void SetSaveLastSelected()
    {
        if (_saveExitSelection == IsActive.No)
            LastHighlighted = ThisGroupsUiNodes[0];
    }

    public void SetCanvas(ActiveCanvas activeCanvas) => _branchTypeBaseClass.SetCanvas(activeCanvas);
    public void SetBlockRaycast(BlockRaycast blockRaycast) => _branchTypeBaseClass.SetBlockRaycast(blockRaycast);
    public void SetUpAsTabBranch() => _branchTypeBaseClass.SetUpAsTabBranch();
    public void SetUpGOUIBranch(IGOUIModule module) => _branchTypeBaseClass.SetUpGOUIBranch(module);

    public void OnPointerEnter(PointerEventData eventData) => AutoOpenCloseClass.OnPointerEnter();

    public void OnPointerExit(PointerEventData eventData) => AutoOpenCloseClass.OnPointerExit();
}