using EZ.Events;

public class PopUpBindings : EZEventBindingsBase
{
    protected override void EventsToBind()
    {
        //PopUps
        AutoRemove().CreateEvent<IClearOptionalPopUp>();
        AutoRemove().CreateEvent<IAddOptionalPopUp>();
        AutoRemove().CreateEvent<IAddResolvePopUp>();
        AutoRemove().CreateEvent<INoResolvePopUp>();
        AutoRemove().CreateEvent<INoPopUps>();
    }
}


public interface INoResolvePopUp // This one is test
{
    bool NoActiveResolvePopUps { get; }
}

public interface INoPopUps // This one is test
{
    bool NoActivePopUps { get; }
}

public interface IClearOptionalPopUp // This one is test
{
    IBranch ThisPopUp { get; }
}

public interface IAddOptionalPopUp // This one is test
{
    IBranch ThisPopUp { get; }
}

public interface IAddResolvePopUp // This one is test
{
    IBranch ThisPopUp { get; }
}
