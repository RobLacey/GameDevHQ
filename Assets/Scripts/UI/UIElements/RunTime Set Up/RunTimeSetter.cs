using System;
using UnityEngine;
using UnityEngine.UI;

public interface IRunTimeSetter
{
    void ChangeMainText(string newText);
    
    Action<RectTransform> SetWorldFixedPosition { get; set; }
    void SetToolTipObjects(LayoutGroup[] newToolTip);
    LayoutGroup[] ReturnToolTipObjects();
    Action<IBranch> SetChildBranch { get; set; }
    void SetEvents(IEventSettings eventSettings);
    IEventSettings GetEvents();
}


public class RunTimeSetter : MonoBehaviour, IRunTimeSetter
{
    [SerializeField] private Image _uiImage;
    [SerializeField] private Text _uiText;
    
    //Variables
    private LayoutGroup[] _toolTips;
    private IEventSettings _eventSettings;
    
    private void Start()
    {
        if(_toolTips.IsNull()) return;
        
        int count = 1;
        INode node = GetComponent<INode>();
        foreach (var tip in _toolTips)
        {
            tip.name = $"Tool Tip {count} : {node.MyBranch} : {node.ReturnGameObject.name}";
            count++;
        }
    }
    
    //Main Settings
    public void ChangeMainText(string newText) => _uiText.text = newText;

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

