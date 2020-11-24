public interface ICancelOrBack : INodeBase { }


public class CancelOrBackButton : NodeBase, ICancelButtonActivated, ICancelOrBack
{
    private readonly bool _isPopUp;
    
    public EscapeKey EscapeKeyType { get; }
    
    private static CustomEvent<ICancelButtonActivated> CancelButtonActive { get; } 
        = new CustomEvent<ICancelButtonActivated>();
    private static CustomEvent<ICancelPopUp> CancelPopUp { get; } 
        = new CustomEvent<ICancelPopUp>();
    private static CustomEvent<ICancelHoverOver> CancelHoverOver { get; } 
        = new CustomEvent<ICancelHoverOver>();

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
            CancelHoverOver?.RaiseEvent(this);
        }
        else if (_isPopUp)
        {
            CancelPopUp?.RaiseEvent(this);
        }
        else
        {
            CancelButtonActive?.RaiseEvent(this);
        }
    }

    public override void DeactivateNode() { }
}

