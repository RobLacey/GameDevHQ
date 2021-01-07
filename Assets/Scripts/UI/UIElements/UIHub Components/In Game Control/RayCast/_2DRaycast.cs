using UnityEngine;

namespace UIElements
{
    public interface I2DRaycast : IRaycast { }
    
    public class _2DRaycast : RaycastBase
    {
        public _2DRaycast(IRaycastController parent) : base(parent) { }
        
        private readonly Vector3 _direction2D = Vector3.forward;

        protected override ICursorHandler RaycastToObj(Vector3 virtualCursorPos)
        {
            var mousePos = virtualCursorPos;
            mousePos.z = 10;
            var origin = _mainCamera.ScreenToWorldPoint(mousePos);
            var hit = Physics2D.Raycast(origin, _direction2D, 0, _layerToHit);
            return hit.collider.IsNull() ? null : hit.collider.gameObject.GetComponent<ICursorHandler>();
        }
    }
}