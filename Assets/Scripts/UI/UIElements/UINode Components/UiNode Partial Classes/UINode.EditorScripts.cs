// ReSharper disable UnusedMember.Local

public partial class UINode
{
    private const string CanAutoOpenClose = nameof(CheckAutoOpenCloseStatus);
    private const string CancelOrBack = nameof(IsCancelOrBack);
    private const string ShowGroupSettings = nameof(GroupSettings);
    private const string HasNavigation = nameof(UseNavigation);
    private const string HasColour = nameof(NeedColour);
    private const string HasInvert = nameof(NeedInvert);
    private const string HasSize = nameof(NeedSize);
    private const string HasSwap = nameof(NeedSwap);
    private const string HasAccessories = nameof(NeedAccessories);
    private const string HasAudio = nameof(NeedAudio);
    private const string HasEvents = nameof(NeedEvents);
    private const string HasTooltip = nameof(NeedTooltip);

    private bool CheckAutoOpenCloseStatus()
    {
         return (_buttonFunction == ButtonFunction. ToggleGroup 
                 || _buttonFunction == ButtonFunction. Standard
                 || _buttonFunction == ButtonFunction.InGameUi) 
                 && _autoOpen == IsActive.Yes;
    }

    private bool UseNavigation()
    {
        _navigation.CantNavigate = SetIfCanNavigate();
        return (_enabledFunctions & Setting.NavigationAndOnClick) != 0;
    }
    private bool NeedColour() => (_enabledFunctions & Setting.Colours) != 0;
    private bool NeedSize() => (_enabledFunctions & Setting.SizeAndPosition) != 0;
    private bool NeedInvert() => (_enabledFunctions & Setting.InvertColourCorrection) != 0;
    private bool NeedSwap() => (_enabledFunctions & Setting.SwapImageOrText) != 0;
    private bool NeedAccessories() => (_enabledFunctions & Setting.Accessories) != 0;
    private bool NeedAudio() => (_enabledFunctions & Setting.Audio) != 0;
    private bool NeedTooltip() => (_enabledFunctions & Setting.ToolTip) != 0;
    private bool NeedEvents() => (_enabledFunctions & Setting.Events) != 0;
    private bool GroupSettings() => _buttonFunction != ButtonFunction.ToggleGroup;

}