public interface IReturnToHome
{
    ActivateNodeOnReturnHome ActivateOnReturnHome { get; }
} 
public interface IPausePressed { }

public interface ICancelPressed 
{
    EscapeKey EscapeKeySettings { get; }
}

public interface ISwitchGroupPressed
{
    SwitchType SwitchType { get; }
}
public interface IChangeControlsPressed { }

public interface IAllowKeys
{
    bool CanAllowKeys { get; }
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
public interface ICancelHoverOverButton { }

public interface IMenuGameSwitchingPressed { }

public interface IGameIsPaused
{
    bool GameIsPaused { get; }
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

public interface ISetUpStartBranches 
{
    IBranch StartBranch { get; }
} 
public interface IOnStart { }

public interface IOnHomeScreen
{
    bool OnHomeScreen { get; }
}

public interface IClearScreen 
{
    IBranch IgnoreThisBranch { get; }
}

public interface IInMenu
{
    bool InTheMenu { get; }
}

public interface INoResolvePopUp
{
    bool ActiveResolvePopUps { get; }
}

public interface INoPopUps 
{
    bool NoActivePopUps { get; }
}

public interface IRemoveOptionalPopUp 
{
    IBranch ThisPopUp { get; }
}


public interface IAddOptionalPopUp
{
    IBranch ThisPopUp { get; }
}

public interface IAddResolvePopUp
{
    IBranch ThisPopUp { get; }
}

public interface IHotKeyPressed
{
    INode ParentNode { get; }
    IBranch MyBranch { get; }
}

public interface IDisabledNode
{
    INode ToThisDisabledNode { get; }
    bool IsDisabled { get; set; }
    bool IsThisNodeIsDisabled();
}

public interface ISceneChange { }




