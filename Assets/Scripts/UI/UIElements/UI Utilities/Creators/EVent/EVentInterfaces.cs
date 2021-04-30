using System;
using System.Collections.Generic;
using UnityEngine;
using UIElements;using UnityEngine.UIElements;

public interface IReturnToHome { }// This one is test
public interface IPausePressed { } // This one is test

public interface ICancelPressed // This one is test
{
    EscapeKey EscapeKeySettings { get; }
}

public interface ISwitchGroupPressed // This one is test
{
    SwitchType SwitchType { get; }
}


public interface IChangeControlsPressed { } // This one is test

public interface IActivateBranchOnControlsChange
{
    IBranch ActiveBranch { get; }
}

public interface IAllowKeys // This one is test
{
    bool CanAllowKeys { get; }
} 

public interface ICancelButtonActivated: ICancelPopUp
{
    EscapeKey EscapeKeyType { get; }
} 
public interface ICancelPopUp
{
    IBranch MyBranch { get; }
}

public interface ICancelHoverOver
{
    EscapeKey EscapeKeyType { get; }
}
public interface ICancelHoverOverButton { }

public interface IMenuGameSwitchingPressed { }

public interface IGameIsPaused
{
    bool GameIsPaused { get; }
}

public interface IHighlightedNode
{
    INode Highlighted { get; }
}

public interface ISelectedNode
{
    INode UINode { get; }
}

public interface IActiveBranch
{
    IBranch ActiveBranch { get; }
}

public interface ISetUpStartBranches 
{
    IBranch StartBranch { get; }
} 
public interface IOnStart { }

public interface IOnHomeScreen
{
    bool OnHomeScreen { get; }
}

public interface IClearScreen 
{
    IBranch IgnoreThisBranch { get; }
}

public interface IInMenu
{
    bool InTheMenu { get; }
}

public interface INoResolvePopUp // This one is test
{
    bool ActiveResolvePopUps { get; }
}

public interface INoPopUps // This one is test
{
    bool NoActivePopUps { get; }
}

public interface IRemoveOptionalPopUp // This one is test
{
    IBranch ThisPopUp { get; }
}

public interface ILastRemovedPopUp
{
    IBranch LastOptionalPopUp { get; }
}

public interface IAddOptionalPopUp // This one is test
{
    IBranch ThisPopUp { get; }
}

public interface IAddResolvePopUp // This one is test
{
    IBranch ThisPopUp { get; }
}

public interface IHotKeyPressed
{
    INode ParentNode { get; }
    IBranch MyBranch { get; }
}

public interface IDisabledNode
{
    INode ToThisDisabledNode { get; }
    bool IsDisabled { get; set; }
    bool IsThisNodeIsDisabled();
}

public interface ISceneChange { }

public interface IEndTween
{
    RectTransform EndTweenRect { get; }
    TweenScheme Scheme { get; }
}

public interface IGetHomeBranches
{
    List<UIBranch> HomeBranches { set; }
}

public interface IClearAll { }

public interface ISetUpUIGOBranch
{
    IBranch TargetBranch  { get; }
    GOUIModule ReturnGOUIModule { get; }
    Transform GOUITransform { get; }
}

public interface IStartGOUIBranch
{
    IBranch TargetBranch { get; }
    GOUIModule ReturnGOUIModule { get; }
}

public interface ICloseAndResetBranch
{
    IBranch TargetBranch { get; }
    GOUIModule ReturnGOUIModule { get; }
}

public interface ICloseGOUIModule
{
    IBranch TargetBranch { get; }
}

public interface IOffscreen
{
    IBranch TargetBranch { get; }
    bool IsOffscreen { get; }
}

public interface ISetStartingCanvasOrder
{
    ISetStartingCanvasOrder ReturnCanvasOrderData { get; }
    int ReturnPresetCanvasOrder(CanvasOrderCalculator canvasOrderCalculator);
    int ReturnToolTipCanvasOrder();
    int ReturnVirtualCursorCanvasOrder();
    int ReturnOffScreenMarkerCanvasOrder();
    int ReturnControlBarCanvasOrder();
}

public interface IReturnHomeGroupIndex
{
    INode TargetNode { set; }
}

public interface IVCSetUpOnStart
{
    bool ShowCursorOnStart { get; }
}

public interface IAddNewBranch
{
    IBranch MyBranch { get; }
}
public interface IRemoveBranch
{
    IBranch MyBranch { get; }
}

public interface IVcChangeControlSetUp{ }



