using System;
using NaughtyAttributes;
using UnityEngine;

[Serializable]
public class CursorSettings
{
    [SerializeField] 
    [ShowIf("CustomCursor")] 
    private Texture2D _cursor = default;

    [SerializeField] 
    [ShowIf("CustomCursor")] 
    private Vector2 _hotSpot =default;

    public Texture2D CursorTexture => _cursor;
    public Vector2 HotSpot => _hotSpot;
}

public abstract class InputScheme : ScriptableObject
{
    [Space(EditorSpace, order = 0)]
    
    [SerializeField] 
    [DisableIf(IsPlaying)]
    protected ControlMethod _mainControlType = ControlMethod.MouseOnly;

    [Header("Mouse and Cursor Settings")] [Space(10f)] [HorizontalLine(1, color: EColor.Blue, order = 1)]

    [SerializeField]
    [Label("Hide Cursor When Keys Active")]
    private IsActive _hideMouseCursor = IsActive.No;
    
    [SerializeField] 
    [Label("Cancel When Clicked Off UI")] //TODO set to left if VC active (adjust Input Scheme)
    protected CancelClickLocation _cancelClickOn = CancelClickLocation.Never;
    
    //TODO Create a custom cursor and virual cursor class so a hotspot can be set for a VC and just use a texture not a prefab
    
    [SerializeField] 
    [HideIf(KeysOnly)] //TODO Hide for VC active
    private IsActive _customMouseCursor = IsActive.No;

    [SerializeField] 
    [EnableIf(UseCustomCursor)]
    private CursorSettings _cursorSettings = default;

    [SerializeField] 
    [ShowIf(KeysOnly)]
    protected InGameSystem _inGameMenuSystem = InGameSystem.Off;
    
    [SerializeField] 
    [Space(EditorSpace)] [DisableIf(IsPlaying)]
    private VirtualControl _useVirtualCursor = VirtualControl.No;

    [SerializeField] 
    [ShowIf(VirtualCursor)] 
    private VirtualCursor _virtualCursor;
    
    [SerializeField] [Space(EditorSpace)] [DisableIf(IsPlaying)]
    protected InMenuOrGame _startGameWhere = InMenuOrGame.InGameControl;
    
    [Header("Cancel / Back Settings")] [Space(10f)] [HorizontalLine(1, color: EColor.Blue, order = 1)]
    
    [SerializeField] 
    [Label("Nothing to Cancel Action")] 
    protected PauseOptionsOnEscape _pauseOptionsOnEscape = PauseOptionsOnEscape.DoNothing;
    
    [SerializeField]
    private PauseFunction _globalEscapeFunction;
    
    [Header("Start Delay")] [Space(10f)] [HorizontalLine(1, color: EColor.Blue, order = 1)]
    
    [SerializeField] 
    [Label("Delay UI Start By then..")] [Range(0, 10)]
    protected float _delayUIStart;
    
    [SerializeField] 
    [Label("..Enable Controls After..")] [Range(0, 10)] 
    protected float _controlActivateDelay;


    //Variables
    protected Vector3 _virtualCursorPosition;

    private enum PauseFunction { DoNothing, BackOneLevel, BackToHome }

    
    //Editor
    private bool InGameOn => _inGameMenuSystem == InGameSystem.On;
    private bool CustomCursor => _customMouseCursor == IsActive.Yes;
    private const string UseCustomCursor = nameof(CustomCursor);
    private bool UseVirtualCursor => _useVirtualCursor == VirtualControl.Yes;
    private const string VirtualCursor = nameof(UseVirtualCursor);
    private const int EditorSpace = 20;
    private static bool AppIsPlaying => Application.isPlaying;
    private const string IsPlaying = nameof(AppIsPlaying);

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
            Cursor.SetCursor(_cursorSettings.CursorTexture, _cursorSettings.HotSpot, CursorMode.Auto);
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
    public VirtualControl CanUseVirtualCursor => _useVirtualCursor;
    public bool HideMouseCursor => _hideMouseCursor == IsActive.Yes;

    public VirtualCursor ReturnVirtualCursor => _virtualCursor;
    public CancelClickLocation CanCancelWhenClickedOff => _cancelClickOn;

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
    public abstract bool  AnyMouseClicked { get; }
    public abstract bool  LeftMouseClicked { get; }
    public abstract bool  RightMouseClicked { get; }
    public abstract bool CanSwitchToKeysOrController(bool allowKeys);
    public abstract bool CanSwitchToMouseOrVC(bool allowKeys);
    protected abstract string SwitchToVC { get; }
    protected abstract string MouseXAxis { get; }
    protected abstract string MouseYAxis { get; }
    public abstract Vector3 GetMouseOrVcPosition();
    public abstract void SetVirtualCursorPosition(Vector3 pos);
    private protected abstract Vector3 GetVirtualCursorPosition();

    public void OnAwake()
    {
        SetUpUInputScheme();
    }

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
    private protected abstract bool VCSwitchTo();
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

    public abstract float MouseXAxisValue();
    public abstract float MouseYAxisValue();
}