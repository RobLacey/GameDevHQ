using System;
using System.Collections;
using UIElements;
using UnityEngine;

public interface IInGameUi: IBranchBase { }

public class InGameUI : BranchBase, IInGameUi
{
    private readonly Camera _mainCamera;
    private readonly RectTransform _rectTransform;
    private Coroutine _coroutine;
    private InGameObjectUI _currentObjUser;

    public InGameUI(IBranch branch) : base(branch)
    {
        _myBranch.StartInGamePopUp += StartPopUp;
        _myBranch.ExitPopUp += ExitPopUp;
        _mainCamera = Camera.main;
        _rectTransform = _myBranch.MyCanvas.GetComponent<RectTransform>();
    }

    private void SaveActiveUiObject(IActiveInGameObject args) 
        => _currentObjUser = args.IsNull() ? null : args.ActiveObject;

    public override void ObserveEvents()
    {
        base.ObserveEvents();
        EVent.Do.Subscribe<IActiveInGameObject>(SaveActiveUiObject);
    }

    protected override void SaveInMenu(IInMenu args)
    {
        base.SaveInMenu(args);
        if(_currentObjUser is null) return;
        if (args.InTheMenu)
        {
            ExitPopUp();
        }
    }

    private void StartPopUp(InGameObjectUI newUser)
    {
        if (ActivateOnFirstUse(NextUser, newUser)) return;
        
        ExitProcess(NextUser);
        
        void NextUser() => ToNext(newUser);
    }

    private bool ActivateOnFirstUse(Action callBack, InGameObjectUI newUser)
    {
        if(_currentObjUser is null)
        {
            _currentObjUser = newUser;
            callBack?.Invoke();
            return true;
        }
        return false;
    }

    private void ToNext(InGameObjectUI newUser)
    {
        _currentObjUser = newUser;
        StaticCoroutine.StopCoroutines(_coroutine);
        _myBranch.MoveToThisBranch();
        _coroutine = StaticCoroutine.StartCoroutine(SetMyScreenPosition(_currentObjUser.UsersTransform));
    }

    public override void SetUpBranch(IBranch newParentController = null)
    {
        _myBranch.DontSetBranchAsActive(); 
        SetCanvas(ActiveCanvas.Yes);
    }

    private void ExitPopUp() => ExitProcess(StopSettingPosition);

    private void ExitProcess(Action callback)
    { 
        _currentObjUser.SetAsNotActive();
        _currentObjUser.MyBranch.StartBranchExitProcess(OutTweenType.Cancel, callback);
    }

    protected override void ClearBranchForFullscreen(IClearScreen args)
    {
        base.ClearBranchForFullscreen(args);
        if(_currentObjUser.IsNull()) return;
        _currentObjUser.SetAsNotActive();
        _currentObjUser.CancelUi();
    }

    private void StopSettingPosition() => StaticCoroutine.StopCoroutines(_coroutine);

    private IEnumerator SetMyScreenPosition(Transform objTransform)
    {
        while (true)
        {
            SetPosition(objTransform);
            yield return null;
        }
    }

    private void SetPosition(Transform objTransform) 
        => _rectTransform.anchoredPosition3D = _mainCamera.WorldToScreenPoint(objTransform.position);
}
