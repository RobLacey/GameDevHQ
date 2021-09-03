using EZ.Events;

public class InputBindings : EZEventBindingsBase
{
    protected override void EventsToBind()
    {
        //Input
        AutoRemove().CreateEvent<IPausePressed>();
        AutoRemove().CreateEvent<ISwitchGroupPressed>();
        AutoRemove().CreateEvent<IHotKeyPressed>();
        AutoRemove().CreateEvent<IMenuGameSwitchingPressed>();
        AutoRemove().CreateEvent<ICancelPressed>();

        //ChangeControl
        AutoRemove().CreateEvent<IAllowKeys>();
        AutoRemove().CreateEvent<IChangeControlsPressed>();
        AutoRemove().CreateEvent<IChangeControlsSwitchPressed>();
        AutoRemove().CreateEvent<IVCSetUpOnStart>();
        AutoRemove().CreateEvent<IVcChangeControlSetUp>();
    }
}

public interface IPausePressed { } // This one is test

public interface ISwitchGroupPressed  { }

public interface IHotKeyPressed
{
    INode ParentNode { get; }
    IBranch MyBranch { get; }
}

public interface IMenuGameSwitchingPressed { }

public interface IAllowKeys // This one is test
{
    bool CanAllowKeys { get; }
}

public interface IChangeControlsPressed { }
public interface IChangeControlsSwitchPressed : IChangeControlsPressed { }

public interface IVCSetUpOnStart
{
    bool ShowCursorOnStart { get; }
}

public interface IVcChangeControlSetUp{ }

public interface ICancelPressed // This one is test
{
    EscapeKey EscapeKeySettings { get; }
}





