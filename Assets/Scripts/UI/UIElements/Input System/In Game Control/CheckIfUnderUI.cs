using UnityEngine;
using UnityEngine.EventSystems;

namespace UIElements
{
    public class CheckIfUnderUI
    {
        private bool _allowMouseOver;

        public bool UnderUI()
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                _allowMouseOver = true;
                return true;
            }
            
            _allowMouseOver = false;
            return false;
        }

        public bool MouseNotUnderUI()
        {
            bool WasUnderUI() => _allowMouseOver && !EventSystem.current.IsPointerOverGameObject();

            if (!WasUnderUI()) return false;
            
            _allowMouseOver = false;
            return true;
        }
    }
}