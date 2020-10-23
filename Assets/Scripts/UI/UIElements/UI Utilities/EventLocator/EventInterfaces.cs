public interface IReturnToHome { } //**
public interface IPausePressed { } //**
public interface ICancelPressed { } //**

public interface ISwitchGroupPressed //**
{
    SwitchType SwitchType { get; }
}
public interface IChangeControlsPressed { } //**

public interface IAllowKeys //**
{
    bool CanAllowKeys { get; }
} 

public interface ICancelButtonActivated //**
{
    EscapeKey EscapeKeyType { get; }
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

public interface IInMenu
{
    bool InTheMenu { get; set; }
}

public interface INoResolvePopUp
{
    bool NoActiveResolvePopUps { get; }
}
public interface INoPopUps { }
public interface IMoveToNextFromPopUp { }
public interface IRemoveOptionalPopUp { }
public interface IAddOptionalPopUp { }
public interface IAddResolvePopUp { }
public interface IBackToNextPopUp { }
public interface IReturnNextPopUp { }

public interface IClearScreen //**
{
    UIBranch IgnoreThisBranch { get; }
}
