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
    protected override string SwitchToVC { get; } = " ";
    protected override string MouseXAxis { get; } = " ";
    protected override string MouseYAxis { get; } = " ";
    protected override string SelectedButton { get; } = " ";
    public override bool AnyMouseClicked { get; } = false;
    public override bool LeftMouseClicked { get; } = false;
    public override bool RightMouseClicked { get; } = false;

    public override bool CanSwitchToMouseOrVC(bool allowKeys)
    {
        return false;
    }
    public override bool CanSwitchToKeysOrController(bool allowKeys)
    {
        return false;
    }

    public override Vector3 GetMouseOrVcPosition()
    {
        return Vector3.zero;
    }

    public override void SetVirtualCursorPosition(Vector3 pos)
    {
        Debug.Log("Set VC Position");
    }

    private protected override Vector3 GetVirtualCursorPosition()
    {
        return Vector3.zero;
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

    public override bool VcHorizontalPressed()
    {
        return false;
    }

    public override bool VcVerticalPressed()
    {
        return false;
    }

    private protected override bool VCSwitchTo()
    {
        return false;
    }

    public override bool PressSelect()
    {
        return false;
    }

    public override bool HotKeyChecker(HotKey hotKey)
    {
        return false;
    }

    public override float MouseXAxisValue()
    {
        return 0;
    }

    public override float MouseYAxisValue()
    {
        return 0;
    }
}