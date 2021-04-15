using UnityEngine;

namespace UIElements
{
    public interface IRaycast
    {
        void DoRaycast(Vector3 virtualCursorPos);
        void WhenInMenu();
    }
}