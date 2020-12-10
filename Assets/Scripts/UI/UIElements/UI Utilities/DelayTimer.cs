
using System;
using System.Collections;
using UnityEngine;

public class DelayTimer : IDelayTimer
{
    private float _delay;
    private bool _delaySet;
    private Coroutine _coroutine;
    private Action _callback;

    public IDelayTimer SetDelay(float delay)
    {
        _delaySet = true;
        _delay = delay;
        return this;
    }
    public void StartTimer(Action callBack)
    {
        CheckForException(callBack);
        
        _callback = callBack;
        _coroutine = StaticCoroutine.StartCoroutine(Timer());
    }

    private void CheckForException(Action callBack)
    {
        if (!_delaySet)
            throw new Exception($"No Delay set : {callBack.Method}");
    }

    private IEnumerator Timer()
    {
        yield return new WaitForSeconds(_delay);
        _delaySet = false;
        _callback.Invoke();
    }

    public void StopTimer() => StaticCoroutine.StopCoroutines(_coroutine);
}

public interface IDelayTimer
{
    void StartTimer(Action callBack);
    IDelayTimer SetDelay(float delay);
    void StopTimer();
}
