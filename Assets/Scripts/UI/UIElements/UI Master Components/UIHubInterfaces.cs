using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIHubInterfaces
{
}
public interface IHubData : IUIHistory, IPopUpData, IPauseData
{
    bool OnHomeScreen { get; set; }
    List<UIBranch> HomeGroupBranches { get; }
    UIBranch[] AllBranches { get; }
    int GroupIndex { get; set; }
    EscapeKey GlobalEscape { get; }

    UIHomeGroup UIHomeGroup { get; }
}

public interface IUIHistory
{
    UINode LastSelected { get; }
    UINode LastHighlighted { get; }
    UIBranch ActiveBranch { get; set; }

    //Methods
    void SetLastSelected(UINode NewNode);
    void SetLastHighlighted(UINode newNode);
}

public interface IPopUpData
{
    UINode LastNodeBeforePopUp { get; set; }
    List<UIBranch> ActivePopUps_Resolve { get; }
    List<UIBranch> ActivePopUps_NonResolve { get; }
    int PopIndex { get; set; }

    //Methods
    void HandleActivePopUps();
}

public interface IPauseData
{
    bool GameIsPaused { get; set; }

    //Methods
    void PauseOptionMenu();
}

public interface ICancel
{
   // void Cancel();
    //void CancelOrBackButton(EscapeKey escapeKey);
    bool CanCancel();

}

public interface IHomeGroup
{
    void SwitchHomeGroups();
    void SetHomeGroupIndex(UIBranch uIBranch);
    void ClearHomeScreen(UIBranch ignoreBranch);
    void RestoreHomeScreen();
}
