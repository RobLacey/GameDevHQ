using UnityEngine;

namespace UIElements
{
    public interface IRaycast
    {
        bool DoSelectedInGameObj();
        void DoRaycast(Vector3 virtualCursorPos);
        void WhenInMenu();
    }
}