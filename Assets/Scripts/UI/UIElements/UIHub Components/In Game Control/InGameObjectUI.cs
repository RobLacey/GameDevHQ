using System;
using NaughtyAttributes;
using UIElements;
using UnityEngine;
using UnityEngine.Events;

public interface ICursorHandler
{
    void CursorEnter();
    void CursorExit();
    void CursorDown();
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
        [SerializeField] [InfoBox("If left blank the centre of object will be used")] 
        private Transform _uiOffsetPosition;
        
        //Variables
        private bool _active, _justSwitchedFromMouse;
        private static bool pointerOver;
        private bool _allowKeys;

        //Properties & Set / Getters
        public bool UiTargetNotActive => !_active;
        public void SetAsNotActive() => _active = false;
        private void SetAllowKeys(IAllowKeys args) => _allowKeys = args.CanAllowKeys;
        public Transform UsersTransform => _uiOffsetPosition;
        public InGameObjectUI ActiveObject => this;

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
        public UnityEvent<bool> _activateInGameObject;
        private Action<IActiveInGameObject> DoActivateInGameObject;
        
        //Main
        private void Awake()
        {
            pointerOver = false;
            if (_uiOffsetPosition == null)
            {
                _uiOffsetPosition = transform;
            }
            FetchEvents();
        }

        private void OnEnable() => ObserveEvents();

        public void FetchEvents() => DoActivateInGameObject = EVent.Do.Fetch<IActiveInGameObject>();

        public void ObserveEvents()
        {
            EVent.Do.Subscribe<IClearAll>(ClearUI);
            EVent.Do.Subscribe<IAllowKeys>(SetAllowKeys);
        }
       
        /// <summary>
        /// Three public methods to allow access for cursor control for keyboard or mouse. Not Tested yet
        /// </summary>
        //TODO Activate these controls if needs be
        
        public void OnEnter() => OnMouseEnter();

        public void OnExit() => OnMouseExit();

        public void OnSelect() => OnMouseDown();

        public void OverFocus()
        {
            pointerOver = true;
            if(_active || _turnOnWhen == InGameUiTurnOn.OnClick) return;
            Activate();
            EnterUi();
            _branch.DefaultStartOnThisNode.SetNodeAsActive();
        }

        public void SwitchMouseOnly()
        {
            Activate();
            EnterUi();
            _active = false;
            _justSwitchedFromMouse = true;
        }

        
        //TODO Not Used??
        public void SelectFocus()
        {
            if (!_active)
            {
                if (_turnOnWhen == InGameUiTurnOn.OnEnter) return;
                EnterUi();
                _branch.DefaultStartOnThisNode.SetNodeAsActive();
                Activate();
            }
            else
            {
                if(_turnOffWhen == InGameUiTurnOff.OnExit || _turnOffWhen == InGameUiTurnOff.ScriptCall) return;
                ExitUi();
                _branch.DefaultStartOnThisNode.DeactivateNode();
                Deactivate();
            }
        }

        public void UnFocus() 
        {
            Deactivate();
            pointerOver = false;

            if (!_active || _turnOffWhen == InGameUiTurnOff.OnClick
                         || _turnOffWhen == InGameUiTurnOff.ScriptCall) return;
            ExitUi();
            _branch.DefaultStartOnThisNode.DeactivateNode();
        }

        private void OnMouseEnter()
        {
            if (AllowUseOfMouseOver()) return;
            pointerOver = true;
            if(_active || _turnOnWhen == InGameUiTurnOn.OnClick) return;
            OnlyJustSwitchedOnFromMouseOnlyControl();
            EnterUi();
            _branch.DefaultStartOnThisNode.SetNodeAsActive();

        }

        private bool AllowUseOfMouseOver()
        {
            if (_allowKeys) return true;
            return false;
        }

        private void OnlyJustSwitchedOnFromMouseOnlyControl()
        {
            if (_justSwitchedFromMouse)
            {
                _branch.DoNotTween();
                _justSwitchedFromMouse = false;
            }
        }

        private void OnMouseExit()
        {
            if(AllowUseOfMouseOver()) return;
            pointerOver = false;
            if (!_active || _turnOffWhen == InGameUiTurnOff.OnClick
                            || _turnOffWhen == InGameUiTurnOff.ScriptCall) return;
            ExitUi();
            _branch.DefaultStartOnThisNode.DeactivateNode();

        }

        private void OnMouseDown()
        {
            if(_allowKeys) return;
            
            if(!_active)
            {
                if (_turnOnWhen == InGameUiTurnOn.OnEnter) return;
                EnterUi();
            }
            else
            {
                if(_turnOffWhen == InGameUiTurnOff.OnExit || _turnOffWhen == InGameUiTurnOff.ScriptCall) return;
                ExitUi();
            }
        }

        private void ClearUI(IClearAll args)
        {
            if(!_active || pointerOver) return;
            ExitUi();
        }

        private void EnterUi()
        {
            _branch.StartInGameUi(this);
            _active = true;
        }

        private void ExitUi()
        {
            _branch.ExitInGameUi();
            _active = false;
        }

        public void CursorEnter()
        {
            pointerOver = true;

            if(_active || _turnOnWhen == InGameUiTurnOn.OnClick) return;

            EnterUi();
            Activate();
            _branch.DefaultStartOnThisNode.SetNodeAsActive();
        }

        public void CursorExit()
        {
            pointerOver = false;

            if (!_active || _turnOffWhen == InGameUiTurnOff.OnClick
                         || _turnOffWhen == InGameUiTurnOff.ScriptCall) return;
            
            ExitUi();
            Deactivate();
            _branch.DefaultStartOnThisNode.DeactivateNode();
        }

        public void CursorDown()
        {
            if(!_active)
            {
                if (_turnOnWhen == InGameUiTurnOn.OnEnter) return;
                EnterUi();
                _branch.DefaultStartOnThisNode.SetNodeAsActive();

            }
            else
            {
                if(_turnOffWhen == InGameUiTurnOff.OnExit || _turnOffWhen == InGameUiTurnOff.ScriptCall) return;
                ExitUi();
                _branch.DefaultStartOnThisNode.DeactivateNode();
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

        public void SetToUseSwitcher()
        {
            _turnOnWhen = InGameUiTurnOn.OnEnter;
            _turnOffWhen = InGameUiTurnOff.OnExit;
        }
    }
}