using System;

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
        if (!IsAPopUpBranch() && _uIHub.CanStart) MyCanvasGroup.blocksRaycasts = true;
        if (IsAPopUpBranch()) PopUpClass.ManagePopUpResolve();

        if (!DontSetAsActive)
        {
            SaveLastHighlighted(LastHighlighted);
            LastHighlighted.SetNodeAsActive();
        }

        _branchEvents?._onBranchEnter.Invoke();
        DontSetAsActive = false;
    }
}