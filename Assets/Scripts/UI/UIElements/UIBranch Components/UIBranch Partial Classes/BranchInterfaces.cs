using System;
using UnityEngine;

public interface IStartPopUp
{
    void StartPopUp();
}

public interface IBranch : IParameters
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
    void SetStayOn(IsActive setting);
    IsActive GetStayOn();
    DoTween TweenOnHome { get; set; }
    IBranch ThisBranch { get; }
    IBranchBase BranchBase { get; }
    IBranch MyParentBranch { get; set; }
    float Timer { get; }
    IBranch[] FindAllBranches();
    INode[] ThisGroupsUiNodes { get; }
    INode LastSelected { get; }
    GameObject ThisBranchesGameObject { get; }
    
    bool OpenHooverOnEnter { get; }
    bool CloseHooverOnExit { get; set; }
    bool PointerOverBranch { get; }


    void NavigateToChildBranch(IBranch moveToo);
    void MoveToThisBranch(IBranch newParentBranch = null);
    void DontSetBranchAsActive();
    void SetHighlightedNode();
    void DoNotTween();
    void StartBranchExitProcess(OutTweenType outTweenType, Action endOfTweenCallback = null);
}
