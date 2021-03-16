using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UIElements
{
    public interface IMouseOnlySwitcher
    {
        void UseMouseOnlySwitcher();
        void ClearSwitchActivatedGOUI();
    }
    
    public class MouseOnlySwitcher : IMouseOnlySwitcher, IEventDispatcher, IClearAll
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
        private readonly GOUIController _controller;
        private readonly InputScheme _scheme;
        private readonly GOUIModule[] _playerObjects;
        private bool _switcherActive;

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

        public void ClearSwitchActivatedGOUI()
        {
            if (!MouseSwitchOnly || !_switcherActive) return;
            
            if (_scheme.CanSwitchToMouse 
                && !_playerObjects[_index].TargetBranch.DefaultStartOnThisNode.HasChildBranch.CanvasIsEnabled)
            {
                Debug.Log(_playerObjects[_index]);
                _playerObjects[_index].SwitchActive(false);
                _switcherActive = false;
                ClearAll?.Invoke(this);
            }
        }

        private void DoMouseOnlySwitch()
        {
            if (_scheme.PressedPositiveGOUISwitch())
            {
                DoSwitchProcess(x => _index.PositiveIterate(x));
                return;
            }

            if (_scheme.PressedNegativeGOUISwitch())
            {
                DoSwitchProcess(x => _index.NegativeIterate(x));
            }
        }

        private void DoSwitchProcess(Func<int, int> switchAction)
        {
            _switcherActive = true;
            ClearAll?.Invoke(this);
            SwapObjectMouseOnly(switchAction);
        }

        private void SwapObjectMouseOnly(Func<int, int> swap)
        {
            _index = _controller.GetIndex();
            _playerObjects[_index].ExitInGameUi();
            _index = swap(_playerObjects.Length);
            _playerObjects[_index].SwitchActive(true);
            _playerObjects[_index].StartInGameUi();
        }

    }
}