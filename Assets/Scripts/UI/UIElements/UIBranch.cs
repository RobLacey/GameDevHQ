using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(GraphicRaycaster))]
[RequireComponent(typeof(UITweener))]


public partial class UIBranch : MonoBehaviour, IEventUser, IActiveBranch, IBranch, IEventDispatcher,
                                IPointerEnterHandler, IPointerExitHandler, IGetHomeBranches
{
    [Header("Branch Main Settings")] [HorizontalLine(1f, EColor.Blue, order = 1)]
    [SerializeField]
    private BranchType _branchType = BranchType.Standard;
    
    [SerializeField] 
    [ShowIf(EConditionOperator.Or, HomeScreenBranch)] [Label("Is Control Bar")]
    private IsActive _controlBar = IsActive.No;

    [SerializeField]
    [Label("Start On (Optional)")] 
    private UINode _startOnThisNode;
    
    [SerializeField] 
    [ShowIf(ManualOrder)] 
    [OnValueChanged(SetUpCanvasOrder)] 
    private int _orderInCanvas;

    [SerializeField]
    [ShowIf(EConditionOperator.Or, TimedBranch, ResolveBranch)]
    private IsActive _onlyAllowOnHomeScreen = IsActive.Yes;
    
    [SerializeField] 
    [ShowIf(EConditionOperator.Or, HomeScreenBranch, StandardBranch)] 
    private OrderInCanvas _canvasOrderSetting = OrderInCanvas.Default;
    
    [SerializeField]
    [Label("Move To Next Branch...")] [HideIf(InGamUIBranch)] 
    private WhenToMove _moveType = WhenToMove.Immediately;
    
    [SerializeField] 
    [HideIf(EConditionOperator.Or, ControlBarBranch, AnyPopUpBranch, InGamUIBranch)] 
    [Label("Auto Open/Close")]
    private AutoOpenClose _autoOpenClose = AutoOpenClose.No;
    
    [SerializeField] 
    [ShowIf(TimedBranch)] [Range(0f,20f)] 
    private float _timer = 5f;
    
    [SerializeField] 
    [HideIf(EConditionOperator.Or, OptionalBranch, TimedBranch, HomeScreenBranch, ControlBarBranch, InGamUIBranch)]
    private ScreenType _screenType = ScreenType.Normal;
    
    [SerializeField] 
    [HideIf(EConditionOperator.Or, AnyPopUpBranch, Fullscreen, ControlBarBranch, InGamUIBranch)]
    [ValidateInput(ValidInAndOutTweens, MessageINAndOutTweens)]
    private IsActive _stayVisible = IsActive.No;
    
    [SerializeField] 
    [ShowIf(OptionalBranch)] 
    private StoreAndRestorePopUps _storeOrResetOptional = StoreAndRestorePopUps.Reset;
    
    [SerializeField] 
    [ShowIf(EConditionOperator.Or, HomeScreenButNotControl, Stored)] 
    [Label("On Return To Home Screen")]
    private DoTween _tweenOnHome = DoTween.Tween;
    
    [SerializeField] 
    [Label("Save Position On Exit")] [HideIf(EConditionOperator.Or,AnyPopUpBranch, InGamUIBranch)] 
    private IsActive _saveExitSelection = IsActive.Yes;
    
    [SerializeField] 
    [ShowIf(EConditionOperator.Or, StandardBranch)]
    private EscapeKey _escapeKeyFunction = EscapeKey.GlobalSetting;

    [SerializeField]
    [HideIf(EConditionOperator.Or, AnyPopUpBranch, HomeScreenBranch, InGamUIBranch)] 
    [Label("Branch Groups List (Leave blank if NO groups needed)")] 
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
    private int _groupIndex;
    private bool _onHomeScreen = true, _tweenOnChange = true, _canActivateBranch = true;
    private bool _activePopUp, _isTabBranch, _canStartGOUI;
    private IBranchBase _branchTypeClass;
    private INode _lastHighlighted;
    
    
    //Delegates & Events
    private Action TweenFinishedCallBack { get; set; }
    private  Action<IActiveBranch> SetAsActiveBranch { get; set; }
    private  Action<IGetHomeBranches> GetHomeBranches { get; set; }

    //Main
    private void Awake()
    {
        CheckForValidSetUp();
        ThisGroupsUiNodes = SetBranchesChildNodes.GetChildNodes(this);
        MyCanvasGroup = GetComponent<CanvasGroup>();
        MyCanvasGroup.blocksRaycasts = false;
        _uiTweener = GetComponent<UITweener>();
        MyCanvas = GetComponent<Canvas>();
        MyParentBranch = this;
        SetStartPositions();
        AutoOpenCloseClass = EJect.Class.WithParams<IAutoOpenClose>(this); 
    }

    private void CheckForValidSetUp()
    {
        if (AllowableInAndOutTweens(_stayVisible)) return;
        
        throw new Exception($"Can't have Stay Visible and also have IN AND OUT Tweens on : {this} " +
                  $"{Environment.NewLine} OutTween NOT Allowed");
    }

    private void SetStartPositions()
    {
        SetDefaultStartPosition();
        _lastHighlighted = DefaultStartOnThisNode;
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
    }

    private void FindStartPosition()
    {
        var childNodes = transform.GetComponentsInChildren<UINode>();
        
        if (childNodes.Length == 0)
            throw new Exception($"No child Nodes in {this}");
        
        _startOnThisNode = transform.GetComponentsInChildren<UINode>().First();
    }    

    private void OnEnable()
    {
        FetchEvents();
        ObserveEvents();
        _branchTypeClass = BranchFactory.Factory.PassThisBranch(this).CreateType(_branchType);
        _branchTypeClass.OnEnable();
        AutoOpenCloseClass.OnEnable();
    }

    public void FetchEvents()
    {
        SetAsActiveBranch = EVent.Do.Fetch<IActiveBranch>();
        GetHomeBranches = EVent.Do.Fetch<IGetHomeBranches>();
    }

    public void ObserveEvents()
    {
        EVent.Do.Subscribe<ISwitchGroupPressed>(SwitchBranchGroup);
        EVent.Do.Subscribe<IHighlightedNode>(SaveHighlighted);
        EVent.Do.Subscribe<ISelectedNode>(SaveSelected);
        EVent.Do.Subscribe<IOnHomeScreen>(SaveIfOnHomeScreen);
        EVent.Do.Subscribe<IStartBranch>(StartBranch_GOUIEventCall);
        EVent.Do.Subscribe<ICloseBranch>(ExitBranch_GOUIEventCall);
    }

    private void Start() => CheckForControlBar();

    private void CheckForControlBar()
    {
        GetHomeBranches?.Invoke(this);
        BranchGroups.AddControlBarToGroupList(_groupsList, HomeBranches, this);
    }

    private void StartBranch_GOUIEventCall(IStartBranch args)
    {
        if(!ReferenceEquals(args.TargetBranch, this)) return;
        if (_branchType == BranchType.InGameUi)
            _canStartGOUI = true;
        MoveToThisBranch();
    }
    
    private void ExitBranch_GOUIEventCall(ICloseBranch args)
    {
        if(!ReferenceEquals(args.TargetBranch, this)) return;
        if(!CanvasIsEnabled) return;
        StartBranchExitProcess(OutTweenType.Cancel);
    }

    public void StartBranch_InspectorCall()
    {
        if (IsInGameBranch())
        {
            Debug.Log("Can't Start GOUI like this: Call ActivateGOUI in GOUIModule");
            return;
        }
        MoveToThisBranch();
    }

    public void MoveToThisBranch(IBranch newParentBranch = null)
    {
        if(!_branchTypeClass.CanStartBranch()) return;
        
        _branchTypeClass.SetUpBranch(newParentBranch);
        SetHighlightedNode();
        
        if (_canActivateBranch) 
            SetAsActiveBranch?.Invoke(this);
        
        if (_tweenOnChange)
        {
            _uiTweener.StartInTweens(callBack: InTweenCallback);
        }
        else
        {
            InTweenCallback();
        }

        _tweenOnChange = true;
        _canStartGOUI = false;
    }
    
    private void InTweenCallback()
    {
        _branchTypeClass.EndOfBranchStart();
        
        if (_canActivateBranch)
            _lastHighlighted.SetNodeAsActive();

        _branchEvents.OnBranchEnter();
        _canActivateBranch = true;
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

        bool DontExitBranch() => _stayVisible == IsActive.Yes && outTweenType == OutTweenType.MoveToChild 
                                 || !_branchTypeClass.CanExitBranch();
    }

    private void StartOutTween(Action endOfTweenCallback = null)
    {
        TweenFinishedCallBack = endOfTweenCallback;
        
        _branchEvents.OnBranchExit();
        _branchTypeClass.StartBranchExit();
        _uiTweener.StartOutTweens(OutTweenCallback);
        
        void OutTweenCallback()
        {
            _branchTypeClass.EndOfBranchExit();
            TweenFinishedCallBack?.Invoke();
        }
    }

    private void SwitchBranchGroup(ISwitchGroupPressed args)
    {
        var cannotSwitchGroups = _onHomeScreen || !CanvasIsEnabled || _groupsList.Count <= 1;
        
        if (cannotSwitchGroups) return;
        _groupIndex = BranchGroups.SwitchBranchGroup(_groupsList, _groupIndex, args.SwitchType);
    }

    private void SetHighlightedNode()
    {
        if (_saveExitSelection == IsActive.Yes) return;
        
        _groupIndex = BranchGroups.SetGroupIndex(DefaultStartOnThisNode, _groupsList);
        _lastHighlighted = DefaultStartOnThisNode;
    }

    public void SetCanvas(ActiveCanvas activeCanvas) => _branchTypeClass.SetCanvas(activeCanvas);
    public void SetBlockRaycast(BlockRaycast blockRaycast) => _branchTypeClass.SetBlockRaycast(blockRaycast);
    public void SetUpAsTabBranch() => _branchTypeClass.SetUpAsTabBranch();

    public void OnPointerEnter(PointerEventData eventData) => AutoOpenCloseClass.OnPointerEnter();

    public void OnPointerExit(PointerEventData eventData) => AutoOpenCloseClass.OnPointerExit();
}

