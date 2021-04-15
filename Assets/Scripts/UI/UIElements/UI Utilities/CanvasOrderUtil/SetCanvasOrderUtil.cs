using System;
using UnityEngine;

namespace UIElements
{
    public class SetCanvasOrderUtil
    {
        public static void Set(Func<int> getCanvasSortingOrder, Canvas canvas)
        {
            var storedCondition = canvas.enabled;
            canvas.enabled = true;
            canvas.overrideSorting = true;
            canvas.sortingOrder = getCanvasSortingOrder.Invoke();
            canvas.enabled = storedCondition;
        }
    }
}