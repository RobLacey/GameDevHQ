
using System;
using UnityEngine.EventSystems;

public class UiActions
{
    public int InstanceId { get; }
    public UINode Node { get; }
        public UiActions(int instanceId, UINode node)
        {
            InstanceId = instanceId;
            Node = node;
        }
        public Action<bool> _whenPointerOver;
        public Action<bool> _isSelected;
        public Action _isPressed;
        public Action _canPlayCancelAudio;
        public Action<bool> _isDisabled;
        public Action<MoveDirection> _onMove;
}
