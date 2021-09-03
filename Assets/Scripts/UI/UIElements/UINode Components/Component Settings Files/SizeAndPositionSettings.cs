
using System;
using UnityEngine;

public interface ISizeAndPositionSettings : IComponentSettings, IOverride
{
    SizeAndPositionScheme Scheme { get; }
    RectTransform RectTransform { get; }
}

[Serializable]
public class SizeAndPositionSettings : ISizeAndPositionSettings
{
    [SerializeField] private RectTransform _rectTransforms = default;
    [SerializeField] private SizeAndPositionScheme _scheme = default;
    [SerializeField] private Override _overrideAlwaysHighlighted = Override.Allow;

    public SizeAndPositionScheme Scheme => _scheme;
    public RectTransform RectTransform => _rectTransforms;
    public IBranch ParentBranch { get; private set; }
    public Override OverrideAlwaysHighlighted => _overrideAlwaysHighlighted;

    public NodeFunctionBase SetUp(IUiEvents uiNodeEvents, Setting functions)
    {
        ParentBranch = uiNodeEvents.ReturnMasterNode.MyBranch;
        
        CheckForSetUpError(functions, uiNodeEvents.ReturnMasterNode);
        
        if (CanCreate(functions))
        {
            return new UISizeAndPosition(this, uiNodeEvents);
        }
        return null;
    }
    
    private bool CanCreate(Setting functions) => (functions & Setting.SizeAndPosition) != 0;

    private void CheckForSetUpError(Setting functions, UINode parentNode) 
    {
        if(!CanCreate(functions)) return;
        
        if(_scheme.IsNull())
            throw new Exception($"No Scheme set in Size & Position settings for {parentNode}");

        if (_scheme.IsNotNull() && _rectTransforms == null)
            throw new Exception($"No Rect Transform set in Size & Position settings for {parentNode}");
    }

}
