using System;

namespace UIElements
{
    public interface ISwitcher
    {
        void OnEnable();
        void UseSwitcher();
    }
    
    public class Switcher: ISwitcher, IEventUser
    {
        public Switcher(IGOController uiGameObjectController)
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
        private bool _inGame;

        //Properties
        private bool InGameSwitch => true; // _controller.ControlType == VirtualControl.Switcher  || UseBoth;
        private bool UseBoth => true;      // _controller.ControlType == VirtualControl.Both;

        //Main
        public void OnEnable() => ObserveEvents();
        
        public void ObserveEvents() => EVent.Do.Subscribe<IInMenu>(InGame);

        private void InGame(IInMenu args)
        {
            _inGame = !args.InTheMenu;
            if (_playerObjects.Length == 0) return;
        
            if (_inGame && InGameSwitch)
            {
                if(UseBoth) return;
                //_playerObjects[_index].OverFocus();
            }
            else
            {
               // _playerObjects[_index].UnFocus();
            }
        }

        public void UseSwitcher()
        {
            if(!InGameSwitch || _playerObjects.Length == 0 || !_inGame) return;
        
            if (_scheme.PressedPositiveSwitch())
            {
                SwapPlayerControlObject(x => _index.PositiveIterate(x));
                return;
            }

            if (_scheme.PressedNegativeSwitch())
            {
                SwapPlayerControlObject(x => _index.NegativeIterate(x));
            }
        }

        private void SwapPlayerControlObject(Func<int, int> swap)
        {
            //_playerObjects[_index].UnFocus();
            _index = swap(_playerObjects.Length);
          //  _playerObjects[_index].OverFocus();
        }
    }
}