
using System;
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
    private GameObject _virtualCursorPrefab;
    [SerializeField] 
    [Range(1f, 20f)] 
    private float _cursorSpeed = 7f;
    [SerializeField] 
    private LayerMask _layerToHit;
    [SerializeField]
    private float _raycastLength = 1000f;

    //Variables
    private IRaycast _raycastTo2D, _raycastTo3D;
    private GOUIController _controller;
    private IGOUIModule[] _playerObjects;
    private Canvas _cursorCanvas;
    private bool _canStart;
    private bool _allowKeys;
    private InteractWithUi _interactWithUi = new InteractWithUi();
    private MoveVirtualCursor _moveVirtualCursor = new MoveVirtualCursor();
    
    //Editor
    private bool HasRect(RectTransform rect) => rect != null;
    private const string HasRecTransform = nameof(HasRect);
    private const string RectMessage = "Assign Virtual Cursor RectTransform";


    //Properties & Setters / Getters
    public LayerMask LayerToHit => _layerToHit;
    public float LaserLength => _raycastLength;
    public bool SelectPressed => Scheme.PressSelect();
    public GraphicRaycaster GraphicRaycaster { get; private set; }
    public GameObject ReturnVirtualCursorPrefab => _virtualCursorPrefab;
    public GameObject OverAnyObject { get; set; }
    public Vector3 Position => CursorRect.transform.position;
    public InputScheme Scheme { get; private set; }
    public RectTransform CursorRect { get; private set; }
    private bool NoInput => Scheme.VcHorizontal() == 0 && Scheme.VcVertical() == 0 && !Scheme.PressSelect();
    public float Speed => _cursorSpeed;
    private bool Allow2D => _restrictRaycastTo == GameType._2D || _restrictRaycastTo == GameType.NoRestrictions;
    private bool Allow3D => _restrictRaycastTo == GameType._3D || _restrictRaycastTo == GameType.NoRestrictions;
    
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
        EVent.Do.Subscribe<ISetStartingCanvasOrder>(SetStartingCanvasOrder);
        EVent.Do.Subscribe<IAllowKeys>(SaveAllowKeys);
        EVent.Do.Subscribe<IVCActive>(DoSearchesOnCActivation);
        _interactWithUi.OnEnable();
    }

    private void SaveAllowKeys(IAllowKeys args)
    {
        _allowKeys = args.CanAllowKeys;
        if(Scheme.CanUseVirtualCursor == VirtualControl.No) return;
        
        if (_allowKeys)
        {
            if(Scheme.HideMouseCursor)
                _cursorCanvas.enabled = false;
            _raycastTo2D.WhenInMenu();
            _raycastTo3D.WhenInMenu();
            OverAnyObject = null;
        }
        else
        {
            _cursorCanvas.enabled = true;
        }
    }

    private void DoSearchesOnCActivation(IVCActive args)
    {
        CheckIfCursorOverGOUI();
        _interactWithUi.CheckIfCursorOverUI(this);
    }

    public void OnAwake(GOUIController controller)
    {
        _controller = controller;
        Scheme = _controller.GetScheme();
        CursorRect = _controller.NewVirtualCursor.GetComponent<RectTransform>();
        SetUpCursorCanvas();
        GraphicRaycaster = _controller.GetComponent<GraphicRaycaster>();
        
        ObserveEvents();
        _playerObjects = _controller.GetPlayerObjects();
        _raycastTo2D = EJect.Class.WithParams<I2DRaycast>(this);
        _raycastTo3D = EJect.Class.WithParams<I3DRaycast>(this);
        SetUpInGameObjects();

        _canStart = true;
    }

    private void SetUpCursorCanvas()
    {
        _cursorCanvas = CursorRect.GetComponent<Canvas>();

        if (Scheme.ControlType == ControlMethod.MouseOnly
            || Scheme.ControlType == ControlMethod.AllowBothStartWithMouse)
        {
            _cursorCanvas.enabled = true;
        }
        else
        {
            _cursorCanvas.enabled = false;
        }
    }

    private void SetUpInGameObjects()
    {
        foreach (var obj in _playerObjects)
        {
            obj.CheckForSetLayerMask(LayerToHit);
        }
    }

    public void Update()
    {
        if(!_canStart || _allowKeys || NoInput) return;
        
        if(HasSelectedBeenPressed()) return;
        
        //HasSelectedBeenPressed();
        
        _moveVirtualCursor.Move(this);
        _interactWithUi.CheckIfCursorOverUI(this);
    }

    public void FixedUpdate()
    {
        if(!_canStart | NoInput) return;
        
        CheckIfCursorOverGOUI();
    }

    private void CheckIfCursorOverGOUI()
    {
        if (Allow2D)
            _raycastTo2D.DoRaycast(CursorRect.position);
        if (Allow3D)
            _raycastTo3D.DoRaycast(CursorRect.position);
    }

    private bool HasSelectedBeenPressed()
    {
        if (_interactWithUi.UIObjectSelected(SelectPressed))
            return true;
        
        if (Allow2D)
            return _raycastTo2D.DoSelectedInGameObj();
        if (Allow3D)
            return _raycastTo3D.DoSelectedInGameObj();
        
        return false;
    }
}
