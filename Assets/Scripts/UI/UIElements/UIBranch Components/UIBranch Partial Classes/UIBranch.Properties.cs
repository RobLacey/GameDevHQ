using NaughtyAttributes;
using UnityEngine;

/// <summary>
/// This partial class holds all the classes properties as UIBranch is an important link between higher and lower parts of the system
/// </summary>
public partial class UIBranch : ICancelHoverOver
{
    public bool IsAPopUpBranch()
    {
        return _branchType == BranchType.OptionalPopUp
               || _branchType == BranchType.ResolvePopUp;
    }
    public bool IsPauseMenuBranch() => _branchType == BranchType.PauseMenu;
    public bool IsInternalBranch() => _branchType == BranchType.Internal;
    public bool IsHomeScreenBranch() => _branchType == BranchType.HomeScreen;
    public BranchType ReturnBranchType => _branchType;
    public bool IsTimedPopUp() => _branchType == BranchType.TimedPopUp;
    public bool CanvasIsEnabled => MyCanvas.enabled;
    public bool CanStoreAndRestoreOptionalPoUp => _storeOrResetOptional == StoreAndRestorePopUps.StoreAndRestore;
    public INode DefaultStartOnThisNode => _startOnThisNode;
    private INode LastHighlighted { get; set; }
    public INode LastSelected { get; private set; }
    public INode[] ThisGroupsUiNodes { get; private set; }
    public Canvas MyCanvas { get; private set; } 
    public CanvasGroup MyCanvasGroup { get; private set; }
    public IBranchBase BranchBase { get; private set; }
    public IBranch MyParentBranch { get; set; }
    public IBranch ThisBranch => this;
    public GameObject ThisBranchesGameObject => gameObject;
    public IBranch ActiveBranch => this;


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
    
    public void SetStayOn(IsActive setting) => _stayVisible = setting;
    public IsActive GetStayOn() => _stayVisible;

    public float Timer => _timer;

    //Editor Properties
    public bool IsOptional() => _branchType == BranchType.OptionalPopUp;

    public bool IsStored() =>
        _branchType == BranchType.OptionalPopUp && _storeOrResetOptional == StoreAndRestorePopUps.StoreAndRestore; 
    public bool IsEmpty(UINode node) => node != null;

    public bool IsFullScreen()
    {
        if (_screenType != ScreenType.FullScreen) return false;
        
        _stayVisible = IsActive.No;
        return true;
    }

}