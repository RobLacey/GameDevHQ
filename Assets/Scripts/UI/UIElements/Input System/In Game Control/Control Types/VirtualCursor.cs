using System;
using UIElements;
using UnityEngine;
using Object = UnityEngine.Object;

public interface IVirtualCursor
{
    void OnEnable();
    bool CanMoveVirtualCursor();
    void PreStartMovement();
    void Update();
    void FixedUpdate();
    GameObject OverAnyObject { get; }
}

[Serializable]
public class VirtualCursor : IRaycastController, IEventUser, IClearAll, IVirtualCursor
{
    public VirtualCursor(IVirtualCursorSettings settings)
    {
        Scheme = settings.ReturnScheme;
        _virtualCursorSetting = Scheme.ReturnVirtualCursorSettings;
        SetUpVirtualCursor(settings.GetParentTransform);
        OnAwake();
    }
    
    public void SetUpVirtualCursor(Transform transform)
    {
        var newVirtualCursor = Object.Instantiate(VirtualCursorPrefab, transform, true);
        CursorRect = newVirtualCursor.GetComponent<RectTransform>();
        CursorRect.anchoredPosition3D = Vector3.zero;
        SetUpCursorCanvas();
    }
    
    private void SetUpCursorCanvas()
    {
        _cursorCanvas = CursorRect.GetComponent<Canvas>();
        _cursorCanvas.enabled = Scheme.CanUseVirtualCursor;
    }
    
    //Variables
    private IRaycast _raycastTo2D, _raycastTo3D;
    private Canvas _cursorCanvas;
    private bool _allowKeys;
    private IInteractWithUi _interactWithUi = EJect.Class.NoParams<IInteractWithUi>();
    private IMoveVirtualCursor _moveVirtualCursor = EJect.Class.NoParams<IMoveVirtualCursor>();
    private VirtualCursorSettings _virtualCursorSetting;

    //Properties & Setters / Getters
    public LayerMask LayerToHit => _virtualCursorSetting.LayerToHit;
    public float LaserLength => _virtualCursorSetting.RaycastLength;
    public bool SelectPressed => Scheme.PressSelect();
    private GameObject VirtualCursorPrefab => _virtualCursorSetting.VirtualCursorPrefab;
    public GameObject OverAnyObject { get; set; }
    public Vector3 Position => CursorRect.transform.position;
    public InputScheme Scheme { get; private set; }
    public RectTransform CursorRect { get; private set; }
    private bool HasInput => Scheme.VcHorizontal() != 0 || Scheme.VcVertical() != 0 || Scheme.PressSelect();
    public float Speed => _virtualCursorSetting.CursorSpeed;
    private bool Allow2D => _virtualCursorSetting.RestrictRaycastTo == GameType._2D 
                            || _virtualCursorSetting.RestrictRaycastTo == GameType.NoRestrictions;
    private bool Allow3D => _virtualCursorSetting.RestrictRaycastTo == GameType._3D 
                            || _virtualCursorSetting.RestrictRaycastTo == GameType.NoRestrictions;

    //Main
    private void OnAwake()
    {
        _raycastTo2D = EJect.Class.WithParams<I2DRaycast>(this);
        _raycastTo3D = EJect.Class.WithParams<I3DRaycast>(this);
        
        if(_virtualCursorSetting.OnlyHitInGameUi == IsActive.Yes)
            _interactWithUi.CanOnlyHitInGameObjects();
    }

    public void OnEnable()
    {
        ObserveEvents();
        _interactWithUi.OnEnable();
    }

    public void ObserveEvents()
    {
        EVent.Do.Subscribe<ISetStartingCanvasOrder>(SetStartingCanvasOrder);
        EVent.Do.Subscribe<IAllowKeys>(SaveAllowKeys);
        EVent.Do.Subscribe<IVCSetUpOnStart>(SetCursorForStartUp);
    }

    private void SetStartingCanvasOrder(ISetStartingCanvasOrder args)
    {
        var storedCondition = _cursorCanvas.enabled;
        _cursorCanvas.enabled = true;
        _cursorCanvas.overrideSorting = true;
        _cursorCanvas.sortingOrder = args.ReturnVirtualCursorCanvasOrder();
        _cursorCanvas.enabled = storedCondition;
    }

    private void SaveAllowKeys(IAllowKeys args)
    {
        _allowKeys = args.CanAllowKeys;
        if(!Scheme.CanUseVirtualCursor) return;
        
        if (_allowKeys)
        {
            if(Scheme.HideMouseCursor)
                _cursorCanvas.enabled = false;
            _raycastTo2D.WhenInMenu();
            _raycastTo3D.WhenInMenu();
            OverAnyObject = null;
            _interactWithUi.CloseLastHitNodeAsDifferent();
        }
        else
        {
            _cursorCanvas.enabled = true;
            ActivateCursor();
        }
    }

    private void SetCursorForStartUp(IVCSetUpOnStart args)
    {
        if(!Scheme.CanUseVirtualCursor) return;
        
        if (args.ShowCursorOnStart)
        {
            ActivateCursor();
        }
        else
        {
            _cursorCanvas.enabled = false;
        }
    }

    private void ActivateCursor()
    {
        _cursorCanvas.enabled = true;
        CheckIfCursorOverGOUI();
        _interactWithUi.CheckIfCursorOverUI(this);
    }

    public bool CanMoveVirtualCursor()
    {
        var canUseVirtualCursor = Scheme.CanUseVirtualCursor && !_allowKeys && HasInput;
        return canUseVirtualCursor;
    }

    public void Update()
    {
        if(HasSelectedBeenPressed()) return;
        _moveVirtualCursor.Move(this);
        _interactWithUi.CheckIfCursorOverUI(this);
    }

    public void PreStartMovement()
    {
        if (Scheme.CanUseVirtualCursor && !HasInput)
            _moveVirtualCursor.Move(this);
    }

    public void FixedUpdate()
    {
        if(OverAnyObject) return;
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
