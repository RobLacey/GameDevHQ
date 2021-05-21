using EZ.Events;

public class PopUpBindings : EZEventBindingsBase
{
    protected override void EventsToBind()
    {
        //PopUps
        AutoRemove().CreateEvent<IRemoveOptionalPopUp>();
        AutoRemove().CreateEvent<IAddOptionalPopUp>();
        AutoRemove().CreateEvent<IAddResolvePopUp>();
        AutoRemove().CreateEvent<INoResolvePopUp>();
        AutoRemove().CreateEvent<INoPopUps>();
        AutoRemove().CreateEvent<ILastRemovedPopUp>();
    }
}


public interface INoResolvePopUp // This one is test
{
    bool ActiveResolvePopUps { get; }
}

public interface INoPopUps // This one is test
{
    bool NoActivePopUps { get; }
}

public interface IRemoveOptionalPopUp // This one is test
{
    IBranch ThisPopUp { get; }
}

public interface ILastRemovedPopUp
{
    IBranch LastOptionalPopUp { get; }
}

public interface IAddOptionalPopUp // This one is test
{
    IBranch ThisPopUp { get; }
}

public interface IAddResolvePopUp // This one is test
{
    IBranch ThisPopUp { get; }
}
