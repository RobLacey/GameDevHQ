
using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UIElements;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class VirtualCursor : IRaycastController, IEventUser, IClearAll
{
    [SerializeField]
    private GameType _restrictRaycastTo = GameType.NoRestrictions;
    [SerializeField] 
    [AllowNesting] //[ValidateInput(HasRecTransform, RectMessage)]
    private GameObject _virtualCursor;
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
    private RectTransform _cursorRectTransform;
    private GraphicRaycaster _raycaster;
    private RectTransform _lastHit;
    private bool _canStart;
 
    //Editor
    private bool HasRect(RectTransform rect) => rect != null;
    private const string HasRecTransform = nameof(HasRect);
    private const string RectMessage = "Assign Virtual Cursor RectTransform";


    //Properties & Setters / Getters
    public LayerMask LayerToHit => _layerToHit;
    public float LaserLength => _raycastLength;
    public bool SelectPressed => _scheme.PressSelect();
    public GraphicRaycaster GraphicRaycaster => _raycaster;
    public GameObject ReturnVirtualCursor => _virtualCursor;


    private bool InGameCursor => _controller.ControlType == VirtualControl.Yes;

    private bool Allow2D => _restrictRaycastTo == GameType._2D || _restrictRaycastTo == GameType.NoRestrictions;

    private bool Allow3D => _restrictRaycastTo == GameType._3D || _restrictRaycastTo == GameType.NoRestrictions;

    private void InGame(IInMenu args)
    {
        //if(CanNotUseVirtualCursor()) return;
        
        _inGame = !args.InTheMenu;
        // if (!_inGame)
        // {
        //     _cursorCanvas.enabled = false;
        //     _raycastTo2D.WhenInMenu();
        //     _raycastTo3D.WhenInMenu();
        //     
        //     foreach (var controllerReturnActiveNode in _controller.ReturnActiveNodes)
        //     {
        //         controllerReturnActiveNode.GetComponent<UINode>()
        //                                   .MyBranch
        //                                   .SetBlockRaycast(BlockRaycast.Yes);
        //     }
        // }
        // else
        // {
        //     _cursorCanvas.enabled = true;
        //     _noInput = false;
        //     FixedUpdate();
        //     _noInput = true;
        // }
    }

    
    private void SetStartingCanvasOrder(ISetStartingCanvasOrder args)
    {
        var storedCondition = _cursorCanvas.enabled;
        _cursorCanvas.enabled = true;
        _cursorCanvas.overrideSorting = true;
        _cursorCanvas.sortingOrder = args.ReturnVirtualCursorCanvasOrder();
        _cursorCanvas.enabled = storedCondition;
    }

    
   //Main
    public void ObserveEvents()
    {
        EVent.Do.Subscribe<IInMenu>(InGame);
        EVent.Do.Subscribe<ISetStartingCanvasOrder>(SetStartingCanvasOrder);
        EVent.Do.Subscribe<IAllowKeys>(SaveAllowKeys);
    }

    private void SaveAllowKeys(IAllowKeys args)
    {
        if(_inGame || _scheme.CanUseVirtualCursor == VirtualControl.No) return;
        
        if (args.CanAllowKeys)
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

            // foreach (var controllerReturnActiveNode in _controller.ReturnActiveNodes)
            // {
            //     controllerReturnActiveNode.GetComponent<UINode>()
            //                               .MyBranch
            //                               .SetBlockRaycast(BlockRaycast.Yes);
            // }

        }

    }

    public void SetUpVirtualCursor(GOUIController controller)
    {
        _controller = controller;
        _scheme = _controller.GetScheme();
        _lastHit = null;
       // if (CanNotUseVirtualCursor()) return;
        _cursorRectTransform = _controller.NewVirtualCursor.GetComponent<RectTransform>();
        _cursorCanvas = _cursorRectTransform.GetComponent<Canvas>();
        _raycaster = _controller.GetComponent<GraphicRaycaster>();
        //_cursorCanvas.enabled = false;
        
        ObserveEvents();
        _playerObjects = _controller.GetPlayerObjects();
        _raycastTo2D = EJect.Class.WithParams<I2DRaycast>(this);
        _raycastTo3D = EJect.Class.WithParams<I3DRaycast>(this);
        SetUpInGameObjects();

        _newCursorPos = Vector2.zero;
        _canStart = true;
    }
    
    private bool CanNotUseVirtualCursor()
    {
        if (!_virtualCursor  /*|| !_inGame*/ || _noInput) return false;
        
        if (_scheme.CanUseVirtualCursor == VirtualControl.Yes || _scheme.ControlType != ControlMethod.KeysOrControllerOnly)
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
        if(!_canStart) return;
        
        _noInput = _scheme.VcHorizontal() == 0 && _scheme.VcVertical() == 0;
        
        if (_noInput) return;

        
        if(Allow2D) 
            _raycastTo2D.DoRaycast(_cursorRectTransform.position);
        if(Allow3D)
            _raycastTo3D.DoRaycast(_cursorRectTransform.position);
    }
    
    public void UseVirtualCursor()
    {
        if(!_canStart) return;

        HasSelectedBeenPressed();
        MoveVirtualCursor();
        CheckForUIInteraction();
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
        _controller.SetCursorPos(_cursorRectTransform.transform.position);

    }

    private void CalculateNewPosition()
    {
        var temp = _cursorRectTransform.anchoredPosition + _newCursorPos;
        temp.x = Mathf.Clamp(temp.x, _screenLeft, _screenRight);
        temp.y = Mathf.Clamp(temp.y, _screenBottom, _screenTop);
        _cursorRectTransform.anchoredPosition = temp;

    }

    private void CheckForUIInteraction()
    {
        _noInput = _scheme.VcHorizontal() == 0 && _scheme.VcVertical() == 0;
        
        if (_noInput) return;

        if (_lastHit)
        {
            var rectT = _lastHit;
            var pos = _cursorRectTransform.transform.position;
            if(RectTransformUtility.RectangleContainsScreenPoint(rectT, pos, null)) return;
            
            _lastHit.GetComponent<UINode>().OnPointerExit(null);
            _lastHit = null;
        }
        
        foreach (var node in _controller.ReturnActiveNodes)
        {
            var rectT = node;
            var pos = _cursorRectTransform.transform.position;
            var result = RectTransformUtility.RectangleContainsScreenPoint(rectT, pos, null);
            if (result)
            {
                if (_lastHit == node) return;
                
                Vector3[] corners = new Vector3[4];
                node.GetWorldCorners(corners);
                _controller.SetCorners(corners);
                
                _lastHit = node;
                _lastHit.GetComponent<UINode>().OnPointerEnter(null);
                return;
            }
        }

    }
}
