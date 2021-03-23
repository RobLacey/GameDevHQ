using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// This partial class holds all the classes properties as UIBranch is an important link between higher and lower parts of the system
/// </summary>
public partial class UIBranch
{
    //Set / Getters
    public bool IsAPopUpBranch()
    {
        return _branchType == BranchType.OptionalPopUp
               || _branchType == BranchType.ResolvePopUp;
    }

    public bool IsControlBar() => _controlBar == IsActive.Yes;
    public bool IsPauseMenuBranch() => _branchType == BranchType.PauseMenu;
    public bool IsInternalBranch() => _branchType == BranchType.Internal;
    public bool IsHomeScreenBranch() => _branchType == BranchType.HomeScreen;
    public bool IsStandardBranch() => _branchType == BranchType.Standard;
    public bool IsInGameBranch() => _branchType == BranchType.InGameUi;

    public void DoNotTween() => _tweenOnChange = false;
    public void DontSetBranchAsActive() => _canActivateBranch = false;
    public IBranch[] FindAllBranches() => FindObjectsOfType<UIBranch>().ToArray<IBranch>(); //TODO Write Up this
    public bool IsTimedPopUp() => _branchType == BranchType.TimedPopUp;
    public IsActive GetStayOn() => _stayVisible;
    public IsActive SetStayOn { set => _stayVisible = value; }

    private void SaveIfOnHomeScreen(IOnHomeScreen args) => _onHomeScreen = args.OnHomeScreen;
    private void SaveHighlighted(IHighlightedNode args)
    {
        _lastHighlighted = NodeSearch.Find(args.Highlighted)
                                    .DefaultReturn(LastSelected)
                                    .RunOn(ThisGroupsUiNodes);
    }
    private void SaveSelected(ISelectedNode args)
    {
        LastSelected = NodeSearch.Find(args.UINode)
                                 .DefaultReturn(LastSelected)
                                 .RunOn(ThisGroupsUiNodes);
    }    


    
   //Properties
    public BranchType ReturnBranchType => _branchType;
    public bool CanvasIsEnabled => MyCanvas.enabled;
    public bool CanStoreAndRestoreOptionalPoUp => _storeOrResetOptional == StoreAndRestorePopUps.StoreAndRestore;
    public INode DefaultStartOnThisNode => _startOnThisNode;
    public INode LastSelected { get; private set; }
    public INode[] ThisGroupsUiNodes { get; private set; }
    public Canvas MyCanvas { get; private set; } 
    public CanvasGroup MyCanvasGroup { get; private set; }
    public IBranch MyParentBranch { get; set; }
    public IBranch ThisBranch => this;
    public GameObject ThisBranchesGameObject => gameObject;
    public IBranch ActiveBranch => this;
    public IAutoOpenClose AutoOpenCloseClass { get; private set; }
    public bool PointerOverBranch => AutoOpenCloseClass.PointerOverBranch;
    public List<UIBranch> HomeBranches { private get; set; }
    public float Timer => _timer;
    public IsActive SetSaveLastSelectionOnExit
    {
        set => _saveExitSelection = value;
    }


    public EscapeKey EscapeKeyType
    {
        get => _escapeKeyFunction;
        set => _escapeKeyFunction = value;
    }

    public WhenToMove WhenToMove
    {
        get => _moveType;
        set => _moveType = value;
    }

    public ScreenType ScreenType
    {
        get => _screenType;
        set => _screenType = value;
    }

    public DoTween TweenOnHome
    {
        get => _tweenOnHome;
        set => _tweenOnHome = value;
    }
    
    public AutoOpenClose AutoOpenClose
    {
        get => _autoOpenClose;
        set => _autoOpenClose = value;
    }
    
    public OrderInCanvas CanvasOrder
    {
        get => _canvasOrderSetting;
        set => _canvasOrderSetting = value;
    }
    
    public int ReturnManualCanvasOrder => _orderInCanvas;
    public IsActive ReturnOnlyAllowOnHomeScreen => _onlyAllowOnHomeScreen;
    public IsActive AlwaysOn => _alwaysOnUI; 

    //Editor Properties
    private const string HomeScreenBranch = nameof(IsHomeScreenBranch);
    private const string StandardBranch = nameof(IsStandardBranch);
    private const string ControlBarBranch = nameof(IsControlBar);
    private const string OptionalBranch = nameof(IsOptional);
    private const string InGamUIBranch = nameof(InGameUI);
    private const string TimedBranch = nameof(IsTimedPopUp);
    private const string ResolveBranch = nameof(IsResolve);
    private const string AnyPopUpBranch = nameof(IsAPopUpEditor);
    private const string HomeScreenButNotControl = nameof(IsHomeAndNotControl);
    private const string Stored = nameof(IsStored);
    private const string Fullscreen = nameof(IsFullScreen);
    private const string ManualOrder = nameof(IsManualOrder);
    private const string ValidInAndOutTweens = nameof(AllowableInAndOutTweens);
    private const string SetUpCanvasOrder = nameof(ChangeCanvasOrder);

    private void ChangeCanvasOrder()
    {
        var temp = GetComponent<Canvas>();
        temp.overrideSorting = true;
        temp.sortingOrder = _orderInCanvas;
    }
    private bool InGameUI => _branchType == BranchType.InGameUi;
    private bool IsManualOrder => _canvasOrderSetting == OrderInCanvas.Manual;
    private bool IsResolve => _branchType == BranchType.ResolvePopUp;

    private bool IsOptional() => _branchType == BranchType.OptionalPopUp;

    private bool IsStored() =>
        _branchType == BranchType.OptionalPopUp && _storeOrResetOptional == StoreAndRestorePopUps.StoreAndRestore; 

    private bool IsHomeAndNotControl() => _branchType == BranchType.HomeScreen && _controlBar == IsActive.No;
    private bool IsFullScreen()
    {
        if (_screenType != ScreenType.FullScreen) return false;
        
        _stayVisible = IsActive.No;
        return true;
    }
    private bool IsAPopUpEditor()
    {
        return _branchType == BranchType.OptionalPopUp
               || _branchType == BranchType.ResolvePopUp
               || _branchType == BranchType.TimedPopUp;
    }
    
    private bool AllowableInAndOutTweens(IsActive active)
    {
        if(active == IsActive.Yes)
        {
            var tweener = GetComponent<UITweener>();
            if(tweener.HasInAndOutTween())
            {
                return false;
            }        
        }
        return true;
    }

    private const string MessageINAndOutTweens = "Can't have IN And Out tweens and Stay Visible set";
}