using System;
using UIElements;
using UnityEngine.EventSystems;

public interface INodeBase : IMono
{
    void DeactivateNodeByType();
    void UnHighlightAlwaysOn();
    UINavigation Navigation { set; }
    void SetNodeAsActive();
    void OnEnter();
    void OnExit();
    void ThisNodeIsHighLighted();
    void ThisNodeNotHighLighted();
    void SelectedAction();
    void ClearNodeCompletely();
    void DoMoveToNextNode(MoveDirection moveDirection);
    void DoNonMouseMove(MoveDirection moveDirection);
    void EnableNodeAfterBeingDisabled();
    void DisableNode();
    void HotKeyPressed(bool setAsActive);
    void SetUpGOUIParent(IGOUIModule module);
}