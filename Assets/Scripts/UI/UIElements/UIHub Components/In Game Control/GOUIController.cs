using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NaughtyAttributes;
using UIElements;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public interface IGOController : IParameters
{
    GOUIController Controller { get; }
    GameObject NewVirtualCursor { get; set; }
}

public partial class GOUIController : MonoBehaviour, IGOController, IEventUser
{
    public GOUIController() => _validationCheck = new ValidationCheck(this);

    [SerializeField] private Vector3 _cursorPos;
    [SerializeField] private Vector3[] _corners;
    
    [Space(20, order = 1)]
    [Header(ClearHeader, order = 2)] [HorizontalLine(1f, EColor.Blue, order = 3)]
    [SerializeField] 
    private CancelWhen _cancelWhen;
    [SerializeField] 
    [ShowIf(UseSafeList)] [Space(10, order = 1)] [InfoBox(SafeNodeInfo)] [BoxGroup("Safe Nodes")]
    private UINode[] _safeNodeList;

    [SerializeField] private List<RectTransform> _activeNodes = new List<RectTransform>();

    
    //Variables
    private readonly ValidationCheck _validationCheck;
    private IMouseOnlySwitcher _mouseOnlySwitcher;
    private ISwitcher _switcher;
    private InputScheme _inputScheme;
    private GOUIModule _activeObject;
    private int _index;
    private IGOUIModule[] _playerObjects;
    private bool _canStart;
    private bool _onHomeScreen = true;
    private bool _gameIsPaused;
    private VirtualCursor _virtualCursor;
    
    GraphicRaycaster m_Raycaster;
    PointerEventData m_PointerEventData = new PointerEventData(EventSystem.current);
    EventSystem m_EventSystem;


    private enum CancelWhen
    {
        EscapeKeyOnly, NewHighlightedNode, NewSelectedNode
    }
    //Properties
    public GOUIController Controller => this;
    public VirtualControl ControlType => _inputScheme.CanUseVirtualCursor;
    public InputScheme GetScheme() => GetComponent<UIInput>().ReturnScheme;
    private void SaveOnHomeScreen(IOnHomeScreen args) => _onHomeScreen = args.OnHomeScreen;
    private void GameIsPaused(IGameIsPaused args) => _gameIsPaused = args.GameIsPaused;
    public List<RectTransform> ReturnActiveNodes => _activeNodes;

    public GameObject NewVirtualCursor { get; set; }

    public IGOUIModule[] GetPlayerObjects() => _playerObjects;

    public int GetIndex() => _index;
    private void CanStart(IOnStart obj) => _canStart = true;

    private bool CanSwitch => _canStart && _onHomeScreen && !_gameIsPaused;


    public void SetIndex(GOUIModule newObj)
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
    
    public void SetCursorPos(Vector3 pos)
    {
        _inputScheme.SetVirtualCursorPosition(pos);
        _cursorPos = pos;
    }

    public void SetCorners(Vector3[] corners)
    {
        _corners = corners;
    }

    private void NewHighlightedNode(IHighlightedNode args)
    {
        // if (_activeObject.IsNull() || _cancelWhen != CancelWhen.NewHighlightedNode) return;
        // if(_safeNodeList.Contains(args.Highlighted)) return;
        // _activeObject.CancelUi();
    }
    
    private void NewSelectedBranch(ISelectedNode args)
    {
        // if (_activeObject.IsNull() || _cancelWhen != CancelWhen.NewSelectedNode) return;
        // if(_safeNodeList.Contains(args.UINode)) return;
        // _activeObject.CancelUi();
    }
    
    private void SaveActiveInGameObject(/*IActiveInGameObject args*/)
    {
        // if(_activeObject.IsNotNull())
        // {
        //     _activeObject.CancelUi();
        // }        
        // _activeObject = args.IsNull() ? null : args.UIGOModule;
    }

