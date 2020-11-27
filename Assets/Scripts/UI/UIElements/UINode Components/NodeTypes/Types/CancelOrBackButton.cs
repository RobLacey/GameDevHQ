using System;

public interface ICancelOrBack : INodeBase { }


public class CancelOrBackButton : NodeBase, ICancelButtonActivated, ICancelOrBack
{
    private readonly bool _isPopUp;
    
    public EscapeKey EscapeKeyType { get; }

    private Action<ICancelButtonActivated> CancelButtonActive { get; } 
        = EVent.Do.FetchEVent<ICancelButtonActivated>();
    private Action<ICancelPopUp> CancelPopUp { get; } = EVent.Do.FetchEVent<ICancelPopUp>();
    private Action<ICancelHoverOver> CancelHoverOver { get; } = EVent.Do.FetchEVent<ICancelHoverOver>();

    public CancelOrBackButton(INode node) : base(node)
    {
        MyBranch = node.MyBranch;
        _isPopUp = MyBranch.IsAPopUpBranch();
        EscapeKeyType = node.EscapeKeyType;
    }

    protected override void TurnNodeOnOff()
    {
        if (EscapeKeyType == EscapeKey.HoverClose)
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

    public override void DeactivateNode() { }
}

