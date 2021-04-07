﻿using UnityEngine;

namespace UIElements
{
    public abstract class RaycastBase : I2DRaycast, I3DRaycast, IClearAll
    {
        protected RaycastBase(IRaycastController parent)
        {
            _controller = parent;
            _layerToHit = parent.LayerToHit;
        }

        //Variables
        private ICursorHandler _lastGameObject;
        private readonly IRaycastController _controller;
        protected LayerMask _layerToHit;
        protected readonly Camera _mainCamera = Camera.main;
        
        //Properties
        protected Vector3 CameraPosition => _mainCamera.transform.position;

        //Main
        public void WhenInMenu()
        {
            if(_lastGameObject.IsNull()) return;
            _lastGameObject = null;
        }

        public bool DoSelectedInGameObj()
        {
            if (!_controller.SelectPressed || _lastGameObject.IsNull()) return false;

            _lastGameObject.VirtualCursorDown();
            return true;
        }

        public void DoRaycast(Vector3 virtualCursorPos) => OverGameObj(RaycastToObj(virtualCursorPos));

        protected abstract ICursorHandler RaycastToObj(Vector3 virtualCursorPos);

        private void OverGameObj(ICursorHandler hit)
        {
            if (IfNoGOUIHit(hit)) return;

            if (_lastGameObject.IsNotNull())
            {
                if (_lastGameObject == hit) return;
                _lastGameObject.VirtualCursorExit();
            }
            
            if(hit.AlwaysOn == IsActive.Yes) return;

            _lastGameObject = hit;
            hit.VirtualCursorEnter();
        }

        private bool IfNoGOUIHit(ICursorHandler hit)
        {
            if (!hit.IsNull()) return false;
            ExitLastObject();
            return true;
        }

        private void ExitLastObject()
        {
            if (_lastGameObject.IsNull()) return;
            
            _lastGameObject.VirtualCursorExit();
            _lastGameObject = null;
        }
    }
}