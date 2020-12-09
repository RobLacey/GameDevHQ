
using System;
using System.Collections;
using UnityEngine;

public class AutoOpenCloseController: IAutoOpenClose, IEventDispatcher, ICancelHoverOver
{
    public AutoOpenCloseController(IAutoOpenCloseData data)
    {
        AutoOpenCloseSettings = data.AutoOpenClose;
        _branch = data.ThisBranch;
        FetchEvents();
    }
    
    private readonly IBranch _branch;
    
    //Properties
    private AutoOpenClose AutoOpenCloseSettings {get; }
    public bool PointerOverBranch { get; private set; }

    public EscapeKey EscapeKeyType => EscapeKey.BackToHome;
    public IBranch ChildNodeHasOpenChild { private get; set; }
    public bool CanAutoClose() => AutoOpenCloseSettings == AutoOpenClose.Both 
                                  || AutoOpenCloseSettings == AutoOpenClose.Close;
    public bool CanAutoOpen() => AutoOpenCloseSettings == AutoOpenClose.Both 
                                 || AutoOpenCloseSettings == AutoOpenClose.Open;

    //Events
    private  Action<ICancelHoverOver> CancelHooverOver { get; set; }


    public void FetchEvents() => CancelHooverOver = EVent.Do.Fetch<ICancelHoverOver>();

    public void OnPointerEnter() => PointerOverBranch = true;

    public void OnPointerExit()
    {
        PointerOverBranch = false;
        
        if(!CanAutoClose()) return;
        
        if (ChildNodeHasOpenChild != null)
        {

            StaticCoroutine.StartCoroutine(WaitForPointer());
            return;
        }
        StaticCoroutine.StartCoroutine(WaitForPointerNoChild());

    }
    private IEnumerator WaitForPointer()
    {
        yield return new WaitForEndOfFrame();
        if (!ChildNodeHasOpenChild.PointerOverBranch && !_branch.MyParentBranch.PointerOverBranch)
        {
            Debug.Log($"Close Child as not over it or oNot Over Parent : {this}");
            CloseBranch();
        }
    }

    private IEnumerator WaitForPointerNoChild()
    {
        yield return new WaitForEndOfFrame();
        if (!_branch.MyParentBranch.PointerOverBranch)
        {
            Debug.Log($"No Child and NOT Over parent : {this}");
            CloseBranch();
        }
    }

    private void CloseBranch()
    {
        ChildNodeHasOpenChild = null;
        CancelHooverOver?.Invoke(this);
    }

}

