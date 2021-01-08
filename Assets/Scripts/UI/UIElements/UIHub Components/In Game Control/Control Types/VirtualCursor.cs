
using System;
using NaughtyAttributes;
using UIElements;
using UnityEngine;

[Serializable]
public class VirtualCursor : IRaycastController, IEventUser, IClearAll
{
    [SerializeField]
    private GameType _restrictRaycastTo = GameType.NoRestrictions;
    [SerializeField] 
    [AllowNesting] [ValidateInput(HasRecTransform, RectMessage)]
    private RectTransform _virtualCursor = default;
    [SerializeField] 
    [Range(1f, 20f)] 
    private float _cursorSpeed = 7f;
    [SerializeField] 
    private LayerMask _layerToHit;
    [SerializeField]
    private float _raycastLength = 1000f;

    //Variables
    private Vector2 _newCursorPos = Vector2.zero;
    private readonly int _screenLeft = -Screen.width / 2;
    private readonly int _screenRight = Screen.width / 2;
    private readonly int _screenBottom = -Screen.height / 2;
    private readonly int _screenTop = Screen.height / 2;
    private IRaycast _raycastTo2D, _raycastTo3D;
    private bool _noInput, _inGame;
    private InputScheme _scheme;
    private UIGOController _controller;
    private InGameObjectUI[] _playerObjects;

    //Editor
    private bool HasRect(RectTransform rect) => rect != null;
    private const string HasRecTransform = nameof(HasRect);
    private const string RectMessage = "Assign Virtual Cursor RectTransform";

    //Properties
    public LayerMask LayerToHit => _layerToHit;
    public float LaserLength => _raycastLength;
    public bool SelectPressed => _scheme.PressSelect();
    private void InGame(IInMenu args)
    {
        _inGame = !args.InTheMenu;
        if (!_inGame)
        {
            _raycastTo2D.WhenInMenu();
            _raycastTo3D.WhenInMenu();
            EVent.Do.Fetch<IClearAll>()?.Invoke(this);
        }
        else
        {
            _noInput = false;
            FixedUpdate();
            _noInput = true;
        }
    }

    private bool UseBoth => _controller.ControlType == VirtualControl.Both;
    private bool InGameCursor => _controller.ControlType == VirtualControl.VirtualCursor || UseBoth;
    private bool Allow2D => _restrictRaycastTo == GameType._2D || _restrictRaycastTo == GameType.NoRestrictions;
    private bool Allow3D => _restrictRaycastTo == GameType._3D || _restrictRaycastTo == GameType.NoRestrictions;

    //Main
    public void SetUpVirtualCursor(UIGOController controller)
    {
        _controller = controller;
        _scheme = _controller.GetScheme();
        if (CanNotUseVirtualCursor()) return;
        
        ObserveEvents();
        _playerObjects = _controller.GetPlayerObjects();
        _raycastTo2D = EJect.Class.WithParams<I2DRaycast>(this);
        _raycastTo3D = EJect.Class.WithParams<I3DRaycast>(this);
        SetUpInGameObjects();

        _newCursorPos = Vector2.zero;
    }

    private bool CanNotUseVirtualCursor()
    {
        if (!InGameCursor || _scheme.ControlType != ControlMethod.KeysOrControllerOnly)
        {
            _virtualCursor.gameObject.SetActive(false);
            return true;
        }
        return false;
    }

    private void SetUpInGameObjects()
    {
        foreach (var obj in _playerObjects)
        {
            obj.CheckForSetLayerMask(LayerToHit);
        }
    }

    public void ObserveEvents() => EVent.Do.Subscribe<IInMenu>(InGame);

    public void FixedUpdate()
    {
        if(_noInput || !InGameCursor) return;
        if(Allow2D) 
            _raycastTo2D.DoRaycast(_virtualCursor.position);
        if(Allow3D)
            _raycastTo3D.DoRaycast(_virtualCursor.position);
    }
    
    public void UseVirtualCursor()
    {
        if (!InGameCursor || !_inGame) return;

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
