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
    //void AllowKeyInvoker(bool canAllow);
}

public interface IUIHistory
{
    UINode LastSelected { get; }
    UINode LastHighlighted { get; }
    UIBranch ActiveBranch { get; set; }
    bool InMenu { get;}
    void GameToMenuSwitching(); 
    bool CanStart { get;}
    //Methods
    void SetLastSelected(UINode NewNode);
    void SetLastHighlighted(UINode newNode);
    bool IsUsingMouse();
}

public interface IPopUpData
{
    UINode LastNodeBeforePopUp { get; }
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
    PauseOptionsOnEscape PauseOptions { get; }
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

public interface IChangeControl
{
    bool UsingMouse { get; }
    bool UsingKeysOrCtrl { get; set; }
    IAllowKeys[] AllowKeyClasses { get; set; }
    void StartGame();
    void ChangeControlType();
    void SetAllowKeys();
    void ActivateKeysOrControl();
}

public interface IAllowKeys
{
    bool AllowKeys { set; }
}

