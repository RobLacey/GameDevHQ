using UnityEngine;

namespace UIElements
{
    public abstract class RaycastBase : I2DRaycast, I3DRaycast
    {
        protected RaycastBase(IGameObject parent)
        {
            _controller = parent;
            _layerToHit = parent.LayerToHit;
        }

        //Variables
        private GameObject _lastGameObject;
        private readonly IGameObject _controller;
        protected LayerMask _layerToHit;
        protected readonly Camera _mainCamera = Camera.main;

        //Properties
        protected Vector3 CameraPosition => _mainCamera.transform.position;

        //Main
        public void DoSelectedInGameObj()
        {
            if (_controller.SelectPressed && _lastGameObject.IsNotNull())
            {
                _lastGameObject.GetComponent<ICursorHandler>().CursorDown();
            }
        }

        public void DoRaycast(Vector3 virtualCursorPos) => OverGameObj(RaycastToObj(virtualCursorPos));

        private void OverGameObj(GameObject hit)
        {
            if (hit.IsNull())
            {
                ExitLastObject();
                return;
            }
            
            if (_lastGameObject.IsNotNull())
            {
                if (_lastGameObject == hit) return;
                _lastGameObject.GetComponent<ICursorHandler>().CursorExit();
            }
        
            _lastGameObject = hit;
            hit.GetComponent<ICursorHandler>().CursorEnter();
        }

        private void ExitLastObject()
        {
            if (_lastGameObject.IsNull()) return;
            
            _lastGameObject.GetComponent<ICursorHandler>().CursorExit();
            _lastGameObject = null;
        }

        protected abstract GameObject RaycastToObj(Vector3 virtualCursorPos);
    }
}