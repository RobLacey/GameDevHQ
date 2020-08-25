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

    public bool IsTimedPopUp => _branchType == BranchType.TimedPopUp;
    public UINode DefaultStartOnThisNode => _startOnThisNode;
    public Canvas MyCanvas { get; private set; }
    public CanvasGroup MyCanvasGroup { get; private set; }
    public BranchBase Branch { get; private set; }
    public UINode LastHighlighted { get; private set; }
    public UINode LastSelected { get; private set; }
    public UIBranch MyParentBranch { get; private set; }
    public UINode[] ThisGroupsUiNodes { get; private set; }
    public bool CanvasIsEnabled => MyCanvas.enabled;
    public bool CanStoreAndRestoreOptionalPoUp => _clearOrResetOptional == StoreAndRestorePopUps.StoreAndRestore;
    public BranchType MyBranchType => _branchType;
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

}