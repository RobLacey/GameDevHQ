﻿
using System;
using UnityEngine.EventSystems;

public class UiActions
{
        public Action<bool> _whenPointerOver;
        public Action<bool> _isSelected;
        public Action _isPressed;
        public Action<bool> _isDisabled;
        public Action<MoveDirection> _onMove;
}