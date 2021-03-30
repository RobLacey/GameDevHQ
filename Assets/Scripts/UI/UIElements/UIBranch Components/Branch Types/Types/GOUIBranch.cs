using System;
using System.Collections;
using UIElements;
using UnityEngine;

public interface IGOUIBranch: IBranchBase { }

public class GOUIBranch : BranchBase, IGOUIBranch
{
    private readonly Camera _mainCamera;
    private readonly RectTransform _myRectTransform;
    private RectTransform _mainCanvasRect;
    private Coroutine _coroutine;
    private GOUIModule _myGOUIModule;

    public GOUIBranch(IBranch branch) : base(branch)
    {
        _mainCamera = Camera.main;
        _myRectTransform = branch.MyCanvas.GetComponent<RectTransform>();
    }

    protected override void SaveIfOnHomeScreen(IOnHomeScreen args)
    {
        if(args.OnHomeScreen)
        {
            SetBlockRaycast(BlockRaycast.No);
            if(AlwaysOn == IsActive.Yes)
                SetCanvas(ActiveCanvas.Yes);
        }
        else
        {
            SetBlockRaycast(BlockRaycast.Yes);
        }
    }

    public override void ObserveEvents()
    {
        base.ObserveEvents();
        EVent.Do.Subscribe<ISetUpUIGOBranch>(SetUpGOUIParent);
    }
    
    //Main
    protected override void SetUpBranchesOnStart(ISetUpStartBranches args)
    {
        SetCanvas(ActiveCanvas.No);
        if(AlwaysOn == IsActive.Yes)
        {
            _myBranch.MoveToThisBranch();
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
        _myGOUIModule = args.UIGOModule;
        _mainCanvasRect = args.MainCanvas;
    }

    public override void SetUpBranch(IBranch newParentController = null)
    {
        base.SetUpBranch(newParentController);
        _canvasOrderCalculator.SetCanvasOrder();
        
        if (AlwaysOn == IsActive.Yes)
        {
            _myBranch.DontSetBranchAsActive();
        }
        
        if(_myBranch.CanvasIsEnabled)
        {
            _myBranch.DoNotTween();
        }        
        SetCanvas(ActiveCanvas.Yes);
        
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
        _coroutine = StaticCoroutine.StartCoroutine(SetMyScreenPosition(_myGOUIModule.UIGOTransform));
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
        var temp = _mainCamera.WorldToScreenPoint(objTransform.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_mainCanvasRect, temp, 
                                                                null, out var canvasPos);
        
        _myRectTransform.localPosition = canvasPos;
    }

}
