public interface IReturnToHome
{
    bool ActivateBranchOnReturnHome { get; }
} //**
public interface IPausePressed { } //**

public interface ICancelPressed //**
{
    EscapeKey EscapeKeySettings { get; }
}

public interface ISwitchGroupPressed //**
{
    SwitchType SwitchType { get; }
}
public interface IChangeControlsPressed { } //**

public interface IAllowKeys //**
{
    bool CanAllowKeys { get; }
} 

public interface ICancelButtonActivated: ICancelPopUp //**
{
    EscapeKey EscapeKeyType { get; }
} 
public interface ICancelPopUp
{
    UIBranch MyBranch { get; }
}

public interface IMenuGameSwitchingPressed { } //**

public interface IGameIsPaused//**
{
    bool GameIsPaused { get; }
}

public interface IHighlightedNode//**
{
    INode Highlighted { get; }
}

public interface ISelectedNode//**
{
    INode Selected { get; }
}

public interface IActiveBranch//**
{
    UIBranch ActiveBranch { get; }
}

public interface ISetUpStartBranches //**
{
    UIBranch StartBranch { get; }
} 
public interface IOnStart { }//**

public interface IOnHomeScreen//**
{
    bool OnHomeScreen { get; }
}

public interface IInMenu //**
{
    bool InTheMenu { get; set; }
}

public interface INoResolvePopUp //**
{
    bool ActiveResolvePopUps { get; }
}

public interface INoPopUps //**
{
    bool NoActivePopUps { get; }
}

public interface IRemoveOptionalPopUp //**
{
    UIBranch ThisPopUp { get; }
}

public interface IAddOptionalPopUp //**
{
    UIBranch ThisPopUp { get; }
}

public interface IAddResolvePopUp //**
{
    UIBranch ThisPopUp { get; }
}

public interface IClearScreen //**
{
    UIBranch IgnoreThisBranch { get; }
}

public interface IHotKeyPressed { }

