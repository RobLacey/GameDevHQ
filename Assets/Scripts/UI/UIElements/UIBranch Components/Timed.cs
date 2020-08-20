using System.Collections;
using UnityEngine;

public class Timed : IPopUp
{
    public Timed(UIBranch branch)
    {
        _myBranch = branch;
        OnEnable();
    }

    private readonly UIBranch _myBranch;
    private bool _running;
    private Coroutine _coroutine;
    private readonly UIControlsEvents _uiControlsEvents = new UIControlsEvents();

    //Properties
    private bool GameIsPaused { get; set; }
    private void IsGamePaused(bool paused) => GameIsPaused = paused;

    private void OnEnable()
    {
        _uiControlsEvents.SubscribeToGameIsPaused(IsGamePaused);
    }

    public void StartPopUp()
    {
        if (GameIsPaused) return;

        StaticCoroutine.StopCoroutines(_coroutine);
        _coroutine = StaticCoroutine.StartCoroutine(TimedPopUpProcess());
    }

    public void MoveToNextPopUp(UIBranch lastBranch = null)
    {
        //Maybe Need for quick exit button
    }

    private IEnumerator TimedPopUpProcess()
    {
        if (!_running)
        {
            //TODO Fix 
           // _myBranch.MoveToThisBranchDontSetAsActive();
            _running = true;
        }
        yield return new WaitForSeconds(_myBranch.Timer);
        _running = false;
        _myBranch.StartOutTweenProcess();
    }
}
