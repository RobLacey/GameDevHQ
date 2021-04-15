using System;
using System.Collections;
using UnityEngine;

public static class StaticCoroutine
{
    private class CoroutineHolder : MonoBehaviour
    {
        private void OnDisable()
        {
            StopAllCoroutines();
            Clear();
        }

        private void OnApplicationQuit()
        {
            StopAllCoroutines();
            Clear();
        }
    }

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

    private static void Clear()
    {
        runner.StopAllCoroutines();
    }
}

