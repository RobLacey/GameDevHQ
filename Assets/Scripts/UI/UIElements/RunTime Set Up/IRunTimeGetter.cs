using UnityEngine;
using UnityEngine.Events;

namespace UIElements
{
    public interface IRunTimeGetter
    {
        IBranch CreateBranch(UIBranch template);
        void BufferMainTextForCreation(string newText);
        void SetToolTipDataOnCreation(ToolTipRuntimeData[] tooltipData);
        void SetTooltips(ToolTipRuntimeData[] newTooltips);
        void SetTooltipAtIndex(int indexOfToolTip, string newTooltips);
        void SetFixedPosition(RectTransform newPos = null);
        void SetChildBranch(UIBranch childBranch);
        IEventSettings GetAllEvents();
        void SetOnEnterEvents(UnityEvent newEvent);
        void SetOnExitEvents(UnityEvent newEvent);
        void SetOnClickEvents(UnityEvent newEvent);
        void SetOnDisableEvents(OnDisabledEvent newEvent);
        void SetOnToggleEvents(OnToggleEvent newEvent);
    }
}