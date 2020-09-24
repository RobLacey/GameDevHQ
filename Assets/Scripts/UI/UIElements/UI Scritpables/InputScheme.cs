using NaughtyAttributes;
using UnityEngine;

public abstract class InputScheme : ScriptableObject
{
    [SerializeField] protected ControlMethod _mainControlType = ControlMethod.MouseOnly;
    [SerializeField] [Header("In Game System")] [Space(10f)]
    protected InGameSystem _inGameMenuSystem = InGameSystem.Off;
    [SerializeField] [EnableIf("InGameOn")]
    protected StartInMenu _startGameWhere = StartInMenu.InGameControl;
    [SerializeField] [Header("Cancel / Back Settings")] [Space(10f)]
    [Label("Nothing to Cancel Action")]
    protected PauseOptionsOnEscape _pauseOptionsOnEscape = PauseOptionsOnEscape.DoNothing;
    [SerializeField] 
    [ValidateInput("ProtectEscapeKeySetting", "Can't set Global Settings to Global Settings")]
    protected EscapeKey _globalCancelFunction = EscapeKey.BackOneLevel;
    [SerializeField] [Header("Start Delay")] [Space(10f)]
    [Label("Enable Controls After..")]
    protected float _atStartDelay;

    protected Vector3 _mousePosition;

    public abstract ControlMethod ControlType { get; }
    public abstract PauseOptionsOnEscape PauseOptions { get; }
    protected abstract string PauseButton { get; }
    protected abstract string PositiveSwitch { get; }
    protected abstract string NegativeSwitch { get; }
    protected abstract string CancelButton { get; }
    protected abstract string MenuToGameSwitch { get; }
    public abstract InGameSystem InGameMenuSystem { get; }
    public abstract StartInMenu WhereToStartGame { get; }
    public abstract EscapeKey GlobalCancelAction { get; }
    public abstract float StartDelay { get; }
    public abstract bool  MouseClicked { get; }
    public abstract bool CanSwitchToKeysOrController { get; }
    public abstract bool CanSwitchToMouse { get; }
    public abstract Vector3 SetMousePosition();

    public abstract void OnAwake();
    public abstract void TurnOffInGameMenuSystem();
    public abstract bool PressPause();
    public abstract bool PressedMenuToGameSwitch();
    public abstract bool PressedCancel();
    public abstract bool PressedPositiveSwitch();
    public abstract bool PressedNegativeSwitch();
    public abstract bool HotKeyChecker(HotKey hotKey);
}