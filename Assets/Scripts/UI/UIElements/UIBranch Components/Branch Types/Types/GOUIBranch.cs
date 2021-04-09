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
    private Vector3 _inGameObjectLastFramePosition;

    
    //Properties & Getters / Setters
    private IsActive AlwaysOn { get; set; } = IsActive.No;

    protected override void SaveIfOnHomeScreen(IOnHomeScreen args)
    {
        if(args.OnHomeScreen)
        {
            if(AlwaysOn == IsActive.Yes)
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
    
    //Main
    public override void ObserveEvents()
    {
        base.ObserveEvents();
        EVent.Do.Subscribe<ISetUpUIGOBranch>(SetUpGOUIParent);
    }

    protected override void SetUpBranchesOnStart(ISetUpStartBranches args)
    {
        SetCanvas(ActiveCanvas.No);
        
        if(AlwaysOn == IsActive.Yes)
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
        AlwaysOn = args.AlwaysOn;
        _myGOUIModule = args.ReturnGOUIModule;
        _inGameObjectPosition = args.UIGOTransform;
    }

    public override void UseEServLocator()
    {
        base.UseEServLocator();
        _mainCanvasRect = EServ.Locator.Get<IHub>(this).MainCanvas;
    }

    public override bool CanStartBranch()
    {
        return !_myBranch.CanvasIsEnabled || !_myBranch.DefaultStartOnThisNode.HasChildBranch.CanvasIsEnabled;
    }

    public override void SetUpBranch(IBranch newParentController = null)
    {
        base.SetUpBranch(newParentController);
        _canvasOrderCalculator.SetCanvasOrder();
        
        if(AlwaysOn == IsActive.Yes && _canStart)
            MyBranch.DoNotTween();
        
        SetCanvas(ActiveCanvas.Yes);
        SetBlockRaycast(BlockRaycast.Yes);

        StartMyUIGO();
    }

    protected override void ClearBranchForFullscreen(IClearScreen args)
    {
        base.ClearBranchForFullscreen(args);
        _myGOUIModule.ExitInGameUi();
        _canvasOrderCalculator.ResetCanvasOrder();
    }

    private void StartMyUIGO()
    {
        StaticCoroutine.StopCoroutines(_coroutine);
        _coroutine = StaticCoroutine.StartCoroutine(SetMyScreenPosition(_inGameObjectPosition));
    }
    
    public override bool CanExitBranch() => AlwaysOn == IsActive.No;

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
        if(objTransform.position == _inGameObjectLastFramePosition) return;

        var position = objTransform.position;
        var temp = _mainCamera.WorldToScreenPoint(position);
        
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_mainCanvasRect, temp, 
                                                                null, out var canvasPos);
        
        _myRectTransform.localPosition = canvasPos;
        _inGameObjectLastFramePosition = position;
    }
}
