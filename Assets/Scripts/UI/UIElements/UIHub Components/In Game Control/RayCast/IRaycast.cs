using UnityEngine;

namespace UIElements
{
    public interface IRaycast
    {
        void DoSelectedInGameObj();
        void DoRaycast(Vector3 virtualCursorPos);
        void WhenInMenu();
    }
}