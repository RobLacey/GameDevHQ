public class CancelOrBackButton : INodeBase, ICancelButtonActivated
{
    public EscapeKey EscapeKeyType { get; }
    
    private static CustomEvent<ICancelButtonActivated> CancelButtonActive { get; } 
        = new CustomEvent<ICancelButtonActivated>();

    public CancelOrBackButton(ICancelButtonActivated node) => EscapeKeyType = node.EscapeKeyType;

    public void TurnNodeOnOff() => CancelButtonActive?.RaiseEvent(this);

    public void Start() { }

    public void DeactivateNode() { }
}

