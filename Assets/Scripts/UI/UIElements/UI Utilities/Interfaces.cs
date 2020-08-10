// ReSharper disable once CheckNamespace

internal interface INodeData
{
    UINode LastHighlighted { get; }
    UINode LastSelected { get; }
    void SaveHighlighted(UINode newNode);
    void SaveSelected(UINode newNode);
}

internal interface IBranchData
{
    UIBranch ActiveBranch { get; }
    void SaveActiveBranch(UIBranch newBranch);
}

public interface IHUbData
{
    bool GameIsPaused { get; }
   void IsGamePaused(bool paused);
}

public interface IMono
{
    void OnEnable();
    void OnDisable();
}

public interface IStartPopUp
{
    void StartPopUp();
    void RestoreLastPosition(UINode lastHomeGroupNode = null);
}

public interface IPauseMenu
{
    void StartPauseMenu(bool isGamePaused);
    void OnDisable();
}

public interface IPopUp : IMono
{
    void StartPopUp();
    void MoveToNextPopUp(UINode lastNode = null);
}

public interface IPopUpControls
{
    bool NoActivePopUps { get; }
    void ActivateCurrentPopUp();
    void RemoveNextPopUp();
}


