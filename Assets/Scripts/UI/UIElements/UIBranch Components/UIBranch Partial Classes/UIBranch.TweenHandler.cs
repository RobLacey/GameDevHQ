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
        MyCanvasGroup.blocksRaycasts = false;
        _uiTweener.DeactivateTweens(OutTweenCallback);
    }

    private void OutTweenCallback()
    {
        MyCanvas.enabled = false;
        _onFinishedTrigger?.Invoke();
    }

    private void ActivateInTweens()
    {
        MyCanvasGroup.blocksRaycasts = false;
        _uiTweener.ActivateTweens(InTweenCallback);
    }

    private void InTweenCallback()
    {
        if (!IsAPopUpBranch()) MyCanvasGroup.blocksRaycasts = true;
        if (IsNonResolvePopUp && !_noActiveResolvePopUps)
        {
            DontSetAsActive = true;
        }

        if (!DontSetAsActive)
        {
            MyCanvasGroup.blocksRaycasts = true;
            LastHighlighted.SetNodeAsActive();
            SetAsActiveBranch();
        }

        _branchEvents?._onBranchEnter.Invoke();
        DontSetAsActive = false;
    }
}