using System;
using System.Collections;
using UIElements;
using UnityEngine;

public interface IGOUIBranch: IBranchBase { }

public class GOUIBranch : BranchBase, IGOUIBranch
{
    private readonly Camera _mainCamera;
    private readonly RectTransform _rectTransform;
    private Coroutine _coroutine;
    private GOUIModule _myUIGOModule;

    public GOUIBranch(IBranch branch) : base(branch)
    {
        _mainCamera = Camera.main;
        _rectTransform = _myBranch.MyCanvas.GetComponent<RectTransform>();
    }

    protected override void SaveIfOnHomeScreen(IOnHomeScreen args)
    {
        if(args.OnHomeScreen)
        {
            SetBlockRaycast(BlockRaycast.No);
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
    private void SetUpGOUIParent(ISetUpUIGOBranch args)
    {
        if(args.TargetBranch != _myBranch || _myUIGOModule.IsNotNull()) return;
        _myUIGOModule = args.UIGOModule;
    }

    public override void SetUpBranch(IBranch newParentController = null)
    {
        base.SetUpBranch(newParentController);
        _myBranch.DontSetBranchAsActive(); 
        SetCanvas(ActiveCanvas.Yes);
        
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
        _coroutine = StaticCoroutine.StartCoroutine(SetMyScreenPosition(_myUIGOModule.UsersTransform));
    }

    public override void StartBranchExit()
    {
        base.StartBranchExit();
        StopSettingPosition();
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
        => _rectTransform.anchoredPosition3D = _mainCamera.WorldToScreenPoint(objTransform.position);
}
