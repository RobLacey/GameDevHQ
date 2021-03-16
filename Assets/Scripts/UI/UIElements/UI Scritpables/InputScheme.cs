using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class InputScheme : ScriptableObject
{
    [Space(20f, order = 0)]
    [SerializeField] 
    protected ControlMethod _mainControlType = ControlMethod.MouseOnly;
    
    [SerializeField] [HideIf(KeysOnly)] private IsActive _customMouseCursor = IsActive.No;
    [SerializeField] [ShowIf("CustomCursor")] private Texture2D _cursor = default;
    [SerializeField] [ShowIf("CustomCursor")] private Vector2 _hotSpot =default;
    
    [SerializeField] [ShowIf(KeysOnly)]
    protected InGameSystem _inGameMenuSystem = InGameSystem.Off;
    
    [SerializeField] 
    [ShowIf(KeysOnly)] 
    [EnableIf("InGameOn")]
    protected InMenuOrGame _startGameWhere = InMenuOrGame.InGameControl;
    
    [Header("Cancel / Back Settings")] [Space(10f)] [HorizontalLine(1, color: EColor.Blue, order = 1)]
    [SerializeField] 
    [Label("Nothing to Cancel Action")] 
    protected PauseOptionsOnEscape _pauseOptionsOnEscape = PauseOptionsOnEscape.DoNothing;
    
    [SerializeField]
    private PauseFunction _globalEscapeFunction;
    
    [Header("Start Delay")] [Space(10f)] [HorizontalLine(1, color: EColor.Blue, order = 1)]
    [SerializeField] 
    [Label("Delay UI Start By then..")]
    [Range(0, 10)] protected float _delayUIStart;
    
    [SerializeField] 
    [Label("..Enable Controls After..")]
    [Range(0, 10)] 
    protected float _controlActivateDelay;


    //Variables
    protected Vector3 _mousePosition;
    public ValidationCheck Check { get; set; }
    private enum PauseFunction { DoNothing, BackOneLevel, BackToHome }
    
    //Editor
    private bool InGameOn => _inGameMenuSystem == InGameSystem.On;
    private bool CustomCursor => _customMouseCursor == IsActive.Yes;

    private const string KeysOnly = nameof(KeyboardOnly);
    private bool KeyboardOnly
    {
        get{
            var keysOnly = _mainControlType == ControlMethod.KeysOrControllerOnly;

            if (!keysOnly)
            {
                _inGameMenuSystem = InGameSystem.Off;
            }
            else
            {
                _customMouseCursor = IsActive.No;
            }
            return keysOnly;
        }
    }


    public void SetCursor()
    {
        if (_customMouseCursor == IsActive.Yes && !KeyboardOnly)
        {
            Cursor.SetCursor(_cursor, _hotSpot, CursorMode.Auto);
        }
    }

    public ControlMethod ControlType
    {
        get => _mainControlType;
        set => _mainControlType = value;
    }

    public PauseOptionsOnEscape PauseOptions => _pauseOptionsOnEscape;
    public EscapeKey GlobalCancelAction => SetGlobalEscapeFunction();
    public float ControlActivateDelay => _controlActivateDelay;
    public float DelayUIStart => _delayUIStart;
    public InGameSystem InGameMenuSystem => _inGameMenuSystem;
    public InMenuOrGame WhereToStartGame => _startGameWhere;

    protected abstract string PauseButton { get; }
    protected abstract string PositiveSwitch { get; }
    protected abstract string NegativeSwitch { get; }
    protected abstract string PositiveGOUISwitch { get; }
    protected abstract string NegativeGOUISwitch { get; }

    protected abstract string CancelButton { get; }
    protected abstract string MenuToGameSwitch { get; }
    protected abstract string VCursorHorizontal { get; }
    protected abstract string VCursorVertical { get; }
    protected abstract string SelectedButton { get; }
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
    public abstract bool PressedPositiveGOUISwitch();
    public abstract bool PressedNegativeGOUISwitch();
    public abstract float VcHorizontal();
    public abstract float VcVertical();
    public abstract bool PressSelect();
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

    private void OnValidate()
    {
        FindObjectOfType<UIInput>().DoValidation();
    }
}