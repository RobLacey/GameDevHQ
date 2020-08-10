using System;
using UnityEngine;

/// <summary>
/// This partial Class looks after calling to the UITweener
/// </summary>
public partial class UIBranch
{
    public void StartOutTween(Action action = null)
    {
        _branchEvents?._onBranchExit.Invoke();
        _onFinishedTrigger = action;
        _uiTweener.StopAllCoroutines();
        _myCanvasGroup.blocksRaycasts = false;
        _uiTweener.DeactivateTweens(OutTweenCallback);
    }

    private void OutTweenCallback()
    {
        _myCanvas.enabled = false;
        _onFinishedTrigger?.Invoke();
    }

    private void ActivateInTweens()
    {
        if (IsOptionalPopUp && !_noActiveResolvePopUps)
        {
            _myCanvasGroup.blocksRaycasts = false;
            _setAsActive = false;
        }
        else
        {
            _myCanvasGroup.blocksRaycasts = true;
        }

        _uiTweener.ActivateTweens(InTweenCallback);
    }

    private void InTweenCallback()
    {
        if (_setAsActive)
        {
            LastHighlighted.SetNodeAsActive();
        }

        _branchEvents?._onBranchEnter.Invoke();
        _setAsActive = true;
    }
}