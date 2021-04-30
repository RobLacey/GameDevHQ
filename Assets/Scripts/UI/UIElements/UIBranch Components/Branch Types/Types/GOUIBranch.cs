using System.Collections;
using UnityEngine;

public interface IGOUIBranch: IBranchBase { }

public class GOUIBranch : BranchBase, IGOUIBranch
{
    public GOUIBranch(IBranch branch) : base(branch)
    {
        _mainCamera = Camera.main;
        _myRectTransform = branch.MyCanvas.GetComponent<RectTransform>();
    }
    
    //Variables
    private readonly Camera _mainCamera;
    private readonly RectTransform _myRectTransform;
    private Coroutine _coroutine;
    private IGOUIModule _myGOUIModule;
    private Transform _inGameObjectPosition;
    private Vector3 _inGameObjectLastFrameScreenPosition;
    private bool _canStartGOUI;
    private IHub _myUiHub;


    //Properties & Getters / Setters
    private bool AlwaysOn => _myGOUIModule.AlwaysOnIsActive;

    public override void ObserveEvents()
    {
        base.ObserveEvents();
        EVent.Do.Subscribe<ISetUpUIGOBranch>(SetUpGOUIParent);
        EVent.Do.Subscribe<IStartGOUIBranch>(StartGOUIBranch);
        EVent.Do.Subscribe<ICloseAndResetBranch>(CloseAndReset);
        EVent.Do.Subscribe<IOffscreen>(SetPositionWhenOffScreen);
    }

    protected override void SaveIfOnHomeScreen(IOnHomeScreen args)
    {
        base.SaveIfOnHomeScreen(args);
        
        if(_myGOUIModule.IsNull()) return;
        
        if(args.OnHomeScreen)
        {
            if(AlwaysOn)
            {
                SetCanvas(ActiveCanvas.Yes);
                SetBlockRaycast(BlockRaycast.Yes);
            }        
        }
        else
        {
            SetBlockRaycast(BlockRaycast.No);
        }
    }

    private void StartGOUIBranch(IStartGOUIBranch args)
    {
        if (ReferenceEquals(args.TargetBranch, _myBranch))
        {
            _canStartGOUI = true;
        }
    }

    private void CloseAndReset(ICloseAndResetBranch args)
    {
        if(args.TargetBranch.NotEqualTo(_myBranch)) return;
        
        _canStartGOUI = false;
        _myCanvasGroup.blocksRaycasts = false;
        SetCanvas(ActiveCanvas.No);
        StopSettingPosition();
    }

    public override void SetBlockRaycast(BlockRaycast active)
    {
        if (OnHomeScreen)
        {
            _myCanvasGroup.blocksRaycasts = active == BlockRaycast.Yes;
        }
    }

    public override void SetCanvas(ActiveCanvas active)
    {
        if(!OnHomeScreen) return;
        base.SetCanvas(active);
    }

    private void SetUpGOUIParent(ISetUpUIGOBranch args)
    {
        if(args.TargetBranch != _myBranch || _myGOUIModule.IsNotNull()) return;
        _myGOUIModule = args.ReturnGOUIModule;
        _inGameObjectPosition = args.GOUITransform;
    }

    public override void UseEServLocator()
    {
        base.UseEServLocator();
        _myUiHub = EServ.Locator.Get<IHub>(this);
    }

    //Main
    public override bool CanStartBranch() => _canStartGOUI || AlwaysOn || CanAllowKeys;

    public override void SetUpBranch(IBranch newParentController = null)
    {
        bool AlwaysOnActivated() => AlwaysOn && _myBranch.CanvasIsEnabled;

        base.SetUpBranch(newParentController);
        _canvasOrderCalculator.SetCanvasOrder();
        
        if(_myBranch.CanvasIsEnabled || AlwaysOnActivated() || !_canStartGOUI )
        {
            _myBranch.DoNotTween();
        }
        
        if(_myGOUIModule.PointerOver)
        {
            SetCanvas(ActiveCanvas.Yes);
            SetBlockRaycast(BlockRaycast.Yes);
        }  
        
        StartMyUIGO();
    }

    protected override void ClearBranchForFullscreen(IClearScreen args)
    {
        base.ClearBranchForFullscreen(args);
        _canvasOrderCalculator.ResetCanvasOrder();
    }

    private void StartMyUIGO()
    {
        StaticCoroutine.StopCoroutines(_coroutine);
        _coroutine = StaticCoroutine.StartCoroutine(SetMyScreenPosition(_inGameObjectPosition));
    }

    public override void EndOfBranchStart()
    {
        base.EndOfBranchStart();
        _canStartGOUI = false;
    }

    public override bool CanExitBranch(OutTweenType outTweenType)
    {
        base.CanExitBranch(outTweenType);
        if (outTweenType == OutTweenType.Cancel)
        {
            _canStartGOUI = true;
        }
        return !AlwaysOn && !_myGOUIModule.PointerOver;
    }

    public override void StartBranchExit()
    {
        base.StartBranchExit();
        StopSettingPosition();
    }
    
    public override void EndOfBranchExit()
    {
        base.EndOfBranchExit();
        _canvasOrderCalculator.ResetCanvasOrder();
    }

    protected override void WhenControlsChange(IActivateBranchOnControlsChange args)
    {
        if(args.ActiveBranch != _myBranch) return;
        
        if (CanAllowKeys)
        {
            if(_myBranch.CanvasIsEnabled)
            {
                _myBranch.MoveToThisBranch();
            }
            else
            {
                _myGOUIModule.SwitchEnter();
            }
        }
        else
        {
            if(_myBranch.CanvasIsEnabled)
                _myGOUIModule.SwitchExit();
        }
    }
    
    private void StopSettingPosition() => StaticCoroutine.StopCoroutines(_coroutine);
    
    private void SetPositionWhenOffScreen(IOffscreen args)
    {
        if(args.TargetBranch.NotEqualTo(_myBranch)) return;
        
        if(args.IsOffscreen)
        {
            StaticCoroutine.StopCoroutines(_coroutine);
        }        
        else
        {
            StartMyUIGO();
        }
    }

    private IEnumerator SetMyScreenPosition(Transform objTransform)
    {
        while (true)
        {
            SetPosition(objTransform);
            yield return null;
        }
    }

    private void SetPosition(Transform objTransform)
    {
        var position = objTransform.position;
        var currentScreenPos = _mainCamera.WorldToScreenPoint(position);
        var mainCanvasRectTransform = _myUiHub.MainCanvasRect;
        
        if(currentScreenPos == _inGameObjectLastFrameScreenPosition) return;
        
        RectTransformUtility.ScreenPointToLocalPointInRectangle(mainCanvasRectTransform, currentScreenPos, 
                                                                null, out var canvasPos);
        
        _myRectTransform.localPosition = canvasPos;
        _inGameObjectLastFrameScreenPosition = currentScreenPos;
    }
}
