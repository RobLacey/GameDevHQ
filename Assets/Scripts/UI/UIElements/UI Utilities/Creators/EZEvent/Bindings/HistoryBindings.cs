using EZ.Events;using UIElements;

public class HistoryBindings : EZEventBindingsBase
{
    protected override void EventsToBind()
    {
        //History
        AutoRemove().CreateEvent<IReturnToHome>();
        AutoRemove().CreateEvent<IOnHomeScreen>();
        AutoRemove().CreateEvent<IOnStart>();
        AutoRemove().CreateEvent<IGameIsPaused>();
        AutoRemove().CreateEvent<IInMenu>();
        AutoRemove().CreateEvent<IReturnHomeGroupIndex>();
        
        //Node
        AutoRemove().CreateEvent<IHighlightedNode>();
        AutoRemove().CreateEvent<ISelectedNode>();
        AutoRemove().CreateEvent<IDisabledNode>();
        AutoRemove().CreateEvent<IHistoryData>();
        AutoRemove().CreateEvent<ISceneIsChanging>();
        
        //Branch
        AutoRemove().CreateEvent<IActiveBranch>();
    }
}

public interface IReturnToHome { }

public interface IOnHomeScreen
{
    bool OnHomeScreen { get; }
}
public interface IOnStart { }

public interface IGameIsPaused
{
    bool GameIsPaused { get; }
}

public interface IInMenu
{
    bool InTheMenu { get; }
}

public interface IHighlightedNode
{
    INode Highlighted { get; }
}

public interface ISelectedNode
{
    INode UINode { get; }
}

public interface IActiveBranch
{
    IBranch ActiveBranch { get; }
}

public interface IDisabledNode: IMonoEnable, IMonoDisable
{
    INode ThisIsTheDisabledNode { get; }
    bool IsDisabled { get; set; }
    bool IsThisNodeIsDisabled();
}

public interface IHistoryData //TODO Remove
{
    INode NodeToUpdate { get; }
}

public interface IReturnHomeGroupIndex
{
    INode TargetNode { set; }
}

public interface ISceneIsChanging { }




