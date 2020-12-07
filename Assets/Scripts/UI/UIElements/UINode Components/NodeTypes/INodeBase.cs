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
   // void SetNotHighlighted();
   // void SetAsHighlighted();
    void OnEnter(bool isDragEvent);
    void OnExit(bool isDragEvent);
    void ThisNodeIsHighLighted();
    void SelectedAction(bool isDragEvent);
    void ClearNodeCompletely();
    void DoMoveToNextNode(MoveDirection moveDirection);
    void DoNonMouseMove(MoveDirection moveDirection);

    void EnableNode();
    void DisableNode();
}