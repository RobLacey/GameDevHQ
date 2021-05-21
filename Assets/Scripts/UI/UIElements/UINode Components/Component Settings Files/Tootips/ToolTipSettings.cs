using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public interface ITooltipSettings : IComponentSettings
{
    IUiEvents UiNodeEvents { get; }
    Camera UiCamera { get; }
    ToolTipScheme Scheme { get; }
    LayoutGroup[] ToolTips { get; }
    RectTransform FixedPosition { get; }
}

[Serializable]
public class ToolTipSettings : ITooltipSettings
{
    [Header("General Settings")]
    [SerializeField] 
    private Camera _uiCamera;
    
    [SerializeField] 
    [AllowNesting] [ValidateInput("IsNull", "Add a Tooltip Scheme")] 
    private ToolTipScheme _scheme;
    
    [SerializeField] 
    private LayoutGroup[] _listOfTooltips = new LayoutGroup[0];

    [SerializeField]
    [AllowNesting] [ValidateInput("NeedFixedPos", FixedPosMessage)]
    private RectTransform _fixedPosition;
    
    //Variable
    private UINode _myNode;
    private const string FixedPosMessage = "Make Sure a RectTransform For the Fixed Position is added. " +
                                           "If none added the OBJECT CENTRE will be used as DEFAULT";

    //Editor Scripts
    private bool IsNull(ToolTipScheme scheme) => scheme;
    private bool NeedFixedPos(RectTransform rect)
    {
        if (!_scheme) return true;
        return (!_scheme.MouseFixed() && !_scheme.KeysFixed()) || rect;
    }
    
    //Properties
    public Camera UiCamera => _uiCamera;
    public ToolTipScheme Scheme => _scheme;
    public LayoutGroup[] ToolTips { get; private set; }
    public IUiEvents UiNodeEvents { get; private set; }
    public RectTransform FixedPosition => _fixedPosition;

    public NodeFunctionBase SetUp(IUiEvents uiNodeEvents, Setting functions)
    {
        var activeFunction = (functions & Setting.ToolTip) != 0;

        UiNodeEvents = uiNodeEvents;
        _myNode = uiNodeEvents.ReturnMasterNode;
        
        if (activeFunction)
        {
            var name = $"{_myNode.MyBranch.ThisBranchesGameObject.name} : {_myNode.name}";
            SetUpToolTipClass.SetFixedPositionName(_fixedPosition, name);
            ToolTips = SetUpToolTipClass.SetUpList(_listOfTooltips);
            var newClass = new UITooltip(this);
            SetUpToolTipClass.SetRunTimeSetter(newClass, ToolTips, _myNode.MyRunTimeSetter);
            return newClass;
        }
        return null;
    }
}



