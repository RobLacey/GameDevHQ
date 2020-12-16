using System;
using UnityEngine;

public interface IStartPopUp
{
    void StartPopUp();
}

public interface IBranch : IParameters, IAutoOpenCloseData
{
    bool IsPauseMenuBranch();
    bool IsAPopUpBranch();
    bool IsInternalBranch();
    bool IsHomeScreenBranch();
    bool IsTimedPopUp();
    
    
    event Action OnStartPopUp;
    INode DefaultStartOnThisNode { get; }
    Canvas MyCanvas { get; } 
    CanvasGroup MyCanvasGroup { get; }
    ScreenType ScreenType { get; set; }
    EscapeKey EscapeKeyType { get; set; }
    WhenToMove WhenToMove { set; }
    bool CanvasIsEnabled { get; }
    bool CanStoreAndRestoreOptionalPoUp { get; }
    DoTween TweenOnHome { get; set; }
    IBranch ThisBranch { get; }
    IBranch MyParentBranch { get; set; }
    float Timer { get; }
    INode[] ThisGroupsUiNodes { get; }
    INode LastSelected { get; }
    GameObject ThisBranchesGameObject { get; }
    bool PointerOverBranch { get;}
    IAutoOpenClose AutoOpenCloseClass { get; }
    IsActive BlockOtherNode { get; set; }
    IsActive SetStayOn { get; set; }
    
    IBranch[] FindAllBranches();
    IsActive GetStayOn();
    void MoveToThisBranch(IBranch newParentBranch = null);
    void DontSetBranchAsActive();
    void SetHighlightedNode();
    void DoNotTween();
    void StartBranchExitProcess(OutTweenType outTweenType, Action endOfTweenCallback = null);
    void SetCanvas(ActiveCanvas activeCanvas);
    void SetBlockRaycast(BlockRaycast blockRaycast);
    void SetUpAsTabBranch();
}

public interface IAutoOpenClose
{
    void OnEnable();
    void OnDisable();
    bool CanAutoClose();
    bool CanAutoOpen();
    void OnPointerEnter();
    void OnPointerExit();
    bool PointerOverBranch { get;}
    IBranch ChildNodeHasOpenChild { set; }
}

public interface IAutoOpenCloseData : IParameters
{
    IBranch ThisBranch { get; }
    AutoOpenClose AutoOpenClose { get; set; }
}

