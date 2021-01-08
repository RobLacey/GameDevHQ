using System;
using System.Linq;
using NaughtyAttributes;
using UIElements;
using UnityEngine;

public interface IGOController : IParameters
{
    UIGOController Controller { get; }
}

public partial class UIGOController : MonoBehaviour, IGOController, IEventUser
{
    public UIGOController() => _validationCheck = new ValidationCheck(this);

    [SerializeField] 
    private VirtualControl _inGameControlType = VirtualControl.None;
    [SerializeField] 
    [ShowIf(IsInGameCursor)] 
    private VirtualCursor _virtualCursor;
    
    [Space(20, order = 1)]
    [Header(ClearHeader, order = 2)] [HorizontalLine(1f, EColor.Blue, order = 3)]
    [SerializeField] 
    private CancelWhen _cancelWhen;
    [SerializeField] 
    [ShowIf(UseSafeList)] [Space(10, order = 1)] [InfoBox(SafeNodeInfo)] [BoxGroup("Safe Nodes")]
    private UINode[] _safeNodeList;

    
    //Variables
    private readonly ValidationCheck _validationCheck;
    private IMouseOnlySwitcher _mouseOnlySwitcher;
    private ISwitcher _switcher;
    private InputScheme _inputScheme;
    private InGameObjectUI _activeObject;
    private int _index;
    private InGameObjectUI[] _playerObjects;

    private enum CancelWhen
    {
        EscapeKeyOnly, NewHighlightedNode, NewSelectedNode
    }
    //Properties
    public UIGOController Controller => this;
    public VirtualControl ControlType
    {
        get => _inGameControlType;
        set => _inGameControlType = value;
    }
    public InputScheme GetScheme() => GetComponent<UIInput>().ReturnScheme;
    public InGameObjectUI[] GetPlayerObjects() => _playerObjects;
    private bool UseBoth => _inGameControlType == VirtualControl.Both;

    public int GetIndex() => _index;

    public void SetIndex(InGameObjectUI newObj)
    {
        int index = 0;
        foreach (var inGameObjectUI in _playerObjects)
        {
            if (inGameObjectUI == newObj)
            { 
                _index = index;
                break;
            }
            index++;
        }
    }

    private void NewHighlightedNode(IHighlightedNode args)
    {
        if (_activeObject.IsNull() || _cancelWhen != CancelWhen.NewHighlightedNode) return;
        if(_safeNodeList.Contains(args.Highlighted)) return;
        if(_activeObject.PointerOver) return;
        _activeObject.CancelUi();
    }
    
    private void NewSelectedBranch(ISelectedNode args)
    {
        if (_activeObject.IsNull() || _cancelWhen != CancelWhen.NewSelectedNode) return;
        if(_safeNodeList.Contains(args.UINode)) return;
        _activeObject.CancelUi();
    }
    
    private void SaveActiveInGameObject(IActiveInGameObject args) 
        => _activeObject = args.IsNull() ? null : args.ActiveObject;

    //Main
    private void Awake()
    {
        _playerObjects = FindObjectsOfType<InGameObjectUI>();
        _inputScheme = GetComponent<UIInput>().ReturnScheme;
        _mouseOnlySwitcher = EJect.Class.WithParams<IMouseOnlySwitcher>(this);
        _switcher = EJect.Class.WithParams<ISwitcher>(this);
        SetUpVirtualCursor();
    }

    private void SetUpVirtualCursor() => _virtualCursor.SetUpVirtualCursor(this);
    private void OnEnable()
    {
        ObserveEvents();
        _switcher.OnEnable();
    }
    
    public void ObserveEvents()
    {
        EVent.Do.Subscribe<IActiveInGameObject>(SaveActiveInGameObject);
        EVent.Do.Subscribe<IHighlightedNode>(NewHighlightedNode);
        EVent.Do.Subscribe<ISelectedNode>(NewSelectedBranch);
    }

    private void OnValidate() => _validationCheck.ValidateDialogue();

    private void Update()
    {
        if (_inputScheme.PressedCancel())
        {
            if(_activeObject.IsNull()) return;
            _activeObject.CancelUi();
        }
        
        if (_inGameControlType == VirtualControl.None) return;
        _mouseOnlySwitcher.UseMouseOnlySwitcher();
        _switcher.UseSwitcher();
        _virtualCursor.UseVirtualCursor();

    }

    private void FixedUpdate() => _virtualCursor.FixedUpdate();
}
