using System;
using UnityEngine;

public interface ICancelOrBack : INodeBase { }


public class CancelOrBackButton : NodeBase, ICancelButtonActivated, ICancelOrBack, ICancelHoverOver
{
    public CancelOrBackButton(INode node) : base(node)
    {
        MyBranch = node.MyBranch;
        _isPopUp = MyBranch.IsAPopUpBranch();
        EscapeKeyType = node.EscapeKeyType;
    }

    private readonly bool _isPopUp;
    public EscapeKey EscapeKeyType { get; }

    //Events
    private Action<ICancelButtonActivated> CancelButtonActive { get; set; }
    private Action<ICancelPopUp> CancelPopUp { get; set; }
    private Action<ICancelHoverOver> CancelHoverOver { get; set; }

    //Main
    public override void FetchEvents()
    {
        base.FetchEvents();
        CancelButtonActive = EVent.Do.Fetch<ICancelButtonActivated>();
        CancelPopUp= EVent.Do.Fetch<ICancelPopUp>();
        CancelHoverOver= EVent.Do.Fetch<ICancelHoverOver>();
    }

    protected override void TurnNodeOnOff()
    {
        if (CloseOnExit())
        {
            CancelHoverOver?.Invoke(this);
        }
        else if (_isPopUp)
        {
            CancelPopUp?.Invoke(this);
        }
        else
        {
            CancelButtonActive?.Invoke(this);
        }
        
        bool CloseOnExit() => MyBranch.AutoOpenCloseClass.CanAutoClose();        

    }
}

