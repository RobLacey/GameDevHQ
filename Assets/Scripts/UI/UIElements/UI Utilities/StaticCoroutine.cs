using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StaticCoroutine
{
    private class CoroutineHolder : MonoBehaviour { }

    //lazy singleton pattern. Note it's not set to dontdestroyonload - you usually want corotuines to stop when you load a new scene.
    private static CoroutineHolder runner;
    private static CoroutineHolder Runner
    {
        get
        {
            if (runner == null)
            {
                runner = new GameObject("Static Coroutine Holder").AddComponent<CoroutineHolder>();
            }
            return runner;
        }
    }

    public static Coroutine StartCoroutine(IEnumerator coroutine)
    {
        return Runner.StartCoroutine(coroutine);
    }

    public static void StopCoroutines(Coroutine coroutine)
    {
        if (coroutine is null) return;
        Runner.StopCoroutine(coroutine);
    }
}

