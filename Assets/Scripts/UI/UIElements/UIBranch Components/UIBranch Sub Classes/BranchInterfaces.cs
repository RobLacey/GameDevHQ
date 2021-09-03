using System;
using System.Collections.Generic;
using EZ.Inject;
using UIElements;
using UnityEngine;


public interface IBranch : IParameters, IAutoOpenCloseData, ICanvasOrder, IMonoDisable, IMonoEnable, IMonoOnDestroy,
                           IDynamicBranch
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
    IsActive AlwaysHighlighted { get; }
    INode[] ThisGroupsUiNodes { get; }
    bool PointerOverBranch { get;}
    IAutoOpenClose AutoOpenCloseClass { get; }
    IsActive SetSaveLastSelectionOnExit { set; }
    BranchType ReturnBranchType { get; }
    IsActive SetStayOn { set; }
    INode LastSelected { get; }
    INode LastHighlighted { get; }
    GameObject ThisBranchesGameObject { get; }
    IsActive ReturnOnlyAllowOnHomeScreen { get; }
    List<GroupList> BranchGroupsList { get; }
    int GroupIndex { get; set; }
    GOUIModule ReturnGOUIModule { get; }


    IsActive GetStayOn();
    void SetNotAControlBar();
    void StartPopUp_RunTimeCall(bool fromPool);
    void MoveToThisBranch(IBranch newParentBranch = null);
    void SetBranchAsActive();
    void DontSetBranchAsActive();
    void DoNotTween();
    void StartBranchExitProcess(OutTweenType outTweenType, Action endOfTweenCallback = null);
    void SetCanvas(ActiveCanvas activeCanvas);
    void SetBlockRaycast(BlockRaycast blockRaycast);
    void SetUpAsTabBranch();
    void SetUpGOUIBranch(IGOUIModule module);
}

public interface IAutoOpenClose : IMonoEnable, IMonoDisable
{
    void OnPointerEnter();
    void OnPointerExit();
    bool PointerOverBranch { get;}
    IBranch ChildNodeHasOpenChild { set; }
}

public interface IAutoOpenCloseData : IThisBranch
{
    EscapeKey EscapeKeyType { get; }
    IsActive AutoClose { get; }
    float AutoCloseDelay { get; }
}

public interface IThisBranch
{
    IBranch ThisBranch { get; }
}

public interface ICanvasOrder
{
    OrderInCanvas CanvasOrder { get; set; }
    int ReturnManualCanvasOrder { get; }
    Canvas MyCanvas { get; }
}

public interface IDynamicBranch : IThisBranch
{
    IsActive GetSaveOnExit { get; }
    INode DefaultStartOnThisNode { get; set; }
    INode LastSelected { get; set; }
    INode LastHighlighted { get; set; }
    INode[] ThisGroupsUiNodes { get; set; }
    void SetThisGroupsNode(INode[] groupsNodes);
    bool IsInGameBranch();
}



