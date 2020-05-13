
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
    Navigation = 1 << 0,
    Colours = 1 << 1,
    Size = 1 << 2,
    Invert = 1 << 3,
    Swap = 1 << 4,
    Accessories = 1 << 5,
    Audio = 1 << 6
}

public enum NavigationType { RightAndLeft, UpAndDown, AllDirections, None }

public enum EventType { Never, Highlighted, Selected }

public enum ScaleType { ScaleUp, ScaleDown, Punch, Shake }

public enum Choose { None, Highlighted, HighlightedAndSelected, Selected, Pressed };







