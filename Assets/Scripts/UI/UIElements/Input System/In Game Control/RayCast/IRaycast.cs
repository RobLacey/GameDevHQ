using UnityEngine;

namespace UIElements
{
    public interface IRaycast : IMonoStart, IMonoEnable
    {
        void DoRaycast(Vector3 virtualCursorPos);
        void WhenInMenu();
    }
}