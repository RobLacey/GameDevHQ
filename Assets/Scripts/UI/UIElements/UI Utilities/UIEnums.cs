
using System;


public enum IsActive
{
    Yes, No
}

public enum TweenStyle 
{ 
    NoTween, In, Out, InAndOut 
}

public enum ToggleGroup { TG1, TG2, TG3, TG4, TG5 }

public enum EscapeKey { None, BackOneLevel, BackToHome, GlobalSetting }

public enum ButtonFunction { Standard, ToggleGroup, ToggleNotLinked, CancelOrBack, InGameUi }

[Flags]
public enum Setting
{
    None = 0,
    NavigationAndOnClick = 1 << 0,
    Colours = 1 << 1,
    SizeAndPosition = 1 << 2,
    InvertColourCorrection = 1 << 3,
    SwapImageOrText = 1 << 4,
    Accessories = 1 << 5,
    Audio = 1 << 6,
    ToolTip = 1 << 7,
    Events = 1 << 8
}

public enum NavigationType { RightAndLeft, UpAndDown, AllDirections, None }

[Flags]
public enum AccessoryEventType
{
    None = 0,
    Highlighted = 1 << 0,
    Selected = 1 << 1
}

public enum TweenEffect { Position, Scale, Punch, Shake }

public enum Choose { None, Highlighted, HighlightedAndSelected, Selected, Pressed };

public enum ToolTipAnchor
{
    Centre, MiddleLeft, MiddleRight, MiddleTop,
    MiddleBottom, TopLeft, TopRight, BottomLeft, BottomRight
}

public enum TooltipType { FixedPosition, Follow }

[Flags]
public enum EventType
{
    None = 0,
    Highlighted = 1 << 0,
    Selected = 1 << 1,
    Pressed = 1 << 2,
}

public enum ScreenType { Normal, FullScreen }

public enum InMenuOrGame { InMenu, InGameControl }
public enum BranchType { HomeScreen, Standard, ResolvePopUp, OptionalPopUp, 
                         TimedPopUp, PauseMenu, Internal, InGameObject }

public enum WhenToMove { Immediately, AfterEndOfTween }
public enum PauseOptionsOnEscape { EnterPauseOrEscapeMenu, DoNothing }
public  enum ControlMethod { MouseOnly, KeysOrControllerOnly, AllowBothStartWithMouse, AllowBothStartWithKeys }
public enum SwitchType { Positive, Negative, Activate }
public enum TweenType { In, Out }
public enum OutTweenType { Cancel, MoveToChild }
public enum UseSide { ToTheRightOf, ToTheLeftOf, ToTheTopOf, ToTheBottomOf, CentreOf  }
public enum InGameSystem { On, Off }

public enum HotKey {  HotKey1, HotKey2, HotKey3, HotKey4, HotKey5, HotKey6, HotKey7, HotKey8, HotKey9, HotKey0 }

[Flags]
public enum ActivateWhen
{
    None = 0,
    OnHighlighted = 1 << 0,
    OnSelected = 2 <<1
}
public enum ChangeWhen { OnHighlight, OnPressed, OnControlChanged }

public enum BlockRaycast { Yes, No }
public enum ActiveCanvas { Yes, No }

public enum ActivateNodeOnReturnHome { Yes, No }

public enum DoTween { Tween, DoNothing }

public enum AutoOpenClose { No, Open, Close, Both }

public enum StoreAndRestorePopUps { StoreAndRestore, Reset }

public enum OrderInCanvas { InFront, Behind, Manual, Default }


public enum VirtualControl
{
    Yes, No
}
    
public enum GameType
{
    _2D, _3D, NoRestrictions
}

public enum CancelClickLocation
{
    Never, Left, Right, Either
}


