using UnityEngine;

public partial class UIBranch
{
    private const string CanAutoOpenClose = nameof(CheckAutoOpenCloseStatus);
    private const string HomeScreenBranch = nameof(IsHomeScreenBranch);
    private const string StandardBranch = nameof(IsStandardBranch);
    private const string ControlBarBranch = nameof(IsControlBar);
    private const string OptionalBranch = nameof(IsOptional);
    private const string InGamUIBranch = nameof(InGameUI);
    private const string IsPauseMenu = nameof(PauseMenu);
    private const string TimedBranch = nameof(IsTimedPopUp);
    private const string ResolveBranch = nameof(IsResolve);
    private const string AnyPopUpBranch = nameof(IsAPopUpEditor);
    private const string HomeScreenButNotControl = nameof(IsHomeAndNotControl);
    private const string Stored = nameof(IsStored);
    private const string Fullscreen = nameof(IsFullScreen);
    private const string ShowManualOrder = nameof(IsManualOrder);
    private const string ValidInAndOutTweens = nameof(AllowableInAndOutTweens);
    private const string SetUpCanvasOrder = nameof(ChangeCanvasOrder);
    private bool CheckAutoOpenCloseStatus() => !IsAPopUpBranch() && !IsPauseMenuBranch() && !IsTimedPopUp();

    private void ChangeCanvasOrder()
    {
        var temp = GetComponent<Canvas>();
        temp.overrideSorting = true;
        temp.sortingOrder = _orderInCanvas;
    }

    private bool InGameUI => _branchType == BranchType.InGameObject;
    private bool PauseMenu => _branchType == BranchType.PauseMenu;
    private bool IsManualOrder => _canvasOrderSetting == OrderInCanvas.Manual 
                                  && (IsStandardBranch() || IsInternalBranch() || IsHomeScreenBranch());
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
        if (active == IsActive.Yes)
        {
            var tweener = GetComponent<UITweener>();
            if (tweener.HasInAndOutTween())
            {
                return false;
            }
        }

        return true;
    }

    private const string MessageINAndOutTweens = "Can't have IN And Out tweens and Stay Visible set";
}