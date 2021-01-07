using NaughtyAttributes;
using UIElements;
using UnityEngine;

public interface IGOController : IParameters
{
    UIGameObjectController Controller { get; }
}

public class UIGameObjectController : MonoBehaviour, IGOController
{
    public UIGameObjectController() => _validationCheck = new ValidationCheck(this);

    [SerializeField] 
    private VirtualControl _inGameControlType = VirtualControl.None;
    [SerializeField] 
    [ShowIf("InGameCursorEditor")]private VirtualCursor _virtualCursor;
    
    //Variables
    private readonly ValidationCheck _validationCheck;
    private IMouseOnlySwitcher _mouseOnlySwitcher;
    private ISwitcher _switcher;

    //Properties
    public UIGameObjectController Controller => this;
    public VirtualControl ControlType
    {
        get => _inGameControlType;
        set => _inGameControlType = value;
    }
    public InputScheme GetScheme() => GetComponent<UIInput>().ReturnScheme;
    public InGameObjectUI[] GetPlayerObjects() => FindObjectsOfType<InGameObjectUI>();
    private bool UseBoth => _inGameControlType == VirtualControl.Both;
    
    //Editor
    private bool InGameCursorEditor => (_inGameControlType == VirtualControl.VirtualCursor || UseBoth) 
                                      && GetComponent<UIInput>().ReturnScheme.InGameMenuSystem == InGameSystem.On;

    //Main
    private void Awake()
    {
        _mouseOnlySwitcher = EJect.Class.WithParams<IMouseOnlySwitcher>(this);
        _switcher = EJect.Class.WithParams<ISwitcher>(this);
        SetUpVirtualCursor();
    }
    
    private void OnValidate() => _validationCheck.ValidateDialogue();

    private void SetUpVirtualCursor() => _virtualCursor.SetUpVirtualCursor(this);

    private void OnEnable() => _switcher.OnEnable();
    
    private void Update()
    {
        if (_inGameControlType == VirtualControl.None) return;
        _mouseOnlySwitcher.UseMouseOnlySwitcher();
        _switcher.UseSwitcher();
        _virtualCursor.UseVirtualCursor();
    }

    private void FixedUpdate() => _virtualCursor.FixedUpdate();
}
