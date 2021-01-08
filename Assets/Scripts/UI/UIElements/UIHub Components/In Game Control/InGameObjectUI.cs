using System;
using NaughtyAttributes;
using UIElements;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public interface ICursorHandler
{
    void CursorEnter();
    void CursorExit();
    void CursorDown();
    void NotInGame();
}

public interface IActiveInGameObject
{
    InGameObjectUI ActiveObject { get; }
}

namespace UIElements
{
    public class InGameObjectUI : MonoBehaviour, IEventUser, ICursorHandler, IActiveInGameObject, IEventDispatcher
    {
        [SerializeField] private UIBranch _branch;
        [SerializeField] private InGameUiTurnOn _turnOnWhen;
        [SerializeField] private InGameUiTurnOff _turnOffWhen;
        [SerializeField] [Space(10f, order = 1)] [InfoBox(InfoBox, order = 2)] 
        private Transform _uiPosition;
        [SerializeField] [Space(10f)] private UnityEvent<bool> _activateInGameObject;

        //Variables
        private bool _active, _justSwitchedToMouse;
        private bool _pointerOver;
        private bool _allowKeys;
        private UIGOController _controller;
        
        //Editor
        private const string InfoBox = "If left blank the centre of object will be used";

        //Properties & Set / Getters
        public UIBranch MyBranch => _branch;
        private void SetAllowKeys(IAllowKeys args) => _allowKeys = args.CanAllowKeys;
        public Transform UsersTransform => _uiPosition;
        public InGameObjectUI ActiveObject => this;
        public bool PointerOver => _pointerOver;

        public void CheckForSetLayerMask(LayerMask layerMask)
        {
            var temp = LayerMask.LayerToName(gameObject.layer);
            var thisLayerMask = LayerMask.GetMask(temp);
            if ((thisLayerMask & layerMask) == 0)
            {
                throw new Exception
                    ($"Set Layer on {gameObject.name} to _layerToHit found in UI GameObject Controller");
            }
        }

        //Events
        private Action<IActiveInGameObject> DoActivateInGameObject;
        
        //Main
        private void Awake()
        {
            Debug.Log("Trying to fix the cancel bug when switching from mouse to in game and" +
                      " also the weird not appearing tooltip bug. Might need new EVent as ClearAll causes issues if used" +
                      "to clear ingameUi");
            
            _controller = FindObjectOfType<UIGOController>();
            
            _pointerOver = false;
            if (_uiPosition == null)
            {
                _uiPosition = transform;
            }
            FetchEvents();
        }

        public void FetchEvents() => DoActivateInGameObject = EVent.Do.Fetch<IActiveInGameObject>();
        
        private void OnEnable() => ObserveEvents();

        public void ObserveEvents()
        {
            EVent.Do.Subscribe<IClearAll>(ClearUI);
            EVent.Do.Subscribe<IAllowKeys>(SetAllowKeys);
        }
        
        public void SetAsNotActive()
        {
            _justSwitchedToMouse = false;
            _active = false;
        }

        public void OverFocus()
        {
            if(_active || _turnOnWhen == InGameUiTurnOn.OnClick) return;
            StartInGameUi();
        }

        public void UnFocus() 
        {
            _justSwitchedToMouse = false;
            
            if (!_active /*|| _turnOffWhen == InGameUiTurnOff.OnClick
                         || _turnOffWhen == InGameUiTurnOff.ScriptCall*/) return;
            ExitInGameUi();
        }

        public void SwitchMouseOnly()
        {
            _justSwitchedToMouse = true;
            StartInGameUi();
        }

        private void OnMouseEnter()
        {
            if (_allowKeys) return;
            _pointerOver = true;
            OnlyJustSwitchedOnFromMouseOnlyControl();
            if(_active)
                _branch.DefaultStartOnThisNode.SetNodeAsActive();
            
            if(_active || _turnOnWhen == InGameUiTurnOn.OnClick) return;
            
            StartInGameUi();
        }
        
        public void CursorEnter()
        {
            _pointerOver = true;
            if(_active)
                _branch.DefaultStartOnThisNode.SetNodeAsActive();

            if(_active || _turnOnWhen == InGameUiTurnOn.OnClick) return;

            StartInGameUi();
        }

        private void OnlyJustSwitchedOnFromMouseOnlyControl()
        {
            if (_justSwitchedToMouse)
            {
                _branch.DoNotTween();
                _justSwitchedToMouse = false;
            }
        }

        private void OnMouseExit()
        {
            if(_allowKeys) return;
            _pointerOver = false;

            if (!_active) return; 
            if (DeactivateNodeForOnClick()) return;             
            ExitInGameUi();
        }

        private bool DeactivateNodeForOnClick()
        {
            if (_turnOffWhen == InGameUiTurnOff.OnClick || _turnOffWhen == InGameUiTurnOff.ScriptCall)
            {
                _branch.DefaultStartOnThisNode.DeactivateNode();
                return true;
            }
            return false;
        }

        private void OnMouseDown()
        {
            if(_allowKeys) return;
            
            if(!_active)
            {
                if (_turnOnWhen == InGameUiTurnOn.OnEnter) return;
                StartInGameUi();
            }
            else
            {
                if(_turnOffWhen == InGameUiTurnOff.OnExit || _turnOffWhen == InGameUiTurnOff.ScriptCall) return;
                ExitInGameUi();
            }
        }

        public void NotInGame() => _pointerOver = false;

        public void CancelUi()
        {
            _justSwitchedToMouse = false;
            ExitInGameUi();
        }

        private void ClearUI(IClearAll args = null)
        {
            if(!_active || _pointerOver) return;
            _justSwitchedToMouse = false;
            ExitInGameUi();
        }

        private void StartInGameUi()
        {
            _controller.SetIndex(this);

            _branch.StartInGameUi(this);
            _active = true;
            Activate();
            if(_justSwitchedToMouse) return;
            _branch.DefaultStartOnThisNode.SetNodeAsActive();
        }

        private void ExitInGameUi()
        {
            _branch.ExitInGameUi();
            _active = false;
            Deactivate();
            _branch.DefaultStartOnThisNode.DeactivateNode();
        }


        public void CursorExit()
        {
            _pointerOver = false;
            if (!_active) return; 
            if (DeactivateNodeForOnClick()) return;

            ExitInGameUi();
        }

        public void CursorDown()
        {
            if(!_active)
            {
                if (_turnOnWhen == InGameUiTurnOn.OnEnter) return;
                StartInGameUi();
            }
            else
            {
                if(_turnOffWhen == InGameUiTurnOff.OnExit || _turnOffWhen == InGameUiTurnOff.ScriptCall) return;
                ExitInGameUi();
            }
        }
        
        private void Activate()
        {
            DoActivateInGameObject?.Invoke(this);
            _activateInGameObject.Invoke(true);
        }
        
        private void Deactivate()
        {
            DoActivateInGameObject?.Invoke(null);
            _activateInGameObject.Invoke(false);
        }

        // public void SetToUseSwitcher()
        // {
        //     _turnOnWhen = InGameUiTurnOn.OnEnter;
        //     _turnOffWhen = InGameUiTurnOff.OnExit;
        // }
    }
}