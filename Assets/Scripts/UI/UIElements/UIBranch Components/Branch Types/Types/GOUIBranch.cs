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
    private RectTransform _mainCanvasRect;
    private Coroutine _coroutine;
    private IGOUIModule _myGOUIModule;
    private Transform _inGameObjectPosition;
    private Vector3 _inGameObjectLastFrameScreenPosition;
    private bool _canStartGOUI;


    //Properties & Getters / Setters
    private bool AlwaysOn => _myGOUIModule.AlwaysOnIsActive;
    

    protected override void SaveIfOnHomeScreen(IOnHomeScreen args)
    {
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
    
    //Main
    public override void ObserveEvents()
    {
        base.ObserveEvents();
        EVent.Do.Subscribe<ISetUpUIGOBranch>(SetUpGOUIParent);
        EVent.Do.Subscribe<IStartGOUIBranch>(StartGOUIBranch);
        EVent.Do.Subscribe<IOffscreen>(SetPositionWhenOffScreen);
    }

    protected override void SetUpBranchesOnStart(ISetUpStartBranches args)
    {
        if(AlwaysOn)
        {
            _myBranch.DontSetBranchAsActive();
            _myBranch.MoveToThisBranch();
            SetCanvas(ActiveCanvas.Yes);
            SetBlockRaycast(BlockRaycast.Yes);
        }        
        else
        {
            base.SetUpBranchesOnStart(args);
        }
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
        _mainCanvasRect = EServ.Locator.Get<IHub>(this).MainCanvasRect;
    }

    public override void SetUpBranch(IBranch newParentController = null)
    {
        base.SetUpBranch(newParentController);
        _canvasOrderCalculator.SetCanvasOrder();
        
        if(_myBranch.CanvasIsEnabled)
            _myBranch.DoNotTween();
        
        SetCanvas(ActiveCanvas.Yes);
        SetBlockRaycast(BlockRaycast.Yes);
        
        if((AlwaysOn || !_canStartGOUI) && _canStart)
            MyBranch.DoNotTween();

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

    public override bool CanExitBranch() => !AlwaysOn;

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

    public override void SetBlockRaycast(BlockRaycast active)
    {
        if (OnHomeScreen)
        {
            _myCanvasGroup.blocksRaycasts = active == BlockRaycast.Yes;
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
        
        if(currentScreenPos == _inGameObjectLastFrameScreenPosition) return;
        
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_mainCanvasRect, currentScreenPos, 
                                                                null, out var canvasPos);
        
        _myRectTransform.localPosition = canvasPos;
        _inGameObjectLastFrameScreenPosition = currentScreenPos;
    }
}
