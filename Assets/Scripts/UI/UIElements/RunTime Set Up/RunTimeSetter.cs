using System;
using UnityEngine;
using UnityEngine.UI;

public interface IRunTimeSetter
{
    Action<RectTransform> SetWorldFixedPosition { get; set; }
    void SetToolTipObjects(LayoutGroup[] newToolTip);
    LayoutGroup[] ReturnToolTipObjects();
    Action<IBranch> SetChildBranch { get; set; }
    void SetEvents(IEventSettings eventSettings);
    IEventSettings GetEvents();
}


public class RunTimeSetter : MonoBehaviour, IRunTimeSetter
{
    //Variables
    private LayoutGroup[] _toolTips;
    private IEventSettings _eventSettings;
    
    //Tooltips
    public Action<RectTransform> SetWorldFixedPosition{ get; set; }
    public void SetToolTipObjects(LayoutGroup[] newToolTip) => _toolTips = newToolTip;
    public LayoutGroup[] ReturnToolTipObjects() => _toolTips;
    
    //Navigation
    public Action<IBranch> SetChildBranch { get; set; }
    
    //Events
    public void SetEvents(IEventSettings eventSettings) => _eventSettings = eventSettings;
    public IEventSettings GetEvents() => _eventSettings;
}

