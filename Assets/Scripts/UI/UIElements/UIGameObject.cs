using NaughtyAttributes;
using UnityEngine;

namespace UIElements
{
    public class UIGameObject : MonoBehaviour, IEventUser
    {
        [SerializeField] private UIBranch _branch;
        [SerializeField] private InGameUiTurnOn _turnOnWhen;
        [SerializeField] private InGameUiTurnOff _turnOffWhen;
        [SerializeField] [InfoBox("If left blank the centre of object will be used")] 
        private Transform _uiOffsetPosition;
        
        //Variables
        private bool _active;
        private static bool pointerOver;

        //Properties & Set / Getters
        public bool UiTargetNotActive => !_active;
        public void SetAsNotActive() => _active = false;
        public Transform UsersTransform => _uiOffsetPosition;
        
        //Main
        private void Awake()
        {
            pointerOver = false;
            if (_uiOffsetPosition == null)
            {
                _uiOffsetPosition = transform;
            }
            ObserveEvents();
        }
        
        public void ObserveEvents() => EVent.Do.Subscribe<IClearAll>(ClearUI);
        
        /// <summary>
        /// Three publics methods to allow access for cursor control for keyboard or mouse. Not Tested yet
        /// </summary>
        //TODO Activae these controls if needs be
        
        public void OnEnter() => OnMouseEnter();

        public void OnExit() => OnMouseExit();

        public void OnSelect() => OnMouseDown();

        private void OnMouseEnter()
        {
            pointerOver = true;
            if(_active || _turnOnWhen == InGameUiTurnOn.OnClick) return;
            EnterUi();
        }

        private void OnMouseExit()
        {
            pointerOver = false;
            if (!_active || _turnOffWhen == InGameUiTurnOff.OnClick
                            || _turnOffWhen == InGameUiTurnOff.ScriptCall) return;
            ExitUi();
        }

        private void OnMouseDown()
        {
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
            Debug.Log(this);
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

    }
}