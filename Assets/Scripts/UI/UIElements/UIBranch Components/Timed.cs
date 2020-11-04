using System.Collections;
using UnityEngine;

public class Timed : BranchBase, IStartPopUp
{
    public Timed(UIBranch branch) : base(branch)
    {
        _myBranch.OnStartPopUp = StartPopUp;
    }

    //Variables
    private bool _running;
    private Coroutine _coroutine;

    public void StartPopUp()
    {
        if (_gameIsPaused || !_canStart || _resolvePopUps) return;
        
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
        }
        else
        {
            _myBranch.SetNoTween();
        }
    }

    public override void SetUpBranch(UIBranch newParentController = null)
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
        _running = false;
        _myBranch.StartBranchExitProcess(OutTweenType.Cancel);
    }
}
