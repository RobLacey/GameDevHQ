using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StaticCoroutine
{
    private class CoroutineHolder : MonoBehaviour { }

    //lazy singleton pattern. Note it's not set to dontdestroyonload - you usually want corotuines to stop when you load a new scene.
    private static CoroutineHolder _runner;
    private static CoroutineHolder runner
    {
        get
        {
            if (_runner == null)
            {
                _runner = new GameObject("Static Corouine Holder").AddComponent<CoroutineHolder>();
            }
            return _runner;
        }
    }

    public static Coroutine StartCoroutine(IEnumerator corotuine)
    {
        return runner.StartCoroutine(corotuine);
    }

    public static void StopCoroutines(Coroutine coroutine)
    {
        runner.StopCoroutine(coroutine);
    }
}

