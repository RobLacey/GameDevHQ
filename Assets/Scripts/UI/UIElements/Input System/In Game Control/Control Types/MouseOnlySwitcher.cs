using System;

namespace UIElements
{
    public interface IMouseOnlySwitcher
    {
        void UseMouseOnlySwitcher();
        void ClearSwitchActivatedGOUI();
    }
    
    public class MouseOnlySwitcher : IMouseOnlySwitcher
    {
        public MouseOnlySwitcher(IGOController uiGameObjectController)
        {
            _controller = uiGameObjectController.Controller;
            _scheme = _controller.GetScheme();
            _playerObjects = _controller.GetPlayerObjects();
        }

        //Variables
        private int _index = 0;
        private readonly GOUIController _controller;
        private readonly InputScheme _scheme;
        private readonly IGOUIModule[] _playerObjects;
        private bool _switcherActive;

        
        //Properties
     //   private bool MouseSwitchOnly => _controller.ControlType == VirtualControl.SwitcherMouseOnly;
        private bool MouseSwitchOnly => true;
        
        //Main

        public void UseMouseOnlySwitcher()
        {
            if (MouseSwitchOnly)
                DoMouseOnlySwitch();
        }

        public void ClearSwitchActivatedGOUI()
        {
            if (!MouseSwitchOnly || !_switcherActive) return;
            
            //Todo Review
            // if (_scheme.CanSwitchToMouseOrVC)
            // { 
                _playerObjects[_index].StartChild(false);
                _switcherActive = false;
           // }
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
            SwapObjectMouseOnly(switchAction);
        }

        private void SwapObjectMouseOnly(Func<int, int> swap)
        {
            _index = _controller.GetIndex();
            _playerObjects[_index].UnHighlightBranch();
            _playerObjects[_index].ExitInGameUi();
            
            _index = swap(_playerObjects.Length);
            
            _playerObjects[_index].StartChild(true);
            _playerObjects[_index].HighlightBranch();
            _playerObjects[_index].StartInGameUi();
        }
    }
}