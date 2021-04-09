﻿
using System;
using System.Collections.Generic;
using UnityEngine;

public interface INode : IToggles, IParameters
{
    EscapeKey EscapeKeyType { get; }
    void SetAsHotKeyParent(bool setAsActive);
    IBranch MyBranch { get; }
    IBranch HasChildBranch { get; set; }
    bool CanNotStoreNodeInHistory { get; }
    GameObject ReturnGameObject { get; }
    void SetNodeAsActive();
    void DeactivateNode();
    void ThisNodeIsHighLighted();
    IUiEvents UINodeEvents { get; }
    void ClearNode();
    float AutoOpenDelay { get; }
    bool CanAutoOpen { get; }
}

public interface IToggles
{
    ToggleData ToggleData { get; }
    bool IsToggleGroup { get; }
}
        
