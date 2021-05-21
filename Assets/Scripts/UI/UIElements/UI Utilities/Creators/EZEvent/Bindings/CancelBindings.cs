using EZ.Events;

public class CancelBindings : EZEventBindingsBase
{
    protected override void EventsToBind()
    {
        //Cancel
        AutoRemove().CreateEvent<ICancelButtonActivated>();
        AutoRemove().CreateEvent<ICancelPopUp>();
        AutoRemove().CreateEvent<ICancelHoverOver>();
    }
}

public interface ICancelButtonActivated: ICancelPopUp
{
    EscapeKey EscapeKeyType { get; }
} 
public interface ICancelPopUp
{
    IBranch MyBranch { get; }
}

public interface ICancelHoverOver
{
    EscapeKey EscapeKeyType { get; }
}


