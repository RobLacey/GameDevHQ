﻿using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public interface ITooltipSettings : IComponentSettings
{
    RectTransform MainCanvas { get; }
    Camera UiCamera { get; }
    ToolTipScheme Scheme { get; }
    LayoutGroup[] ToolTips { get; }
}

[Serializable]
public class ToolTipSettings : ITooltipSettings
{
    [Header("General Settings")]
    [SerializeField] 
    private RectTransform _mainCanvas;
    [SerializeField] 
    private Camera _uiCamera;
    [SerializeField] 
    [AllowNesting] [ValidateInput("IsNull", "Add a Tooltip Scheme")] private ToolTipScheme _scheme;
    [SerializeField] 
    private LayoutGroup[] _listOfTooltips = new LayoutGroup[0];

    //Editor Scripts
    private bool IsNull(ToolTipScheme scheme) => scheme;

    public RectTransform MainCanvas => _mainCanvas;
    public Camera UiCamera => _uiCamera;
    public ToolTipScheme Scheme => _scheme;
    public LayoutGroup[] ToolTips => _listOfTooltips;

    public NodeFunctionBase SetUp(IUiEvents uiNodeEvents, Setting functions)
    {
        if ((functions & Setting.TooplTip) != 0)
        {
            return new UITooltip(this, uiNodeEvents);
        }
        return new NullFunction();
    }
}