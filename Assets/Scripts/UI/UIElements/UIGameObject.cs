using NaughtyAttributes;
using UnityEngine;

public interface ICursorHandler
{
    void CursorEnter();
    void CursorExit();
    void CursorDown();
}

namespace UIElements
{
    public class UIGameObject : MonoBehaviour, IEventUser, ICursorHandler
    {
        [SerializeField] private UIBranch _branch;
        [SerializeField] private InGameUiTurnOn _turnOnWhen;
        [SerializeField] private InGameUiTurnOff _turnOffWhen;
        [SerializeField] [InfoBox("If left blank the centre of object will be used")] 
        private Transform _uiOffsetPosition;
        
        //Variables
        private bool _active;
        private static bool pointerOver;
        private MovementTest _myMovement;
        private bool _allowKeys;

        //Properties & Set / Getters
        public bool UiTargetNotActive => !_active;
        public void SetAsNotActive() => _active = false;
        public void SetAllowKeys(IAllowKeys args) => _allowKeys = args.CanAllowKeys;
        public Transform UsersTransform => _uiOffsetPosition;
        
        //Main
        private void Awake()
        {
            pointerOver = false;
            if (_uiOffsetPosition == null)
            {
                _uiOffsetPosition = transform;
            }

            _myMovement = GetComponent<MovementTest>();
            
            ObserveEvents();
        }
        
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
            _myMovement.ActivateObject(this);
            EnterUi();
            _branch.DefaultStartOnThisNode.SetNodeAsActive();
        }

        public void SelectFocus()
        {
            if (!_active)
            {
                if (_turnOnWhen == InGameUiTurnOn.OnEnter) return;
                EnterUi();
                _branch.DefaultStartOnThisNode.SetNodeAsActive();
                _myMovement.ActivateObject(this);
            }
            else
            {
                if(_turnOffWhen == InGameUiTurnOff.OnExit || _turnOffWhen == InGameUiTurnOff.ScriptCall) return;
                ExitUi();
                _branch.DefaultStartOnThisNode.DeactivateNode();
                _myMovement.ActivateObject(null);
            }
        }

        public void UnFocus()
        {
            _myMovement.ActivateObject(null);
            pointerOver = false;
            if (!_active || _turnOffWhen == InGameUiTurnOff.OnClick
                         || _turnOffWhen == InGameUiTurnOff.ScriptCall) return;
            ExitUi();
            _branch.DefaultStartOnThisNode.DeactivateNode();
        }

        private void OnMouseEnter()
        {
            if(_allowKeys) return;
            pointerOver = true;
            if(_active || _turnOnWhen == InGameUiTurnOn.OnClick) return;
            EnterUi();
        }

        private void OnMouseExit()
        {
            if(_allowKeys) return;
            pointerOver = false;
            if (!_active || _turnOffWhen == InGameUiTurnOff.OnClick
                            || _turnOffWhen == InGameUiTurnOff.ScriptCall) return;
            ExitUi();
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

            _branch.DefaultStartOnThisNode.SetNodeAsActive();
        }

        public void CursorExit()
        {
            pointerOver = false;
            if (!_active || _turnOffWhen == InGameUiTurnOff.OnClick
                         || _turnOffWhen == InGameUiTurnOff.ScriptCall) return;
            ExitUi();

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
    }
}