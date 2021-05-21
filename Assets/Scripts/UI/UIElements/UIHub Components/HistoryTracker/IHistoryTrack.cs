﻿using System;
using EZ.Inject;
using EZ.Service;

public interface IHistoryTrack : IParameters, IIsAService
{
    void OnEnable();
    bool IsPaused { get; }
    bool NoHistory { get; }
    IHistoryManagement HistoryListManagement { get; }
    void GOUIBranchHasClosed(IBranch branchToClose, IGOUIModule module = null);
    void BackToHome();
    void BackOneLevel();
    void MoveToLastBranchInHistory();
    void CheckForPopUpsWhenCancelPressed(Action endOfCancelAction);
    void UpdateHistoryData(INode node);
    void BackToHomeScreen();
}