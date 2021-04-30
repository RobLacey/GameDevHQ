using System;
using System.Collections.Generic;
using System.Linq;
using Object = UnityEngine.Object;

namespace UIElements
{
    public interface IGOUISwitcher : IMonoEnable, IMonoStart
    {
        void UseGOUISwitcher(SwitchType switchType);
    }
    
    public class GOUISwitcher : IGOUISwitcher, IEventUser, IEventDispatcher, ISwitchGroupPressed
    {
        //Properties & Getters / Setters
        private void SaveOnHomeScreen(IOnHomeScreen args) => _onHomeScreen = args.OnHomeScreen;
        private void CanStart(IOnStart obj) => _canStart = true;
        private bool CanSwitch => _canStart && _onHomeScreen && _playerObjects.Count > 0;
        public SwitchType SwitchType { get; }

        //Variables
        private bool _canStart;
        private bool _onHomeScreen = true;
        private int _index = 0;
        private List<IGOUIModule> _playerObjects;
        
        //Events
        private Action<ISwitchGroupPressed> OnSwitchGroupPressed { get; set; }

        //Main
        public void OnEnable()
        {
            FetchEvents();
            ObserveEvents();
        }

        public void ObserveEvents()
        {
            EVent.Do.Subscribe<IStartGOUIBranch>(SetIndex);
            EVent.Do.Subscribe<IOnStart>(CanStart);
            EVent.Do.Subscribe<IOnHomeScreen>(SaveOnHomeScreen);
            EVent.Do.Subscribe<ICloseAndResetBranch>(RemovePlayerObject);
        }
        
        public void OnStart() => FindActiveGameObjectsOnStart();

        private void FindActiveGameObjectsOnStart()
        {
            _playerObjects = new List<IGOUIModule>();
            var _allObjects = new List<IGOUIModule>(Object.FindObjectsOfType<GOUIModule>().ToList());

            foreach (var playerObject in _allObjects.Where(playerObject => playerObject.GOUIModuleCanBeUsed))
            {
                _playerObjects.Add(playerObject);
            }
        }

        public void FetchEvents() => OnSwitchGroupPressed = EVent.Do.Fetch<ISwitchGroupPressed>();

        public void UseGOUISwitcher(SwitchType switchType)
        {
            SwitchHasBeenPressed();
            
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

        private void SwitchHasBeenPressed()
        {
            if (_playerObjects.Count > 1)
                OnSwitchGroupPressed?.Invoke(this);
        }

        private void DoSwitchProcess(Func<int, int> switchAction)
        {
            if (_playerObjects.Count == 1) return;
            
            _playerObjects[_index].SwitchExit();
            _index = switchAction(_playerObjects.Count);
            _playerObjects[_index].SwitchEnter();
        }

        private void SetIndex(IStartGOUIBranch args)
        {
            if(!_canStart) return;
            
            if (!_playerObjects.Contains(args.ReturnGOUIModule))
            {
                _playerObjects.Add(args.ReturnGOUIModule);
                return;
            }
            
            FindNewIndex(args.ReturnGOUIModule);
        }

        private void FindNewIndex(IGOUIModule currentGOUI)
        {
            int index = 0;
            foreach (var inGameObjectUI in _playerObjects)
            {
                if (ReferenceEquals(inGameObjectUI, currentGOUI))
                {
                    _index = index;
                    break;
                }

                index++;
            }
        }
        
        //TODO GOUI Switcher
        //No list safe guards
        //Object is in position 0 safe guards
        //Change Event in GOUITo Sent branch

        private void RemovePlayerObject(ICloseAndResetBranch args)
        {
            if (_playerObjects.Contains(args.ReturnGOUIModule))
            {
                if (_playerObjects[_index].IsEqualTo(args.ReturnGOUIModule) && _playerObjects.Count > 1)
                {
                    _index = 0;
                }
                _playerObjects.Remove(args.ReturnGOUIModule);
            }
        }
    }
}