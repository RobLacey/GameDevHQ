using System;
using UIElements;
using UnityEngine;


public interface IBranch : IParameters, IAutoOpenCloseData, ICanvasOrder
{
    bool IsControlBar();
    bool IsPauseMenuBranch();
    bool IsAPopUpBranch();
    bool IsInternalBranch();
    bool IsHomeScreenBranch();
    bool IsTimedPopUp();
    bool IsInGameBranch();
    
    INode DefaultStartOnThisNode { get; }
    CanvasGroup MyCanvasGroup { get; }
    ScreenType ScreenType { get; set; }
    EscapeKey EscapeKeyType { get; set; }
    WhenToMove WhenToMove { set; }
    bool CanvasIsEnabled { get; }
    bool CanStoreAndRestoreOptionalPoUp { get; }
    DoTween TweenOnHome { get; set; }
    IBranch MyParentBranch { get; set; }
    float Timer { get; }
    INode[] ThisGroupsUiNodes { get; }
    bool PointerOverBranch { get;}
    IAutoOpenClose AutoOpenCloseClass { get; }
    IsActive BlockOtherNode { get; set; }
    void ResetSavePositionOnExit();
    BranchType ReturnBranchType { get; }
    IsActive SetStayOn { set; }
    INode LastSelected { get; }
    GameObject ThisBranchesGameObject { get; }
    IsActive ReturnOnlyAllowOnHomeScreen { get; }
    IsActive AlwaysOn { get; }


    
    IBranch[] FindAllBranches();
    IsActive GetStayOn();
    void MoveToThisBranch(IBranch newParentBranch = null);
    void DontSetBranchAsActive();
    void DoNotTween();
    void StartBranchExitProcess(OutTweenType outTweenType, Action endOfTweenCallback = null);
    void SetCanvas(ActiveCanvas activeCanvas);
    void SetBlockRaycast(BlockRaycast blockRaycast);
    void SetUpAsTabBranch();
}

public interface IAutoOpenClose
{
    void OnEnable();
    bool CanAutoClose();
    bool CanAutoOpen();
    void OnPointerEnter();
    void OnPointerExit();
    bool PointerOverBranch { get;}
    IBranch ChildNodeHasOpenChild { set; }
}

public interface IAutoOpenCloseData
{
    IBranch ThisBranch { get; }
    AutoOpenClose AutoOpenClose { get; set; }
}

public interface ICanvasOrder
{
    OrderInCanvas CanvasOrder { get; set; }
    int ReturnManualCanvasOrder { get; }
    Canvas MyCanvas { get; }
}

