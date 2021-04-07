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
    private UINode _inGameUINode;

    private IsActive AlwaysOn { get; set; } = IsActive.No;

    private bool StartChildWhenActivated => _myGOUIModule.StartChildWhenActivated == IsActive.Yes;
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
            _myBranch.DontSetBranchAsActive();
            _myBranch.MoveToThisBranch();
            SetCanvas(ActiveCanvas.Yes);
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
        _mainCanvasRect = args.MainCanvas;
        _inGameUINode = (UINode) _myBranch.DefaultStartOnThisNode;
    }

    public bool CanStartBranch()
    {
        return !_myBranch.CanvasIsEnabled || !_myBranch.DefaultStartOnThisNode.HasChildBranch.CanvasIsEnabled;
    }

    public override void SetUpBranch(IBranch newParentController = null)
    {
        base.SetUpBranch(newParentController);
        _canvasOrderCalculator.SetCanvasOrder();
        
        if(AlwaysOn == IsActive.Yes && _canStart)
            MyBranch.DoNotTween();
        
        if(_myBranch.CanStartGOUI)
        {
            if(StartChildWhenActivated)
                _myBranch.DontSetBranchAsActive();
            SetCanvas(ActiveCanvas.Yes);
            ActivateChild();
        }
        else
        {
            if(_canStart)
            {
                _myGOUIModule.ExitInGameUi();
                _canvasOrderCalculator.ResetCanvasOrder();
            }        
        }

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
    
    private void ActivateChild()
    {
        if (!StartChildWhenActivated) return;
            
        _inGameUINode.OnPointerDown(null);
    }

    public override bool CanExitBranch() => AlwaysOn == IsActive.No;

    public override void StartBranchExit()
    {
        base.StartBranchExit();
        StopSettingPosition();
        DeactivateChild();
    }
    
    private void DeactivateChild()
    {
        if(_inGameUINode.HasChildBranch.CanvasIsEnabled)
            _inGameUINode.OnPointerDown(null);
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
