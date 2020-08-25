// ReSharper disable once CheckNamespace

using System;

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

public interface ITriggeredPopUp
{
    void StartPopUp();
}

public interface IPauseMenu
{
    void StartPauseMenu(bool isGamePaused);
}

public interface IPopUpMove
{
    void MoveToNextPopUp(UIBranch lastBranch = null);
}




