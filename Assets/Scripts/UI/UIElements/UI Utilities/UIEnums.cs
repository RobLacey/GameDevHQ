
using System;


public enum UIEventTypes
{
    Normal, Highlighted, Selected, Pressed, Cancelled
}

public enum IsActive
{
    Yes, No
}

public enum PositionTweenType 
{ 
    NoTween, In, Out, InAndOut
}

public enum RotationTweenType 
{ 
    NoTween, In, Out, InAndOut
}

public enum ScaleTween 
{ 
    NoTween, Scale_InOnly, Scale_OutOnly, Scale_InAndOut 
}

public enum PunchShakeTween 
{ 
    NoTween, Punch, Shake 
}

public enum FadeTween 
{ 
    NoTween, FadeIn, FadeOut, FadeInAndOut 
}

public enum EffectType { In, Out, Both }

public enum ToggleGroup { None, TG1, TG2, TG3, TG4, TG5 }

public enum EscapeKey { None, BackOneLevel, BackToHome, GlobalSetting }

public enum ButtonFunction { Standard, ToggleGroup, ToggleNotLinked, HoverToActivate, CancelOrBack }

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
    TooplTip = 1 << 7,
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

public enum TooltipType { Fixed, Follow }

[Flags]
public enum EventType
{
    None = 0,
    Highlighted = 1 << 0,
    Selected = 1 << 1,
    Pressed = 1 << 2,
}

public enum ScreenType { Normal, FullScreen }

public enum StartInMenu { InMenu, InGameControl }
public enum BranchType { HomeScreen, Standard, ResolvePopUp, OptionalPopUp, TimedPopUp, PauseMenu, Internal }

public enum WhenToMove { Immediately, AfterEndOfTween }
public enum PauseOptionsOnEscape { EnterPauseOrEscapeMenu, DoNothing }
public  enum ControlMethod { MouseOnly, KeysOrControllerOnly, AllowBothStartWithMouse, AllowBothStartWithKeys }
public enum SwitchType { Positive, Negative }
public enum TweenType { In, Out }
public enum BlockRayCast { Yes, No }
public enum OutTweenType { Cancel, MoveToChild }
public enum UseSide { ToTheRightOf, ToTheLeftOf, GameObjectAsPosition  }
public enum InGameSystem { On, Off }



