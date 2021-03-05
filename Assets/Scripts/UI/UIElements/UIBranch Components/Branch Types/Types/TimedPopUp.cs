using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITimedPopUpBranch : IBranchBase { }

public class TimedPopUp : BranchBase, ITimedPopUpBranch, IAdjustCanvasOrder
{
    public TimedPopUp(IBranch branch) : base(branch)
    {
        SetUpCanvasOrder(branch);
    }

    private void SetUpCanvasOrder(ICanvasOrder branch)
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

    //Main
    public override bool CanStartBranch()
    {
        if (_gameIsPaused || !_canStart || _activeResolvePopUps) return false;

        SetIfRunningOrNot();
        _myBranch.DontSetBranchAsActive();
        return true;
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

    public override void EndOfBranchStart()
    {
        base.EndOfBranchStart();
        CanvasOrderCalculator.ResetCanvasOrder(_myBranch, _myCanvas);
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

    private void AdjustCanvasOrderAdded()
    {
        timedPopUps.Add(_myBranch.MyCanvas);
        CanvasOrderCalculator.ProcessActiveCanvasses(timedPopUps, CanvasOrderOffset);
    }

    private void AdjustCanvasOrderRemoved(ILastRemovedPopUp args = null)
    {
        timedPopUps.Remove(_myBranch.MyCanvas);
        CanvasOrderCalculator.ProcessActiveCanvasses(timedPopUps, CanvasOrderOffset);
    }
}
