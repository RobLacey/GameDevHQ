
using System;
using UnityEngine;

public interface INode : IToggles, IParameters
{
    EscapeKey EscapeKeyType { get; }
    bool CloseHooverOnExit { get; }
    bool IsSelected { get; }
    IBranch MyBranch { get; }
    IBranch HasChildBranch { get; }
    bool DontStoreTheseNodeTypesInHistory { get; }
    GameObject ReturnGameObject { get; }
    void SetNodeAsActive();
    void DeactivateNode();
    void ThisNodeIsSelected();
    void ThisNodeIsHighLighted();
    void SetAsHighlighted();
    void SetNotHighlighted();
    void SetSelectedStatus(bool isSelected, Action endAction);
    void DoPress();
    void SetNodeAsSelected_NoEffects();
    void SetNodeAsNotSelected_NoEffects();
    
    IUiEvents UINodeEvents { get; }
}

public interface IToggles
{
    ToggleData ToggleData { get; }
    bool IsToggleGroup { get; }
    IsActive ReturnStartAsSelected { get; }
    IsActive SetStartAsSelected { set; }

}
        
