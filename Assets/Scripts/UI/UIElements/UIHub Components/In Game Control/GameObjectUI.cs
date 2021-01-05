using System;
using NaughtyAttributes;
using UIElements;
using UnityEngine;

public interface IGameObject : IParameters
{
    LayerMask LayerToHit { get; }
    float LaserLength { get; }
    bool SelectPressed { get; }
}

public class GameObjectUI : MonoBehaviour, IEventUser, IGameObject
{
    [SerializeField] 
    private VirtualControl _inGameControlType = VirtualControl.None;
    [SerializeField] 
    private GameType _raycastColliderType = GameType._2D;
    [SerializeField] 
    [ValidateInput("HasRect", "Assign Virtual Cursor RectTransform")] [ShowIf("InGameCursorEditor")]
    private RectTransform _virtualCursor = default;
    [SerializeField] 
    [ShowIf("InGameCursorEditor")] [Range(1f, 20f)] 
    private float _cursorSpeed = 1.5f;
    [SerializeField] 
    [ShowIf("InGameCursorEditor")] 
    private LayerMask _layerToHit;
    [SerializeField] 
    [ShowIf("InGameCursorEditor")] 
    private float _raycastLength = 1000f;
    
    //Variables
    private InGameObjectUI[] _playerObjects = default;
    private bool _inGame, _noInput;
    private Vector2 _newCursorPos = Vector2.zero;
    private int _index = 0;
    private readonly int _screenLeft = -Screen.width / 2;
    private readonly int _screenRight = Screen.width / 2;
    private readonly int _screenBottom = -Screen.height / 2;
    private readonly int _screenTop = Screen.height / 2;
    private IRaycast _raycastTo2D, _raycastTo3D;
    private InputScheme _scheme;

    //Properties
    public LayerMask LayerToHit => _layerToHit;
    public float LaserLength => _raycastLength;
    public bool SelectPressed => _scheme.PressSelect();
    private bool UseBoth => _inGameControlType == VirtualControl.Both;
    private bool InGameSwitch => (_inGameControlType == VirtualControl.Switcher || UseBoth) && _inGame;
    private bool InGameCursor => (_inGameControlType == VirtualControl.Cursor || UseBoth) && _inGame;
    private bool Allow2D => _raycastColliderType == GameType._2D || _raycastColliderType == GameType.Both;
    private bool Allow3D => _raycastColliderType == GameType._3D || _raycastColliderType == GameType.Both;
    
    //Editor
    private bool HasRect(RectTransform rect) => rect != null;
    private bool InGameCursorEditor => (_inGameControlType == VirtualControl.Cursor || UseBoth) 
                                      && GetComponent<UIInput>().ReturnScheme.InGameMenuSystem == InGameSystem.On;

    private void Awake()
    {
        _playerObjects = FindObjectsOfType<InGameObjectUI>();
        _raycastTo2D = EJect.Class.WithParams<I2DRaycast>(this);
        _raycastTo3D = EJect.Class.WithParams<I3DRaycast>(this);
        _scheme = GetComponent<UIInput>().ReturnScheme;

        SetUpVirtualCursor();
        SetUpSwitcher();
    }

    private void SetUpVirtualCursor()
    {
        _newCursorPos = Vector2.zero;
        if (_inGameControlType == VirtualControl.Switcher
            || _scheme.ControlType != ControlMethod.KeysOrControllerOnly)
        {
            _virtualCursor.gameObject.SetActive(false);
        }
    }

    private void SetUpSwitcher()
    {
        if (_inGameControlType == VirtualControl.Switcher)
        {
            foreach (var obj in _playerObjects)
            {
                obj.SetToUseSwitcher();
            }
        }
    }

    private void OnEnable() => ObserveEvents();

    public void ObserveEvents() => EVent.Do.Subscribe<IInMenu>(InGame);

    private void InGame(IInMenu args)
    {
        _inGame = !args.InTheMenu;
        if (_playerObjects.Length == 0) return;
        
        if (_inGame && InGameSwitch)
        {
            _playerObjects[_index].OverFocus();
        }
        else
        {
            _playerObjects[_index].UnFocus();
        }
    }

    private void Update()
    {
        if(!_inGame) return;
        UseSwitcher();
        UseVirtualCursor();
    }

    private void FixedUpdate()
    {
        if(_noInput || !InGameCursor) return;
        if(Allow2D) 
            _raycastTo2D.DoRaycast(_virtualCursor.position);
        if(Allow3D)
            _raycastTo3D.DoRaycast(_virtualCursor.position);
    }

    private void UseSwitcher()
    {
        if(!InGameSwitch || _playerObjects.Length == 0) return;
        
        if (_scheme.PressedPositiveSwitch())
        {
            SwapPlayerControlObject(x => _index.PositiveIterate(x));
            return;
        }

        if (_scheme.PressedNegativeSwitch())
        {
            SwapPlayerControlObject(x => _index.NegativeIterate(x));
        }
    }

    private void SwapPlayerControlObject(Func<int, int> swap)
    {
        _playerObjects[_index].UnFocus();
        _index = swap(_playerObjects.Length);
        _playerObjects[_index].OverFocus();
    }

    private void UseVirtualCursor()
    {
        if (!InGameCursor) return;

        HasSelectedBeenPressed();
        MoveVirtualCursor();
    }

    private void HasSelectedBeenPressed()
    {
        if (Allow2D)
            _raycastTo2D.DoSelectedInGameObj();
        if (Allow3D)
            _raycastTo3D.DoSelectedInGameObj();
    }

    private void MoveVirtualCursor()
    {
        _noInput = _scheme.VcHorizontal() == 0 && _scheme.VcVertical() == 0;
        if (_noInput) return;

        MoveVirtualMouse();
    }

    private void MoveVirtualMouse()
    {
        _newCursorPos.x = _scheme.VcHorizontal() * _cursorSpeed;
        _newCursorPos.y = _scheme.VcVertical() * _cursorSpeed;
        CalculateNewPosition();
    }

    private void CalculateNewPosition()
    {
        var temp = _virtualCursor.anchoredPosition + _newCursorPos;
        temp.x = Mathf.Clamp(temp.x, _screenLeft, _screenRight);
        temp.y = Mathf.Clamp(temp.y, _screenBottom, _screenTop);
        _virtualCursor.anchoredPosition = temp;
    }
}
