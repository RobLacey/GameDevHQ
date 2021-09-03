
using System;
using System.Collections.Generic;
using EZ.Inject;
using UIElements.Input_System;
using UnityEngine;
using UnityEngine.EventSystems;

public interface INode : IToggles, IParameters
{
    EscapeKey EscapeKeyType { get; }
    void SetAsHotKeyParent(bool setAsActive);
    IBranch MyBranch { get; }
    IBranch HasChildBranch { get; set; }
    bool CanNotStoreNodeInHistory { get; }
    GameObject ReturnGameObject { get; }
    GameObject InGameObject { get; set; }
    void UnHighlightAlwaysOn();
    void SetNodeAsActive();
    void DeactivateNode();
    void ThisNodeIsHighLighted();
    void ThisNodeNotHighLighted();
    IUiEvents UINodeEvents { get; }
    MultiSelectSettings MultiSelectSettings { get; }
    void ClearNode();
    float AutoOpenDelay { get; }
    bool CanAutoOpen { get; }
    IRunTimeSetter MyRunTimeSetter { get; }
    void DoNonMouseMove(MoveDirection moveDirection);
    void SetGOUIModule(IGOUIModule module);
}

public interface IToggles
{
    ToggleData ToggleData { get; }
    bool IsToggleGroup { get; }
}
        
