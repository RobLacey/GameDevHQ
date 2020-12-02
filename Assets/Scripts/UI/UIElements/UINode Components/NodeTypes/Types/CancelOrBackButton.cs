using System;

public interface ICancelOrBack : INodeBase { }


public class CancelOrBackButton : NodeBase, ICancelButtonActivated, ICancelOrBack, IEventDispatcher
{
    private readonly bool _isPopUp;
    
    public EscapeKey EscapeKeyType { get; }

    //Events
    private Action<ICancelButtonActivated> CancelButtonActive { get; set; }
    private Action<ICancelPopUp> CancelPopUp { get; set; }
    private Action<ICancelHoverOver> CancelHoverOver { get; set; }

    public override void Start() => FetchEvents();

    public void FetchEvents()
    {
        CancelButtonActive = EVent.Do.Fetch<ICancelButtonActivated>();
        CancelPopUp= EVent.Do.Fetch<ICancelPopUp>();
        CancelHoverOver= EVent.Do.Fetch<ICancelHoverOver>();
    }

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

