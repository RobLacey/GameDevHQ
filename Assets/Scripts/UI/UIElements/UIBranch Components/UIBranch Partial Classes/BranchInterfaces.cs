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
    EscapeKey EscapeKeySetting { get; set; }
    WhenToMove WhenToMove { set; }
    bool CanvasIsEnabled { get; }
    bool CanStoreAndRestoreOptionalPoUp { get; }
    void SetStayOn(IsActive setting);
    IsActive TweenOnHome { get; set; }
    IBranch ThisBranch { get; }
    IBranchBase BranchBase { get; }
    IBranch MyParentBranch { get; set; }
    float Timer { get; }
    IBranch[] FindAllBranches();
    INode[] ThisGroupsUiNodes { get; }
    INode LastSelected { get; }


    void NavigateToChildBranch(IBranch moveToo);
    void MoveToThisBranch(IBranch newParentBranch = null);
    void MoveToBranchWithoutTween();
    void DontSetBranchAsActive();
    void ResetBranchesStartPosition();
    void DoNotTween();
    void StartBranchExitProcess(OutTweenType outTweenType, Action endOfTweenCallback = null);
}
