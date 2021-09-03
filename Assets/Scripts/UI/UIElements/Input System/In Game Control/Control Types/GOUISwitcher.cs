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
        int GOUIPlayerCount { get; }
    }
    
    public class GOUISwitcher : IGOUISwitcher, IEZEventUser, IEZEventDispatcher, ISwitchGroupPressed, IServiceUser
    {
        //Properties & Getters / Setters
        private bool CanSwitch => _myDataHub.SceneStarted && _myDataHub.OnHomeScreen && _playerObjects.Count > 0;
        public int GOUIPlayerCount => _playerObjects.Count;

        //Variables
        private int _index = 0;
        private List<IGOUIModule> _playerObjects;
        private IHistoryTrack _historyTrack;
        private IDataHub _myDataHub;
        
        //Events
        private Action<ISwitchGroupPressed> OnSwitchGroupPressed { get; set; }

        //Main
        public void OnEnable()
        {
            UseEZServiceLocator();
            FetchEvents();
            ObserveEvents();
        }
        
        public void UseEZServiceLocator()
        {
            _historyTrack = EZService.Locator.Get<IHistoryTrack>(this);
            _myDataHub = EZService.Locator.Get<IDataHub>(this);
        }

        public void ObserveEvents()
        {
            GOUIEvents.Do.Subscribe<IStartGOUIBranch>(SetIndex);
            BranchEvent.Do.Subscribe<ICloseBranch>(RemovePlayerObject);
        }

        public void UnObserveEvents() { }

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

        private void SwitchHasBeenPressed() => OnSwitchGroupPressed?.Invoke(this);

        private void DoSwitchProcess(Func<int, int> switchAction)
        {
            _playerObjects[_index].SwitchExit();
            _index = switchAction(_playerObjects.Count);
            _playerObjects[_index].SwitchEnter();
        }

        private void SetIndex(IStartGOUIBranch args)
        {
            if(!_myDataHub.SceneStarted) return;
            
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
        
        private void RemovePlayerObject(ICloseBranch args)
        {
            if(args.ReturnGOUIModule.IsNull()) return;
            
            if (_playerObjects.Contains(args.ReturnGOUIModule))
            {
                _index = 0;
                _playerObjects.Remove(args.ReturnGOUIModule);
                MoveToNextObjectOrBranch(args.TargetBranch);
            }
        }

        private void MoveToNextObjectOrBranch(IBranch branchToClose)
        {
            if (_playerObjects.Count > 0)
            {
                if (_myDataHub.ActiveBranch == branchToClose || _myDataHub.ActiveBranch.MyParentBranch == branchToClose)
                {
                    _playerObjects[_index].SwitchEnter();
                }

                _historyTrack.GOUIBranchHasClosed(branchToClose);
            }
            else
            {
                _historyTrack.GOUIBranchHasClosed(branchToClose, true);
            }
        }
    }
}