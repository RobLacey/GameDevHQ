﻿
public enum UIEventTypes
{
    Normal, Highlighted, Selected, Pressed, Cancelled
}

public enum UIGroupID
{
    Group1, Group2, Group3, Group4, Group5
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
    NoTween, Scale_InOnly, Scale_OutOnly, Scale_InAndOut, Punch, Shake 
}

public enum FadeTween 
{ 
    NoTween, FadeIn, FadeOut, FadeInAndOut 
}

public enum EffectType { In, Out, Both }

public enum ToggleGroup { None, TG1, TG2, TG3, TG4, TG5 }

public enum EscapeKey { BackOneLevel, BackToRootLevel, GlobalSetting, None }

public enum PreserveSelection { Never_TempSwitch, Standard, ToggleGroup_AllOff, ToggleGroup_OneAlwaysOn, Toggle_NotLinked }

public enum Setting
{
    None = 0,
    NavigationAndOnClick = 1 << 0,
    Colours = 1 << 1,
    Size = 1 << 2,
    Invert = 1 << 3,
    Swap = 1 << 4,
    Accessories = 1 << 5,
    Audio = 1 << 6,
    TooplTip = 1 << 7
}

public enum NavigationType { RightAndLeft, UpAndDown, AllDirections, None }

public enum EventType { Never, Highlighted, Selected }

public enum ScaleType { PositionTween, ScaleTween, ScalePunch, ScaleShake }

public enum Choose { None, Highlighted, HighlightedAndSelected, Selected, Pressed };

public enum ToolTipAnchor
{
    Centre, middleLeft, middleRight, MiddleTop,
    MiddleBottom, TopLeft, TopRight, BottomLeft, BottomRight
}

public enum TooltipType { Fixed, Follow }

public enum ColourSettings
{
    None = 0,
    Highlighted = 1 << 0,
    Selected = 1 << 1,
    Pressed = 1 << 2,
}

public enum TweenColours
{
    None = 0,
    Images = 1 << 0,
    Text = 1 << 1,
}

public enum DestinationAs { StartTweenAt, MidPointForInAndOut, EndTweenAt }
public enum CurrentRoatationIs { StartRotateAt, MidPointForInAndOut, EndRotateAt }







