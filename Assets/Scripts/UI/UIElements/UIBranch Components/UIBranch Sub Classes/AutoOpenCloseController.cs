
using System;
using System.Collections;
using UnityEngine;

public class AutoOpenCloseController: IAutoOpenClose, IEventDispatcher, ICancelHoverOver, IEventUser
{
    public AutoOpenCloseController(IAutoOpenCloseData data)
    {
        _branch = data.ThisBranch;
        FetchEvents();
    }
    
    //Variables
    private readonly IBranch _branch;
    private bool _hotKeyPressed;

    //Properties
    public bool PointerOverBranch { get; private set; }
    public EscapeKey EscapeKeyType => EscapeKey.BackToHome;
    public IBranch ChildNodeHasOpenChild { private get; set; }
    
    //Set / Getters
    private void HotKeyPressed(IHotKeyPressed args) => _hotKeyPressed = true;
    public void OnPointerEnter() => PointerOverBranch = true;

    //Events
    private  Action<ICancelHoverOver> CancelHooverOver { get; set; }

    //Main
    public void OnEnable()
    {
        FetchEvents();
        ObserveEvents();
    }

    public void FetchEvents() => CancelHooverOver = EVent.Do.Fetch<ICancelHoverOver>();

    public void ObserveEvents() => EVent.Do.Subscribe<IHotKeyPressed>(HotKeyPressed);

    public void OnPointerExit()
    {
        PointerOverBranch = false;
        
        if(_branch.AutoClose == IsActive.No) return;
        if (HasHotKeyBeenPressed()) return;
        
        if (ChildNodeHasOpenChild != null)
        {
            StaticCoroutine.StartCoroutine(WaitForPointer());
            return;
        }

        StaticCoroutine.StartCoroutine(WaitForPointerNoChild());
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

