using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITimedPopUpBranch : IBranchBase { }

public class TimedPopUp : BranchBase, IStartPopUp, ITimedPopUpBranch, IAdjustCanvasOrder
{
    public TimedPopUp(IBranch branch) : base(branch)
    {
        _myBranch.OnStartPopUp += StartPopUp;
        SetUpCanvasOrder(branch);
    }

    public void SetUpCanvasOrder(ICanvasOrder branch)
    {
        EVent.Do.Return<IAdjustCanvasOrder>(this);
        branch.ManualCanvasOrder = CanvasOrderOffset;
        branch.CanvasOrder = OrderInCanvas.Manual;
        timedPopUps = new List<Canvas>();
    }

    //Variables
    private bool _running;
    private Coroutine _coroutine;
    private static List<Canvas> timedPopUps;

    //Properties
    public int CanvasOrderOffset { protected get; set; }
    public BranchType BranchType { get; } = BranchType.TimedPopUp;
    
    public void StartPopUp()
    {
        if (_gameIsPaused || !_canStart || _activeResolvePopUps) return;

        SetIfRunningOrNot();
        _myBranch.DontSetBranchAsActive();
        _myBranch.MoveToThisBranch();
    }

    private void SetIfRunningOrNot()
    {
        if (!_running)
        {
            SetCanvas(ActiveCanvas.Yes);
            _running = true;
            AdjustCanvasOrderAdded();
        }
        else
        {
            _myBranch.DoNotTween();
        }
    }

    public override void SetUpBranch(IBranch newParentController = null)
    {
        StaticCoroutine.StopCoroutines(_coroutine);
        _coroutine = StaticCoroutine.StartCoroutine(TimedPopUpProcess());
    }

    private IEnumerator TimedPopUpProcess()
    {
        yield return new WaitForSeconds(_myBranch.Timer);
        ExitTimedPopUp();
    }
    
    private void ExitTimedPopUp()
    {
        AdjustCanvasOrderRemoved();
        _running = false;
        _myBranch.StartBranchExitProcess(OutTweenType.Cancel);
    }
    
    public void AdjustCanvasOrderAdded()
    {
        timedPopUps.Add(_myBranch.MyCanvas);
        CanvasOrderCalculator.ProcessActiveCanvasses(timedPopUps, CanvasOrderOffset);
    }

    public void AdjustCanvasOrderRemoved(ILastRemovedPopUp args = null)
    {
        timedPopUps.Remove(_myBranch.MyCanvas);
        CanvasOrderCalculator.ProcessActiveCanvasses(timedPopUps, CanvasOrderOffset);
    }
}
