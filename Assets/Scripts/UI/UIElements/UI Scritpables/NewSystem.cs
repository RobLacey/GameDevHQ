using UnityEngine;

[CreateAssetMenu(menuName = "UIElements Schemes / New Input Scheme - New", fileName = "Scheme - New")]
public class NewSystem : InputScheme
{
    protected override string PauseButton { get; } = " ";
    protected override string PositiveSwitch { get; } = " ";
    protected override string NegativeSwitch { get; } = " ";
    protected override string PositiveGOUISwitch { get; } = " ";
    protected override string NegativeGOUISwitch { get; } = " ";
    protected override string CancelButton { get; } = " ";
    protected override string MenuToGameSwitch { get; } = " ";
    protected override string VCursorHorizontal { get; } = " ";
    protected override string VCursorVertical { get; } = " ";
    protected override string SelectedButton { get; } = " ";
    public override bool MouseClicked { get; } = false;
    public override bool CanSwitchToKeysOrController { get; } = false;
    public override bool CanSwitchToMouse { get; } = false;

    public override void SetMousePosition()
    {
        Debug.Log("Old Mouse Position set");
    }

    protected override void SetUpUInputScheme()
    {
        Debug.Log("New Scheme");
    }

    public override bool PressPause()
    {
        return false;
    }

    public override bool PressedMenuToGameSwitch()
    {
        return false;
    }

    public override bool PressedCancel()
    {
        return false;
    }

    public override bool PressedPositiveSwitch()
    {
        return false;
    }

    public override bool PressedNegativeSwitch()
    {
       return false;
    }

    public override bool PressedPositiveGOUISwitch()
    {
        return false;
    }

    public override bool PressedNegativeGOUISwitch()
    {
        return false;
    }

    public override float VcHorizontal()
    {
        return 0;
    }

    public override float VcVertical()
    {
        return 0;
    }

    public override bool PressSelect()
    {
        return false;
    }

    public override bool HotKeyChecker(HotKey hotKey)
    {
        return false;

    }

}