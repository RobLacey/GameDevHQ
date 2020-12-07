using System;

public interface ICancelOrBack : INodeBase { }


public class CancelOrBackButton : NodeBase, ICancelButtonActivated, ICancelOrBack, ICancelHoverOver
{
    public CancelOrBackButton(INode node) : base(node)
    {
        MyBranch = node.MyBranch;
        _isPopUp = MyBranch.IsAPopUpBranch();
        _closeOnExit = node.MyBranch.CloseHooverOnExit;
        EscapeKeyType = node.EscapeKeyType;
    }

    private readonly bool _isPopUp;
    private readonly bool _closeOnExit;
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
        if (_closeOnExit)
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
    }
}