    //Main
    private void Awake()
    {
        _playerObjects = FindObjectsOfType<GOUIModule>();
        _inputScheme = GetComponent<UIInput>().ReturnScheme;
        _virtualCursor = _inputScheme.ReturnVirtualCursor;
        
        if(_inputScheme.CanUseVirtualCursor == VirtualControl.Yes)
        {
            NewVirtualCursor = Instantiate(_virtualCursor.ReturnVirtualCursor, transform, true);
            NewVirtualCursor.GetComponent<Canvas>().enabled = false;
            NewVirtualCursor.GetComponent<RectTransform>().anchoredPosition3D = Vector3.zero;
            _virtualCursor.SetUpVirtualCursor(this);
        }        
        _mouseOnlySwitcher = EJect.Class.WithParams<IMouseOnlySwitcher>(this);
       // _switcher = EJect.Class.WithParams<ISwitcher>(this);
    }

    private void OnEnable()
    {
        ObserveEvents();
        BranchBase.AddNode += AddNodes;
        BranchBase.RemoveNode += RemoveNodes;

       // _switcher.OnEnable();
    }

    void Start()
    {
        //Fetch the Raycaster from the GameObject (the Canvas)
        m_Raycaster = GetComponent<GraphicRaycaster>();
        //Fetch the Event System from the Scene
        m_EventSystem = EventSystem.current;
    }
    
    public void ObserveEvents()
    {
       // EVent.Do.Subscribe<IActiveInGameObject>(SaveActiveInGameObject);
        EVent.Do.Subscribe<IHighlightedNode>(NewHighlightedNode);
        EVent.Do.Subscribe<ISelectedNode>(NewSelectedBranch);
        EVent.Do.Subscribe<IOnStart>(CanStart);
        EVent.Do.Subscribe<IOnHomeScreen>(SaveOnHomeScreen);
        EVent.Do.Subscribe<IGameIsPaused>(GameIsPaused);
    }



    //private void OnValidate() => _validationCheck.ValidateDialogue();

    private void Update()
    {
        if(!CanSwitch || !_canStart) return;
        // if (_inputScheme.PressedCancel())
        // {
        //     if(_activeObject.IsNull()) return;
        //     _activeObject.CancelUi();
        // }
        //
        // if (_inGameControlType == VirtualControl.None) return;
         _mouseOnlySwitcher.UseMouseOnlySwitcher();
        // _switcher.UseSwitcher();
        // if (Input.GetKeyDown(KeyCode.V))
        // {
        //     _virtualCursor.ActivateVC();
        // }
        if(_inputScheme.CanUseVirtualCursor == VirtualControl.No) return;
         _virtualCursor.UseVirtualCursor();
    }

    private void FixedUpdate()
    {
        if(!CanSwitch) return;

        _mouseOnlySwitcher.ClearSwitchActivatedGOUI();
        
        if(_inputScheme.CanUseVirtualCursor == VirtualControl.No) return;
        _virtualCursor.FixedUpdate();
    }

    private void OnDisable()
    {
        BranchBase.AddNode -= AddNodes;
        BranchBase.RemoveNode -= RemoveNodes;
    }

    private void AddNodes(INode[] nodes)
    {
        UINode[] list = nodes.Cast<UINode>().ToArray();
        
        foreach (var node in list)
        {
            var temp = node.GetComponent<RectTransform>();
            if(_activeNodes.Contains(temp)) continue;
            _activeNodes.Add( temp);
           // node.MyBranch.SetBlockRaycast(BlockRaycast.Yes);
        }
    }
    private void RemoveNodes(INode[] nodes)
    {
        UINode[] list = nodes.Cast<UINode>().ToArray();

        foreach (var node in list)
        {
            var temp = node.GetComponent<RectTransform>();
            if(!_activeNodes.Contains(temp)) continue;
            _activeNodes.Remove( temp);
           // node.MyBranch.SetBlockRaycast(BlockRaycast.No);
        }

    }

    // private void FixedUpdate() => _virtualCursor.FixedUpdate();
}