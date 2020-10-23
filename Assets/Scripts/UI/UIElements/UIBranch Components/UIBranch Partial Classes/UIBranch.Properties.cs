using UnityEngine;

/// <summary>
/// This partial class holds all the classes properties as UIBranch is an important link between higher and lower parts of the system
/// </summary>
public partial class UIBranch
{
    public bool IsAPopUpBranch()
    {
        return _branchType == BranchType.OptionalPopUp
               || _branchType == BranchType.ResolvePopUp
               || _branchType == BranchType.TimedPopUp;
    }
    public bool IsPauseMenuBranch() => _branchType == BranchType.PauseMenu;
    public bool IsInternalBranch() => _branchType == BranchType.Internal;
    public bool IsHomeScreenBranch() => _branchType == BranchType.HomeScreen;
    public bool IsTimedPopUp => _branchType == BranchType.TimedPopUp;
    public bool CanvasIsEnabled => MyCanvas.enabled;
    public bool CanStoreAndRestoreOptionalPoUp => _storeOrResetOptional == StoreAndRestorePopUps.StoreAndRestore;
    public UINode DefaultStartOnThisNode => _startOnThisNode;
    private INode LastHighlighted { get; set; }
    public INode LastSelected { get; private set; }
    public UINode[] ThisGroupsUiNodes { get; private set; }
    public Canvas MyCanvas { get; private set; }
    public CanvasGroup MyCanvasGroup { get; private set; }
    public BranchBase Branch { get; private set; }
    public UIBranch MyParentBranch { get; set; }

    public EscapeKey EscapeKeySetting
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

    public IsActive TweenOnHome
    {
        get => _tweenOnHome;
        set => _tweenOnHome = value;
    }

    public void SetStayOn(IsActive setting) => _stayOn = setting;

    public float Timer => _timer;

    //Editor Properties
    public bool IsOptional() => _branchType == BranchType.OptionalPopUp;

    public bool IsStored() =>
        _branchType == BranchType.OptionalPopUp && _storeOrResetOptional == StoreAndRestorePopUps.StoreAndRestore; 
    public bool IsEmpty(UINode node) => node != null;

}