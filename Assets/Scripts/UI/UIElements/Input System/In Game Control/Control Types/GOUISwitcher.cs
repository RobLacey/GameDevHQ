using System;
using UnityEngine;

namespace UIElements
{
    public interface IGOUISwitcher
    {
        void OnEnable();
        void UseGOUISwitcher(SwitchType switchType);
    }
    
    public class GOUISwitcher : IGOUISwitcher, IEventUser
    {
        public GOUISwitcher(IGOUISwitchSettings settings)
        {
            _playerObjects = settings.GetPlayerObjects;
        }
        
        //Properties & Getters / Setters
        private void SaveOnHomeScreen(IOnHomeScreen args) => _onHomeScreen = args.OnHomeScreen;
        private void CanStart(IOnStart obj) => _canStart = true;
        private bool CanSwitch => _canStart && _onHomeScreen;


        //Variables
        private bool _canStart;
        private bool _onHomeScreen = true;
        private int _index = 0;
        private readonly IGOUIModule[] _playerObjects;

        //Main
        public void OnEnable() => ObserveEvents();

        public void ObserveEvents()
        {
            EVent.Do.Subscribe<IStartGOUIBranch>(SetIndex);
            EVent.Do.Subscribe<IOnStart>(CanStart);
            EVent.Do.Subscribe<IOnHomeScreen>(SaveOnHomeScreen);
        }
        
        public void UseGOUISwitcher(SwitchType switchType)
        {
            if(!CanSwitch) return;
            
            switch (switchType)
            {
                case SwitchType.Positive:
                    DoSwitchProcess(x => _index.PositiveIterate(x));
                    break;
                case SwitchType.Negative:
                    DoSwitchProcess(x => _index.NegativeIterate(x));
                    break;
                case SwitchType.Activate:
                    _playerObjects[_index].SwitchEnter();
                    break;
            }
        }

        private void DoSwitchProcess(Func<int, int> switchAction)
        {
             _playerObjects[_index].SwitchExit();
            _index = switchAction(_playerObjects.Length);
            _playerObjects[_index].SwitchEnter();
        }

        private void SetIndex(IStartGOUIBranch args)
        {
            int index = 0;
            foreach (var inGameObjectUI in _playerObjects)
            {
                if (ReferenceEquals(inGameObjectUI, args.ReturnGOUIModule))
                { 
                    _index = index;
                    break;
                }
                index++;
            }
        }
    }
}