
using System;
using UnityEngine.EventSystems;

public class UiActions
{
    public readonly int _instanceId;
        public UiActions(int instanceId)
        {
            _instanceId = instanceId;
        }
        public Action<bool> _whenPointerOver;
        public Action<bool> _isSelected;
        public Action _isPressed;
        public Action<bool> _isDisabled;
        public Action<MoveDirection> _onMove;
}
