using System;
using UnityEngine.EventSystems;

public interface INodeBase
{
    void Start();
    void OnEnable();
    void OnDisable();
    void DeactivateNodeByType();
    UINavigation Navigation { set; }

    void SetNodeAsActive();
    void OnEnter();
    void OnExit();
    void ThisNodeIsHighLighted();
    void SelectedAction();
    void ClearNodeCompletely();
    void DoMoveToNextNode(MoveDirection moveDirection);
    void DoNonMouseMove(MoveDirection moveDirection);

    void EnableNode();
    void DisableNode();
    void HotKeyPressed();
}