using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timed : IPopUp
{
    private UIBranch myBranch;
    bool _running;
    Coroutine _coroutine;
    //private UIData _uiData;
    private readonly UIControlsEvents _uiControlsEvents;

    public Timed(UIBranch branch)
    {
        myBranch = branch;
        _uiControlsEvents = new UIControlsEvents();
        OnEnable();
    }
    
    //Properties
    public bool GameIsPaused { get; private set; }
    public void IsGamePaused(bool paused) => GameIsPaused = paused;

    public void OnEnable()
    {
        _uiControlsEvents.SubscribeToGameIsPaused(IsGamePaused);
        //_uiData.IsGamePaused = IsGamePaused;
        //UIHub.GamePaused += IsGamePaused;
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
            // myBranch.DontSetAsActive = true;
            // myBranch.MoveToThisBranch();
            myBranch.MoveToThisBranchDontSetAsActive();
            _running = true;
        }
        yield return new WaitForSeconds(myBranch.Timer);
        _running = false;
        myBranch.StartOutTween();
    }

}
