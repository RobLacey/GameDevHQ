using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public interface IToolTipFade
{
    IStartToolTipTween SetTweenTime(float time);
}
public interface IStartToolTipTween
{
    Tween StartFadeIn(int instanceId);
    Tween StartFadeOut(int instanceId);
}

public class ToolTipFade : IToolTipFade, IStartToolTipTween
{
    private readonly Dictionary<int, CanvasGroup> _toolTipsCanvasGroups = new Dictionary<int, CanvasGroup>();
    private float _time;
    
    public ToolTipFade(IToolTipData data)
    {
        foreach (var toolTip in data.ToolTipsRects)
        {
            var hasCanvasGroup = toolTip.GetComponent<CanvasGroup>();
            
            hasCanvasGroup = AddCanvasGroupIfOneNotPresent(hasCanvasGroup, toolTip);

            hasCanvasGroup.alpha = 0;
            var iD = toolTip.GetInstanceID();
            _toolTipsCanvasGroups.Add(iD, hasCanvasGroup);
        }
    }

    private static CanvasGroup AddCanvasGroupIfOneNotPresent(CanvasGroup hasCanvasGroup, RectTransform toolTip)
    {
        if (!hasCanvasGroup)
        {
            hasCanvasGroup = toolTip.gameObject.AddComponent<CanvasGroup>();
        }
        return hasCanvasGroup;
    }

    public IStartToolTipTween SetTweenTime(float time)
    {
        _time = time;
        return this;
    }
    
    public Tween StartFadeIn(int instanceId)
    {
        _toolTipsCanvasGroups[instanceId].alpha = 0;
        return DoFade(instanceId, 1f);
    }

    public Tween StartFadeOut(int instanceId) => DoFade(instanceId, 0);
    
    private Tween DoFade(int instanceId, float tweenToValue)
    {
        return _toolTipsCanvasGroups[instanceId].DOFade(tweenToValue, _time)
                                                .SetEase(Ease.Linear)
                                                .SetAutoKill(true)
                                                .Play();
    }
}

