public class CancelOrBackButton : NodeBase, ICancelButtonActivated
{
    private readonly bool _isPopUp;
    
    public EscapeKey EscapeKeyType { get; }
    
    private static CustomEvent<ICancelButtonActivated> CancelButtonActive { get; } 
        = new CustomEvent<ICancelButtonActivated>();
    private static CustomEvent<ICancelPopUp> CancelPopUp { get; } 
        = new CustomEvent<ICancelPopUp>();
    private static CustomEvent<ICancelHoverOver> CancelHoverOver { get; } 
        = new CustomEvent<ICancelHoverOver>();

    public CancelOrBackButton(ICancelButtonActivated node) : base((UINode)node)
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
        //_uiActions._playCancelAudio?.Invoke();
        _uiNode.DoPress();
    }

    public override void DeactivateNode() { }
}

