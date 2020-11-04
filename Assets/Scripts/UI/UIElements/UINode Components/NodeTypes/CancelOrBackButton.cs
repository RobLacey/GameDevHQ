public class CancelOrBackButton : NodeBase, ICancelButtonActivated
{
    private readonly bool _isPopUp;
    
    public EscapeKey EscapeKeyType { get; }
    public UIBranch MyBranch { get; }

    private static CustomEvent<ICancelButtonActivated> CancelButtonActive { get; } 
        = new CustomEvent<ICancelButtonActivated>();
    private static CustomEvent<ICancelPopUp> CancelPopUp { get; } 
        = new CustomEvent<ICancelPopUp>();

    public CancelOrBackButton(ICancelButtonActivated node) : base((UINode)node)
    {
        MyBranch = node.MyBranch;
        _isPopUp = MyBranch.IsAPopUpBranch();
        EscapeKeyType = node.EscapeKeyType;
    }

    //public override void Start() { }
    
    protected override void TurnNodeOnOff()
    {
        if (_isPopUp)
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

