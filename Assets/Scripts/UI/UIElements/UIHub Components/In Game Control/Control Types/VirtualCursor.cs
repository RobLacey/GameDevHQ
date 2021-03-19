
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
    private RectTransform _virtualCursor;
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
    private GOUIController _controller;
    private IGOUIModule[] _playerObjects;
    private Canvas _cursorCanvas;

    //Editor
    private bool HasRect(RectTransform rect) => rect != null;
    private const string HasRecTransform = nameof(HasRect);
    private const string RectMessage = "Assign Virtual Cursor RectTransform";

    //Properties & Setters / Getters
    public LayerMask LayerToHit => _layerToHit;
    public float LaserLength => _raycastLength;
    public bool SelectPressed => _scheme.PressSelect();

    private bool UseBoth => _controller.ControlType == VirtualControl.Both;

    private bool InGameCursor => _controller.ControlType == VirtualControl.VirtualCursor || UseBoth;

    private bool Allow2D => _restrictRaycastTo == GameType._2D || _restrictRaycastTo == GameType.NoRestrictions;

    private bool Allow3D => _restrictRaycastTo == GameType._3D || _restrictRaycastTo == GameType.NoRestrictions;

    private void InGame(IInMenu args)
    {
        if(CanNotUseVirtualCursor()) return;
        
        _inGame = !args.InTheMenu;
        if (!_inGame)
        {
            _cursorCanvas.enabled = false;
            _raycastTo2D.WhenInMenu();
            _raycastTo3D.WhenInMenu();
        }
        else
        {
            _cursorCanvas.enabled = true;
            _noInput = false;
            FixedUpdate();
            _noInput = true;
        }
    }
    
    private void SetStartingCanvasOrder(ISetStartingCanvasOrder args)
    {
        _cursorCanvas.enabled = true;
        _cursorCanvas.overrideSorting = true;
        _cursorCanvas.sortingOrder = args.ReturnVirtualCursorCanvasOrder();
    }

    //Main
    public void ObserveEvents()
    {
        EVent.Do.Subscribe<IInMenu>(InGame);
        EVent.Do.Subscribe<ISetStartingCanvasOrder>(SetStartingCanvasOrder);
    }

    public void SetUpVirtualCursor(GOUIController controller)
    {
        if (CanNotUseVirtualCursor()) return;
        
        _cursorCanvas = _virtualCursor.GetComponent<Canvas>();
        _cursorCanvas.enabled = false;
        _controller = controller;
        _scheme = _controller.GetScheme();
        
        ObserveEvents();
        _playerObjects = _controller.GetPlayerObjects();
        _raycastTo2D = EJect.Class.WithParams<I2DRaycast>(this);
        _raycastTo3D = EJect.Class.WithParams<I3DRaycast>(this);
        SetUpInGameObjects();

        _newCursorPos = Vector2.zero;
    }

    private bool CanNotUseVirtualCursor()
    {
        if (!_virtualCursor  || !_inGame || _noInput) return false;

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
    
    public void FixedUpdate()
    {
        if(CanNotUseVirtualCursor()) return;
        
        if(Allow2D) 
            _raycastTo2D.DoRaycast(_virtualCursor.position);
        if(Allow3D)
            _raycastTo3D.DoRaycast(_virtualCursor.position);
    }
    
    public void UseVirtualCursor()
    {
        if (CanNotUseVirtualCursor()) return;

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
