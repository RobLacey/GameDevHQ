// ReSharper disable UnusedMember.Local
public partial class UINode
{
    private bool SetChildBranch(ButtonFunction buttonFunction)
    {
        _navigation.NotAToggle = buttonFunction == ButtonFunction.ToggleGroup
                                 || buttonFunction == ButtonFunction.ToggleNotLinked;

        return true;
    }

    private bool UseNavigation() => (_enabledFunctions & Setting.NavigationAndOnClick) != 0;
    private bool NeedColour() => (_enabledFunctions & Setting.Colours) != 0;
    private bool NeedSize() => (_enabledFunctions & Setting.SizeAndPosition) != 0;
    private bool NeedInvert() => (_enabledFunctions & Setting.InvertColourCorrection) != 0;
    private bool NeedSwap() => (_enabledFunctions & Setting.SwapImageOrText) != 0;
    private bool NeedAccessories() => (_enabledFunctions & Setting.Accessories) != 0;
    private bool NeedAudio() => (_enabledFunctions & Setting.Audio) != 0;
    private bool NeedTooltip() => (_enabledFunctions & Setting.TooplTip) != 0;
    private bool NeedEvents() => (_enabledFunctions & Setting.Events) != 0;
    private bool GroupSettings() => _buttonFunction != ButtonFunction.ToggleGroup;
}