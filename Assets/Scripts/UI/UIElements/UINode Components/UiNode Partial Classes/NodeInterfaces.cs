
using System;
using System.Collections.Generic;
using UIElements.Input_System;
using UnityEngine;

public interface INode : IToggles, IParameters
{
    EscapeKey EscapeKeyType { get; }
    void SetAsHotKeyParent(bool setAsActive);
    IBranch MyBranch { get; }
    IBranch HasChildBranch { get; set; }
    bool CanNotStoreNodeInHistory { get; }
    GameObject ReturnGameObject { get; }
    GameObject InGameObject { get; set; }
    void SetNodeAsActive();
    void DeactivateNode();
    void ThisNodeIsHighLighted();
    IUiEvents UINodeEvents { get; }
    MultiSelectSettings MultiSelectSettings { get; }
    void ClearNode();
    float AutoOpenDelay { get; }
    bool CanAutoOpen { get; }
    IRunTimeSetter MyRunTimeSetter { get; }
}

public interface IToggles
{
    ToggleData ToggleData { get; }
    bool IsToggleGroup { get; }
}
        
