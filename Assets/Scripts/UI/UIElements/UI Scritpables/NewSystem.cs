using UnityEngine;

[CreateAssetMenu(menuName = "New Input Scheme - New", fileName = "Scheme - New")]
public class NewSystem : InputScheme
{
    public override ControlMethod ControlType { get; }
    public override PauseOptionsOnEscape PauseOptions { get; }
    protected override string PauseButton { get; }
    protected override string PositiveSwitch { get; }
    protected override string NegativeSwitch { get; }
    protected override string CancelButton { get; }
    protected override string MenuToGameSwitch { get; }
    public override InGameSystem InGameMenuSystem { get; }
    public override StartInMenu WhereToStartGame { get; }
    public override EscapeKey GlobalCancelAction { get; }
    public override float StartDelay { get; }
    public override bool MouseClicked { get; }
    public override bool CanSwitchToKeysOrController { get; }
    public override bool CanSwitchToMouse { get; }

    public override Vector3 SetMousePosition()
    {
        return Vector3.zero;
    }

    public override void OnAwake()
    {
        Debug.Log("New Scheme");
    }

    public override void TurnOffInGameMenuSystem()
    {
        
    }

    public override bool PressPause()
    {
        Debug.Log("Pressed Pause");
        return true;
    }

    public override bool PressedMenuToGameSwitch()
    {
        Debug.Log("Pressed In Game Switch");
        return true;
    }

    public override bool PressedCancel()
    {
        Debug.Log("Pressed Cancel");
        return true;
    }

    public override bool PressedPositiveSwitch()
    {
        Debug.Log("Pressed Positive Switch");
        return true;
    }

    public override bool PressedNegativeSwitch()
    {
        Debug.Log("Pressed Negative Switch");
        return true;
    }

    public override bool HotKeyChecker(HotKey hotKey)
    {
        Debug.Log("Hot Key Pressed");
        return true;

    }

}