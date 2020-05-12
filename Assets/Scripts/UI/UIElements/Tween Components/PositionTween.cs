using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using NaughtyAttributes;

[System.Serializable]
public class PositionTween
{
    [SerializeField] [AllowNesting] [HideIf("UsingGlobalTime")] public float _inTime = 1;
    [SerializeField] [AllowNesting] [HideIf("UsingGlobalTime")] public float _outTime = 1;
    [SerializeField] DestinationAs _currentPositionIs;
    [SerializeField] public Ease _easeIn = Ease.Unset;
    [SerializeField] public Ease _easeOut = Ease.Unset;
    [SerializeField] bool _snapping = false;


    //Variables
    enum DestinationAs { StartTweenAt, MidPointForInAndOut, EndTweenAt }
    public List<Tweener> _inTweeners = new List<Tweener>();
    public List<Tweener> _outTweeners = new List<Tweener>();
    [HideInInspector] public List<BuildSettings> _reversedBuild = new List<BuildSettings>();
    public bool UsingGlobalTime { get; set; }

    public void SetUpIn(List<BuildSettings> buildSettings)
    {
        foreach (var item in buildSettings)
        {
            if (_currentPositionIs == DestinationAs.StartTweenAt)
            {
                item._resetStartPositionStore = item._element.anchoredPosition;
                _inTweeners.Add(item._element.DOAnchorPos(item._tweenAnchorPosition, _inTime, _snapping));
                _outTweeners.Add(item._element.DOAnchorPos(item._element.anchoredPosition, _outTime, _snapping));
            }
            else
            {
                item._resetStartPositionStore = item._tweenAnchorPosition;
                _inTweeners.Add(item._element.DOAnchorPos(item._element.anchoredPosition, _inTime, _snapping));
                _outTweeners.Add(item._element.DOAnchorPos(item._tweenAnchorPosition, _outTime, _snapping));
                item._element.anchoredPosition = item._tweenAnchorPosition;
            }
        }
        _reversedBuild = new List<BuildSettings>(buildSettings);
        _reversedBuild.Reverse();
        _outTweeners.Reverse();

    }

    public void SetUpOut(List<BuildSettings> buildSettings)
    {
        foreach (var item in buildSettings)
        {
            if (_currentPositionIs == DestinationAs.StartTweenAt)
            {
                item._resetStartPositionStore = item._element.anchoredPosition;
                _outTweeners.Add(item._element.DOAnchorPos(item._tweenAnchorPosition, _outTime, _snapping));
            }
            else
            {
                item._resetStartPositionStore = item._tweenAnchorPosition;
                _outTweeners.Add(item._element.DOAnchorPos(item._element.anchoredPosition, _outTime, _snapping));
                item._element.anchoredPosition = item._tweenAnchorPosition;
            }
        }
    }

    public void RewindPositionTweens(List<BuildSettings> buildSettings, List<Tweener> tweeners)
    {
        foreach (var item in tweeners)
        {
            item.Rewind();
        }

        foreach (var item in buildSettings)
        {
            //item._element.anchoredPosition = item._resetStartPositionStore;
        }
    }

    public IEnumerator MoveSequence(List<BuildSettings> buildSettings, List<Tweener> tweeners, 
                                    float time, Ease ease, TweenCallback tweenCallback = null)
    {
        bool finished = false;
        int index = 0;
        while (!finished)
        {
            foreach (var item in buildSettings)
            {
                if (index == buildSettings.Count - 1)
                {
                    tweeners[index].ChangeStartValue(item._element.anchoredPosition, time).SetEase(ease)
                                                                                          .Play()
                                                                                          .OnComplete(tweenCallback);
                }
                else
                {
                    tweeners[index].ChangeStartValue(item._element.anchoredPosition, time).SetEase(ease).Play();
                    yield return new WaitForSeconds(item._buildNextAfterDelay);
                    index++;
                }
            }
            finished = true;
        }
        yield return null;
    }
}
