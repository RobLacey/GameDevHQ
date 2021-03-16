using System;
using UnityEngine;

namespace UIElements
{
    
    public interface IGOUIInput
    {
        void EnterGO(bool active);
        void ExitGO(bool active);
        void ClickOnGO(bool active);
        void ClearGOUIExceptHighlighted();
        void ClearGOUI();
    }

    public class GOUIInput : IGOUIInput
    {
        private bool _pointerOver;
        private readonly InGameUiTurnOn _turnOnWhen;
        private readonly InGameUiTurnOff _turnoffWhen;
        private readonly IBranch _myBranch;
        private Action StartInGameUI;
        private Action ExitInGameUI;

        public GOUIInput(IGOUIModule parentModule)
        {
            StartInGameUI = parentModule.StartInGameUi;
            ExitInGameUI = parentModule.ExitInGameUi;
            _turnOnWhen = parentModule.TurnOnWhen;
            _turnoffWhen = parentModule.TurnOffWhen;
            _myBranch = parentModule.TargetBranch;
        }
        
        public void EnterGO(bool active)
        {
            //_pointerOver = true;
            
            // if(active)
            //     _myBranch.DefaultStartOnThisNode.SetNodeAsActive();
            
            // if(active || _turnOnWhen == InGameUiTurnOn.OnClick) return;
            // StartInGameUI.Invoke();
            
           // ActivateNode();
        }

        private void ActivateNode()
        {
            if(!_pointerOver) return;
            _myBranch.DefaultStartOnThisNode.SetNodeAsActive();
        }
        
        private void DeactivateNode() => _myBranch.DefaultStartOnThisNode.DeactivateNode();

        public void ExitGO(bool active)
        {
            //_pointerOver = false;

            // if (!active) return; 
            // if (DeactivateNodeForOnClick()) return;             
            // ExitInGameUI.Invoke();
           // DeactivateNode();
        }
        
        private bool DeactivateNodeForOnClick()
        {
            if (_turnoffWhen == InGameUiTurnOff.OnClick 
                || _turnoffWhen == InGameUiTurnOff.ScriptCall)
            {
                DeactivateNode();
                return true;
            }
            return false;
        }

        public void ClickOnGO(bool active)
        {
            // if(!active)
            // {
            //     if (_turnOnWhen == InGameUiTurnOn.OnEnter) return;
            //     StartInGameUI.Invoke();
            //     ActivateNode();
            // }
            // else
            // {
            //     if(_turnoffWhen == InGameUiTurnOff.OnExit 
            //        || _turnoffWhen == InGameUiTurnOff.ScriptCall) return;
            //     ExitInGameUI.Invoke();
            //     DeactivateNode();
            // }

        }

        public void ClearGOUIExceptHighlighted()
        {
            // if(_pointerOver) return;
            // ClearGOUI();
        }

        public void ClearGOUI()
        {
            // _pointerOver = false;
            // DeactivateNode();
        }
    }
}