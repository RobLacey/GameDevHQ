using System;
using UIElements;
using UnityEngine;
using Object = UnityEngine.Object;

public interface IVirtualCursor
{
    IBranch OverAnyObject { get; set; }
    RectTransform CursorRect { get; }
    void OnEnable();
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
public class VirtualCursor : IRaycastController, IEventUser, IClearAll, IVirtualCursor, ICursorSettings
{
    public VirtualCursor(IVirtualCursorSettings settings)
    {
        Scheme = settings.ReturnScheme;
        _virtualCursorSetting = Scheme.ReturnVirtualCursorSettings;
        SetUpVirtualCursor(settings.GetParentTransform);
        OnAwake();
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
    
    //Variables
    private IRaycast _raycastTo2D, _raycastTo3D;
    private Canvas _cursorCanvas;
    private bool _allowKeys;
    private IInteractWithUi _interactWithUi;
    private IMoveVirtualCursor _moveVirtualCursor;
    private VirtualCursorSettings _virtualCursorSetting;
    private bool _canStart;

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
    private void OnAwake()
    {
        _raycastTo2D = EJect.Class.WithParams<I2DRaycast>(this);
        _raycastTo3D = EJect.Class.WithParams<I3DRaycast>(this);
        _moveVirtualCursor = EJect.Class.NoParams<IMoveVirtualCursor>();
        _interactWithUi = EJect.Class.NoParams<IInteractWithUi>();
        
        if(_virtualCursorSetting.OnlyHitInGameUi == IsActive.Yes)
            _interactWithUi.SetCanOnlyHitInGameObjects();
    }

    public void OnEnable()
    {
        ObserveEvents();
        _interactWithUi.OnEnable();
        _moveVirtualCursor.OnEnable();
    }

    public void ObserveEvents()
    {
        EVent.Do.Subscribe<ISetStartingCanvasOrder>(SetStartingCanvasOrder);
        EVent.Do.Subscribe<IAllowKeys>(SaveAllowKeys);
        EVent.Do.Subscribe<IVCSetUpOnStart>(SetCursorForStartUp);
        EVent.Do.Subscribe<IOnStart>(CanStart);
        EVent.Do.Subscribe<IVcChangeControlSetUp>(DoVCStartCheck);
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

    private void SetStartingCanvasOrder(ISetStartingCanvasOrder args) 
        => SetCanvasOrderUtil.Set(args.ReturnVirtualCursorCanvasOrder, _cursorCanvas);


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
