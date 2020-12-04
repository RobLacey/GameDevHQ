
using System;
using System.Collections.Generic;
using UnityEngine;

public interface INode : IToggles, IParameters
{
    EscapeKey EscapeKeyType { get; }
    bool CloseHooverOnExit { get; }
    IBranch MyBranch { get; }
    IBranch HasChildBranch { get; }
    bool CanStoreNodeInHistory { get; }
    GameObject ReturnGameObject { get; }
    void SetNodeAsActive();
    void DeactivateNode();
    void ThisNodeIsHighLighted();
    IUiEvents UINodeEvents { get; }
    void ClearNode();
}

public interface IToggles
{
    ToggleData ToggleData { get; }
    bool IsToggleGroup { get; }
    int HasAGroupStartPoint { get; }
    List<INode> ToggleGroupMembers { get; }
    IsActive ReturnStartAsSelected { get; }
    IsActive SetStartAsSelected { set; }

}
        
