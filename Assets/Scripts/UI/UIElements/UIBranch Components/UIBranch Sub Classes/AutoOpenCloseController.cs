
using System;
using System.Collections;
using EZ.Events;
using UnityEngine;


public class AutoOpenCloseController: IAutoOpenClose, IEZEventDispatcher, ICancelHoverOver, IEZEventUser
{
    public AutoOpenCloseController(IAutoOpenCloseData data)
    {
        _branch = data.ThisBranch;
    }
    
    //Variables
    private readonly IBranch _branch;
    private bool _hotKeyPressed;

    private static Coroutine runningCoroutine;

    //Properties
    public bool PointerOverBranch { get; private set; }
    public EscapeKey EscapeKeyType => _branch.EscapeKeyType;
    public IBranch ChildNodeHasOpenChild { private get; set; }

    //Set / Getters
    private void HotKeyPressed(IHotKeyPressed args) => _hotKeyPressed = true;
    public void OnPointerEnter()
    {
        if (runningCoroutine.IsNotNull())
        {
            StaticCoroutine.StopCoroutines(runningCoroutine);
        }
        PointerOverBranch = true;
    }

    //Events
    private  Action<ICancelHoverOver> CancelHooverOver { get; set; }

    //Main
    public void OnEnable()
    {
        FetchEvents();
        ObserveEvents();
    }

    public void OnDisable()
    {
        UnObserveEvents();
        CancelHooverOver = null;
        StaticCoroutine.StopCoroutines(runningCoroutine);
    }
    
    public void FetchEvents() => CancelHooverOver = CancelEvents.Do.Fetch<ICancelHoverOver>();

    public void ObserveEvents() => InputEvents.Do.Subscribe<IHotKeyPressed>(HotKeyPressed);
    
    public void UnObserveEvents() => InputEvents.Do.Unsubscribe<IHotKeyPressed>(HotKeyPressed);

    public void OnPointerExit()
    {
        PointerOverBranch = false;
        
        if(_branch.AutoClose == IsActive.No) return;
        
        if (HasHotKeyBeenPressed()) return;
        
        if (ChildNodeHasOpenChild != null)
        {
            runningCoroutine = StaticCoroutine.StartCoroutine(WaitForPointer());
            return;
        }

        runningCoroutine = StaticCoroutine.StartCoroutine(WaitForPointerNoChild());
    }

    private bool HasHotKeyBeenPressed()
    {
        if (_hotKeyPressed)
        {
            _hotKeyPressed = false;
            return true;
        }
        return false;
    }

    private IEnumerator WaitForPointer()
    {
        yield return new WaitForSeconds(_branch.AutoCloseDelay);
        if (!ChildNodeHasOpenChild.PointerOverBranch && !_branch.MyParentBranch.PointerOverBranch)
            CloseBranch();
    }

    private IEnumerator WaitForPointerNoChild()
    {
        yield return new WaitForEndOfFrame();
        if (!_branch.MyParentBranch.PointerOverBranch)
            CloseBranch();
    }

    private void CloseBranch()
    {
        ChildNodeHasOpenChild = null;
        CancelHooverOver?.Invoke(this);
    }

}

