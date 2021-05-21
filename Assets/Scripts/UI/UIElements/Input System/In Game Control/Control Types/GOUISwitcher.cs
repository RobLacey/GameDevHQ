using System;
using System.Collections.Generic;
using System.Linq;
using EZ.Events;
using EZ.Service;
using Object = UnityEngine.Object;

namespace UIElements
{
    public interface IGOUISwitcher : IMonoEnable, IMonoStart
    {
        void UseGOUISwitcher(SwitchType switchType);
    }
    
    public class GOUISwitcher : IGOUISwitcher, IEZEventUser, IEZEventDispatcher, ISwitchGroupPressed, IServiceUser
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
        private IHistoryTrack _historyTrack;
        
        //Events
        private Action<ISwitchGroupPressed> OnSwitchGroupPressed { get; set; }

        //Main
        public void OnEnable()
        {
            UseEZServiceLocator();
            FetchEvents();
            ObserveEvents();
        }
        
        public void UseEZServiceLocator() => _historyTrack = EZService.Locator.Get<IHistoryTrack>(this);

        public void ObserveEvents()
        {
            GOUIEvents.Do.Subscribe<IStartGOUIBranch>(SetIndex);
            HistoryEvents.Do.Subscribe<IOnStart>(CanStart);
            HistoryEvents.Do.Subscribe<IOnHomeScreen>(SaveOnHomeScreen);
            GOUIEvents.Do.Subscribe<ICloseGOUIBranch>(RemovePlayerObject);
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

        public void FetchEvents() => OnSwitchGroupPressed = InputEvents.Do.Fetch<ISwitchGroupPressed>();

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
        
        private void RemovePlayerObject(ICloseGOUIBranch args)
        {
            if (_playerObjects.Contains(args.ReturnGOUIModule))
            {
                if (_playerObjects[_index].IsEqualTo(args.ReturnGOUIModule) && _playerObjects.Count > 1)
                {
                    _index = 0;
                }
                
                _playerObjects.Remove(args.ReturnGOUIModule);
                MoveToNextObjectOrBranch(args.TargetBranch);
            }
        }

        private void MoveToNextObjectOrBranch(IBranch branchToClose)
        {
            if (_playerObjects.Count > 0)
            {
                _historyTrack.GOUIBranchHasClosed(branchToClose, _playerObjects[_index]);
            }
            else
            {
                _historyTrack.GOUIBranchHasClosed(branchToClose);
            }
        }
    }
}