using System;
using UnityEngine;

namespace UIElements
{
    public abstract class RaycastBase : I2DRaycast, I3DRaycast, IClearAll, IEventDispatcher
    {
        protected RaycastBase(IRaycastController parent)
        {
            _controller = parent;
            _layerToHit = parent.LayerToHit;
            FetchEvents();
        }

        //Variables
        private ICursorHandler _lastGameObject;
        private readonly IRaycastController _controller;
        protected LayerMask _layerToHit;
        protected readonly Camera _mainCamera = Camera.main;
        
        //Events
        private Action<IClearAll> ClearAll { get; set; }

        //Properties
        protected Vector3 CameraPosition => _mainCamera.transform.position;

        //Main
        public void FetchEvents() => ClearAll = EVent.Do.Fetch<IClearAll>();

        public void WhenInMenu()
        {
            if(_lastGameObject is null) return;
            _lastGameObject.NotInGame();
            _lastGameObject = null;
        }

        public void DoSelectedInGameObj()
        {
            if (!_controller.SelectPressed) return;
            
            if(_lastGameObject.IsNotNull())
            {
                _lastGameObject.CursorDown();
            }
            else
            {
                ClearAll?.Invoke(this);
            }
        }

        public void DoRaycast(Vector3 virtualCursorPos) => OverGameObj(RaycastToObj(virtualCursorPos));

        private void OverGameObj(ICursorHandler hit)
        {
            if (hit.IsNull())
            {
                ExitLastObject();
                return;
            }
            
            if (_lastGameObject.IsNotNull())
            {
                if (_lastGameObject == hit) return;
                _lastGameObject.CursorExit();
            }
            
            _lastGameObject = hit;
            hit.CursorEnter();
        }

        private void ExitLastObject()
        {
            if (_lastGameObject.IsNull()) return;
            
            _lastGameObject.CursorExit();
            _lastGameObject = null;
        }

        protected abstract ICursorHandler RaycastToObj(Vector3 virtualCursorPos);
    }
}