using EZ.Events;
using UIElements;
using UnityEngine;

public class GOUIEventsBindings : EZEventBindingsBase
{
    protected override void EventsToBind()
    {
        //UIGO System
        AutoRemove().CreateEvent<ICloseThisGOUIModule>();
        AutoRemove().CreateEvent<IStartGOUIBranch>();
        AutoRemove().CreateEvent<IOffscreen>();
    }
}

public interface IStartGOUIBranch
{
    IBranch TargetBranch { get; }
    GOUIModule ReturnGOUIModule { get; }
}

public interface ICloseThisGOUIModule
{
    IBranch TargetBranch { get; }
}

public interface IOffscreen
{
    IBranch TargetBranch { get; }
    bool IsOffscreen { get; }
}

