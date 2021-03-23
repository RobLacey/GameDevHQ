
using System;
using System.Collections.Generic;
using UnityEngine;

public interface INode : IToggles, IParameters
{
    EscapeKey EscapeKeyType { get; }
    void SetAsHotKeyParent();
    IBranch MyBranch { get; }
    IBranch HasChildBranch { get; set; }
    bool CanNotStoreNodeInHistory { get; }
    GameObject ReturnGameObject { get; }
    void SetNodeAsActive();
    void DeactivateNode();
    void ThisNodeIsHighLighted();
    IUiEvents UINodeEvents { get; }
    void ClearNode();
    IsActive AutoOpenCloseOverride { get; }
    float AutoOpenDelay { get; }
}

public interface IToggles
{
    ToggleData ToggleData { get; }
    bool IsToggleGroup { get; }
}
        
