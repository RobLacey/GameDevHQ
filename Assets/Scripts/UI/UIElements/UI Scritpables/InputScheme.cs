using System;
using NaughtyAttributes;
using UnityEngine;

public abstract class InputScheme : ScriptableObject
{
    [SerializeField] 
    protected ControlMethod _mainControlType = ControlMethod.MouseOnly;
    [SerializeField] 
    [Header("In Game System")] [Space(10f)]
    protected InGameSystem _inGameMenuSystem = InGameSystem.Off;
    [SerializeField] 
    [EnableIf("InGameOn")]
    protected StartInMenu _startGameWhere = StartInMenu.InGameControl;
    [SerializeField] 
    [Header("Cancel / Back Settings")] [Space(10f)] [Label("Nothing to Cancel Action")]
    protected PauseOptionsOnEscape _pauseOptionsOnEscape = PauseOptionsOnEscape.DoNothing;
    [SerializeField]
    private PauseFunction _globalEscapeFunction;
    [SerializeField] 
    [Header("Start Delay")] [Space(10f)] [Label("Enable Controls After..")]
    protected float _atStartDelay;

    //Variables
    protected Vector3 _mousePosition;
    private enum PauseFunction { DoNothing, BackOneLevel, BackToHome }
    
    //Editor
    private bool InGameOn => _inGameMenuSystem == InGameSystem.On;

    public ControlMethod ControlType => _mainControlType;
    public PauseOptionsOnEscape PauseOptions => _pauseOptionsOnEscape;
    public EscapeKey GlobalCancelAction => SetGlobalEscapeFunction();
    public float StartDelay => _atStartDelay;
    public InGameSystem InGameMenuSystem => _inGameMenuSystem;
    public StartInMenu WhereToStartGame => _startGameWhere;

    protected abstract string PauseButton { get; }
    protected abstract string PositiveSwitch { get; }
    protected abstract string NegativeSwitch { get; }
    protected abstract string CancelButton { get; }
    protected abstract string MenuToGameSwitch { get; }
    public abstract bool  MouseClicked { get; }
    public abstract bool CanSwitchToKeysOrController { get; }
    public abstract bool CanSwitchToMouse { get; }
    public abstract void SetMousePosition();

    public void OnAwake() => SetUpUInputScheme();

    protected abstract void SetUpUInputScheme();
    public void TurnOffInGameMenuSystem() => _inGameMenuSystem = InGameSystem.Off;
    public abstract bool PressPause();
    public abstract bool PressedMenuToGameSwitch();
    public abstract bool PressedCancel();
    public abstract bool PressedPositiveSwitch();
    public abstract bool PressedNegativeSwitch();
    public abstract bool HotKeyChecker(HotKey hotKey);

    private EscapeKey SetGlobalEscapeFunction()
    {
        switch (_globalEscapeFunction)
        {
            case PauseFunction.DoNothing:
                return EscapeKey.None;
            case PauseFunction.BackOneLevel:
                return EscapeKey.BackOneLevel;
            case PauseFunction.BackToHome:
                return EscapeKey.BackToHome;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}