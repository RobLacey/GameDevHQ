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

  //  private bool _active;
   // private InGameObjectUI _myUIGO;

    public InGameUI(IBranch branch) : base(branch)
    {
        //_myBranch.StartInGamePopUp += StartPopUp;
        //_myBranch.ExitPopUp += ExitPopUp;
        _mainCamera = Camera.main;
        _rectTransform = _myBranch.MyCanvas.GetComponent<RectTransform>();
    }

    private void SaveActiveUiObject(IActiveInGameObject args) 
          => _currentObjUser = args.IsNull() ? null : args.ActiveObject;

    public override void ObserveEvents()
    {
        base.ObserveEvents();
        EVent.Do.Subscribe<IActiveInGameObject>(SaveActiveUiObject);
        InGameObjectUI.StartUIGO += StartPopUp;
        InGameObjectUI.ExitUIGO += ExitUIGO;
        //InGameObjectUI.SetUpUIGO += SetUpUIGO;
    }

    // private void SetUpUIGO(InGameObjectUI inGameObjectUI, IBranch uiBranchToUse)
    // {
    //     if (uiBranchToUse == _myBranch)
    //     {
    //         _myUIGO = inGameObjectUI;
    //     }
    // }


    protected override void SaveInMenu(IInMenu args)
    {
        base.SaveInMenu(args);
        if(_currentObjUser is null) return;
        if (args.InTheMenu)
        {
            ExitUIGO();
        }
    }

    public override void SetUpBranch(IBranch newParentController = null)
    {
        _myBranch.DontSetBranchAsActive(); 
        SetCanvas(ActiveCanvas.Yes);
    }

    private void StartPopUp(InGameObjectUI newUIGO, IBranch UIGOBranch)
    {
        Debug.Log(_myBranch);
        if(UIGOBranch != _myBranch /*|| _active*/) return;
        if (_currentObjUser != newUIGO && _currentObjUser.IsNotNull())
        {
            Debug.Log("Exit");
            ExitActiveUIGOAndStart(Start);
        }
        else
        {
            Debug.Log("Start");
            _currentObjUser = newUIGO;
            StartMyUIGO(newUIGO);

        }
        //if (NoUIGOActive(StartMyUIGO, newUIGO)) return;

        void Start() => StartMyUIGO(newUIGO);
    }

    private bool NoUIGOActive(Action callBack, InGameObjectUI newUIGO)
    {
        if(_currentObjUser is null)
        {
            Debug.Log("Here");
            _currentObjUser = newUIGO;
            callBack?.Invoke();
            return true;
        }
            Debug.Log("Or Not");
        return false;
    }

    private void StartMyUIGO(InGameObjectUI newUIGO)
    {
      //  _active = true;
        _currentObjUser = newUIGO;
        StaticCoroutine.StopCoroutines(_coroutine);
        _myBranch.MoveToThisBranch();
        _coroutine = StaticCoroutine.StartCoroutine(SetMyScreenPosition(_currentObjUser.UsersTransform));
    }

    private void ExitUIGO()
    {
       // if(!_active) return;
        ExitActiveUIGOAndStart(StopSettingPosition);
    }

    private void ExitActiveUIGOAndStart(Action callback)
    {
        //_active = false;
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
