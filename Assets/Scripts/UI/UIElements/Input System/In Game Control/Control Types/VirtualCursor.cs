using System;
using UIElements;
using UnityEngine;
using Object = UnityEngine.Object;

public interface IVirtualCursor :IMonoEnable, IMonoStart
{
    IBranch OverAnyObject { get; set; }
    RectTransform CursorRect { get; }
    bool CanMoveVirtualCursor();
    void PreStartMovement();
    void Update();
}

public interface ICursorSettings
{
    RectTransform CursorRect { get; }
    float Speed { get; }
    Vector3 Position { get; }
}

[Serializable]
public class VirtualCursor : IRaycastController, IEventUser, IVirtualCursor, ICursorSettings, IEServUser, IMonoAwake
{
    public VirtualCursor(IVirtualCursorSettings settings)
    {
        _parentTransform = settings.GetParentTransform;
        OnAwake();
    }
    
    //Variables
    private IRaycast _raycastTo2D, _raycastTo3D;
    private Canvas _cursorCanvas;
    private bool _allowKeys;
    private IInteractWithUi _interactWithUi;
    private IMoveVirtualCursor _moveVirtualCursor;
    private ISetCanvasOrder _setCanvasOrder;
    private VirtualCursorSettings _virtualCursorSetting;
    private bool _canStart;
    private Transform _parentTransform;

    //Properties & Setters / Getters
    public LayerMask LayerToHit => _virtualCursorSetting.LayerToHit;
    public float LaserLength => _virtualCursorSetting.RaycastLength;
    public bool SelectPressed => Scheme.PressSelect();
    public IBranch OverAnyObject { get; set; }
    public Vector3 Position => CursorRect.transform.position;
    public RectTransform CursorRect { get; private set; }
    public float Speed => _virtualCursorSetting.CursorSpeed;
    private GameObject VirtualCursorPrefab => _virtualCursorSetting.VirtualCursorPrefab;
    private InputScheme Scheme { get; set; }
    private bool HasInput => (Scheme.VcHorizontalPressed() || Scheme.VcVerticalPressed()) 
                             || Scheme.VcHorizontal() != 0 || Scheme.VcVertical() != 0;
    private bool Allow2D => _virtualCursorSetting.RestrictRaycastTo == GameType._2D 
                            || _virtualCursorSetting.RestrictRaycastTo == GameType.NoRestrictions;
    private bool Allow3D => _virtualCursorSetting.RestrictRaycastTo == GameType._3D 
                            || _virtualCursorSetting.RestrictRaycastTo == GameType.NoRestrictions;
    private void CanStart(IOnStart args) => _canStart = true;
    private void SaveAllowKeys(IAllowKeys args) => _allowKeys = args.CanAllowKeys;

    //Main
    public void OnAwake()
    {
        _raycastTo2D = EJect.Class.NoParams<I2DRaycast>();
        _raycastTo3D = EJect.Class.NoParams<I3DRaycast>();
        _moveVirtualCursor = EJect.Class.NoParams<IMoveVirtualCursor>();
        _interactWithUi = EJect.Class.NoParams<IInteractWithUi>();
        
        UseEServLocator();
    }
    
    public void UseEServLocator()
    {
        Scheme = EServ.Locator.Get<InputScheme>(this);
        _setCanvasOrder = EServ.Locator.Get<ISetCanvasOrder>(this);
    }

    public void OnEnable()
    {
        ObserveEvents();
        _interactWithUi.OnEnable();
        _moveVirtualCursor.OnEnable();
        _raycastTo2D.OnEnable();
        _raycastTo3D.OnEnable();
    }

    public void ObserveEvents()
    {
        EVent.Do.Subscribe<IAllowKeys>(SaveAllowKeys);
        EVent.Do.Subscribe<IVCSetUpOnStart>(SetCursorForStartUp);
        EVent.Do.Subscribe<IOnStart>(CanStart);
        EVent.Do.Subscribe<IVcChangeControlSetUp>(DoVCStartCheck);
    }
    
    public void OnStart()
    {
        _virtualCursorSetting = Scheme.ReturnVirtualCursorSettings;
        SetUpVirtualCursor(_parentTransform);
        
        if(_virtualCursorSetting.OnlyHitInGameUi == IsActive.Yes)
            _interactWithUi.SetCanOnlyHitInGameObjects();
        
        _raycastTo2D.OnStart();
        _raycastTo3D.OnStart();
        _moveVirtualCursor.OnStart();
        SetStartingCanvasOrder();
    }
    
    private void SetUpVirtualCursor(Transform transform)
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


    private void DoVCStartCheck(IVcChangeControlSetUp args)
    {
        if(!Scheme.CanUseVirtualCursor) return;
        
        if (_allowKeys)
        {
            TurnOffAndResetCursor();
        }
        else
        {
            ActivateCursor();
        }
    }

    private void SetStartingCanvasOrder() 
        => SetCanvasOrderUtil.Set(_setCanvasOrder.ReturnVirtualCursorCanvasOrder, _cursorCanvas);


    private void TurnOffAndResetCursor()
    {
        if (Scheme.HideMouseCursor)
            _cursorCanvas.enabled = false;
        _raycastTo2D.WhenInMenu();
        _raycastTo3D.WhenInMenu();
        OverAnyObject = null;
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
        var canUseVirtualCursor = Scheme.CanUseVirtualCursor && !_allowKeys && (HasInput || SelectPressed);
        return canUseVirtualCursor;
    }

    public void Update()
    {
        if(HasSelectedBeenPressed()) return;
        _moveVirtualCursor.Move(this);
        if(OverAnyObject.IsNull())
            CheckIfCursorOverGOUI();
        _interactWithUi.CheckIfCursorOverUI(this);
    }
    
    private bool HasSelectedBeenPressed() => _interactWithUi.UIObjectSelected(SelectPressed);

    public void PreStartMovement()
    {
        if(_canStart) return;
        if (Scheme.CanUseVirtualCursor && !HasInput)
            _moveVirtualCursor.Move(this);
    }

    private void CheckIfCursorOverGOUI()
    {
        if (Allow2D)
            _raycastTo2D.DoRaycast(CursorRect.position);
        if (Allow3D)
            _raycastTo3D.DoRaycast(CursorRect.position);
    }
}
