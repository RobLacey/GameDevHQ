using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;

public class TestRunner : MonoBehaviour
{
    [SerializeField] EventsForTest _eventsForTest;

    [System.Serializable]
    private class EventsForTest
    {
        [SerializeField] public UnityEvent _event1;
        [SerializeField] public UnityEvent _event2;
        [SerializeField] public UnityEvent _event3;
        [SerializeField] public UnityEvent _event4;
        [SerializeField] public UnityEvent _event5;
    }

    [Button ()]
    public void Button_Event1()
    {
        _eventsForTest._event1?.Invoke();
    }
    [Button]
    public void Button_Event2()
    {
        _eventsForTest._event2?.Invoke();
    }
    [Button]
    public void Button_Event3()
    {
        _eventsForTest._event3?.Invoke();
    }
    [Button]
    public void Button_Event4()
    {
        _eventsForTest._event4?.Invoke();
    }
    [Button]
    public void Button_Event5()
    {
        _eventsForTest._event5?.Invoke();
    }
}
