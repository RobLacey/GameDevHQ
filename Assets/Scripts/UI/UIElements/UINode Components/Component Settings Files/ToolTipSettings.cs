using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UIElements;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

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

    //Editor Scripts
    private bool IsNull(ToolTipScheme scheme) => scheme;
    private bool NeedFixedPos(RectTransform rect)
    {
        if (!_scheme) return true;
        return (!_scheme.MouseFixed() && !_scheme.KeysFixed()) || rect;
    }

    private const string FixedPosMessage = "Make Sure a RectTransform For the Fixed Position is added. " +
                                            "If none added the OBJECT CENTRE will be used as DEFAULT";
    
    //Properties
    public Camera UiCamera => _uiCamera;
    public ToolTipScheme Scheme => _scheme;
    public LayoutGroup[] ToolTips => _listOfTooltips;
    public IUiEvents UiNodeEvents { get; private set; }
    public RectTransform FixedPosition => _fixedPosition;

    public NodeFunctionBase SetUp(IUiEvents uiNodeEvents, Setting functions)
    {
        var activeFunction = (functions & Setting.ToolTip) != 0;
        
        UiNodeEvents = uiNodeEvents;
        _myNode = uiNodeEvents.ReturnMasterNode;
        
        if (activeFunction)
        {
            SetUpListIfPrefabs();
            var newClass = new UITooltip(this);
            SetRunTimeSetter(newClass);
            return newClass;
        }
        return new NullFunction();
    }

    private void SetUpListIfPrefabs()
    {
        SetFixedPositionName();
        int count = 1;
        List<LayoutGroup> correctList = new List<LayoutGroup>();
        var tooltipBinTransform = GetToolTipFolder();
        
        foreach (var tooltip in _listOfTooltips)
        {
            if (tooltip.gameObject.GetIsAPrefab())
            {
                CreateTooltip(tooltip, tooltipBinTransform, count, correctList);
                count++;
                continue;
            }
            AddAlreadyExistingTooltip(tooltip, count, correctList);
            count++;
        }
        
        _listOfTooltips = correctList.ToArray();
    }

    private void SetFixedPositionName()
    {
        if (_fixedPosition)
            _fixedPosition.name = $"Fixed Position : {_myNode.name}";
    }

    private void CreateTooltip(LayoutGroup tooltip, Transform tooltipBinTransform, 
                               int count, List<LayoutGroup> correctList)
    {
        var newTooltip = Object.Instantiate(tooltip, tooltipBinTransform);
        newTooltip.transform.position = Vector3.zero;
        newTooltip.name = $"ToolTip {count} : {_myNode.name}";
        correctList.Add(newTooltip);
    }

    private static void AddAlreadyExistingTooltip(LayoutGroup tooltip, int count, List<LayoutGroup> correctList)
    {
        tooltip.name = $"{tooltip.name} : {count}";
        correctList.Add(tooltip);
    }

    private Transform GetToolTipFolder()
    {
        var hub = Object.FindObjectOfType<UIHub>();
        return new CreateNewObjects().CreateToolTipFolder(hub.transform).GetTooltipBin();
    }

    private void SetRunTimeSetter(UITooltip tooltipClass)
    {
        var runtime = _myNode.GetComponent<IRunTimeSetter>();
        runtime.SetWorldFixedPosition = tooltipClass.SetFixedPositionAtRuntime;
        runtime.SetToolTipObjects(_listOfTooltips);
    }
}



