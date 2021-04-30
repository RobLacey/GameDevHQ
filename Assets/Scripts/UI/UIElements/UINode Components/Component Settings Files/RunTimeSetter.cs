using System;
using UnityEngine;
using UnityEngine.UI;

public class RunTimeSetter : MonoBehaviour, IRunTimeSetter
{
    private LayoutGroup[] _toolTips;
    
    //Tooltips
    public Action<RectTransform> SetWorldFixedPosition{ get; set; }
    public void SetToolTipObjects(LayoutGroup[] newToolTip) => _toolTips = newToolTip;
    public LayoutGroup[] ReturnToolTipObjects() => _toolTips;
    
    //Navigation
    public Action<IBranch> SetChildBranch { get; set; }
    public Action<IBranch> SetUp { get; set; }
    public Action<IBranch> SetDown { get; set; }
    public Action<IBranch> SetLeft { get; set; }
    public Action<IBranch> SetRight { get; set; }
    
    //Events
    
    //AccessoriesList
}

public interface IRunTimeSetter
{
    Action<RectTransform> SetWorldFixedPosition { get; set; }
    void SetToolTipObjects(LayoutGroup[] newToolTip);
    LayoutGroup[] ReturnToolTipObjects();

    Action<IBranch> SetChildBranch { get; set; }
    Action<IBranch> SetUp { get; set; }
    Action<IBranch> SetDown { get; set; }
    Action<IBranch> SetLeft { get; set; }
    Action<IBranch> SetRight { get; set; }
}

