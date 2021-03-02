using System;
using UnityEngine;

namespace UIElements
{
    public interface IMouseOnlySwitcher
    {
        void UseMouseOnlySwitcher();
    }
    
    public class MouseOnlySwitcher : IEventDispatcher, IClearAll, IMouseOnlySwitcher
    {
        public MouseOnlySwitcher(IGOController uiGameObjectController)
        {
            FetchEvents();
            _controller = uiGameObjectController.Controller;
            _scheme = _controller.GetScheme();
            _playerObjects = _controller.GetPlayerObjects();
        }

        //Variables
        private int _index = 0;
        private readonly UIGOController _controller;
        private readonly InputScheme _scheme;
        private readonly InGameObjectUI[] _playerObjects;
        
        //Events
        private Action<IClearAll> ClearAll { get; set; }
        
        //Properties
        private bool MouseSwitchOnly => _controller.ControlType == VirtualControl.SwitcherMouseOnly;
        
        //Main
        public void FetchEvents() => ClearAll = EVent.Do.Fetch<IClearAll>();

        public void UseMouseOnlySwitcher()
        {
            if (MouseSwitchOnly)
                DoMouseOnlySwitch();
        }

        private void DoMouseOnlySwitch()
        {
            if (_scheme.PressedPositiveSwitch())
            {
                ClearAll?.Invoke(this);
                SwapObjectMouseOnly(x => _index.PositiveIterate(x));
                return;
            }

            if (_scheme.PressedNegativeSwitch())
            {
                ClearAll?.Invoke(this);
                SwapObjectMouseOnly(x => _index.NegativeIterate(x));
            }
        }
    
        private void SwapObjectMouseOnly(Func<int, int> swap)
        {
            _index = _controller.GetIndex();
            _playerObjects[_index].UnFocus();
            _index = swap(_playerObjects.Length);
            _playerObjects[_index].SwitchMouseOnly();
        }
    }
}