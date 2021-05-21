using EZ.Service;
using UnityEngine;

namespace UIElements
{
    public abstract class RaycastBase : I2DRaycast, I3DRaycast, IServiceUser
    {
        //Variables
        private ICursorHandler _lastGameObject;
        protected LayerMask _layerToHit;
        protected readonly Camera _mainCamera = Camera.main;
        protected InputScheme _inputScheme;
        
        //Properties
        protected Vector3 CameraPosition => _mainCamera.transform.position;

        //Main
        public void OnEnable() => UseEZServiceLocator();

        public void UseEZServiceLocator() => _inputScheme = EZService.Locator.Get<InputScheme>(this);


        public virtual void OnStart() => _layerToHit = _inputScheme.ReturnVirtualCursorSettings.LayerToHit;

        public void WhenInMenu()
        {
            if(_lastGameObject.IsNull()) return;
            _lastGameObject = null;
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