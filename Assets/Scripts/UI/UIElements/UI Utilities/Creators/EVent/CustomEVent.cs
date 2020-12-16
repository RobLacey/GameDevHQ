using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomEVent<TType>
{
    private event Action<TType> Raise;
    private int? _currentScene;
    private bool _autoRemove;

    public void RaiseEvent(TType args) => Raise?.Invoke(args);


    public CustomEVent<TType> AutoRemove()
    {
        Application.quitting += ClearUp;
        _autoRemove = true;
        return this;
    }
    
    public void AddListener(Action<TType> newEvent)
    {
        if(_currentScene is null && _autoRemove)
        {
            _currentScene = SceneManager.GetActiveScene().buildIndex;
            SceneManager.activeSceneChanged += RemoveAllListeners;
        }
        
        Raise -= newEvent;
        Raise += newEvent;
    }

    public void RemoveListener(Action<TType> newEvent)
    {
        Raise -= newEvent;
    }

    private void RemoveAllListeners(Scene current, Scene next)
    {
        if (_currentScene == next.buildIndex) return;
        ClearUp();
    }

    private void ClearUp()
    {
        Raise = null;
        SceneManager.activeSceneChanged -= RemoveAllListeners;
        Application.quitting -= ClearUp;
    }
}