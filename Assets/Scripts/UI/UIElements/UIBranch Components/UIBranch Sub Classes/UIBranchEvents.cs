using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class BranchEvents : IBranchEvents
{
    [SerializeField] private UnityEvent _onBranchEnter;
    [SerializeField] private UnityEvent _onBranchExit;
    
    public void OnBranchEnter() => _onBranchEnter?.Invoke();

    public void OnBranchExit() => _onBranchExit?.Invoke();
}

public interface IBranchEvents
{
    void OnBranchEnter();
    void OnBranchExit();
}