using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using NaughtyAttributes;

[System.Serializable]
public class PositionTween
{
    [SerializeField] [AllowNesting] [HideIf("UsingGlobalTime")] float _inTime = 1;
    [SerializeField] [AllowNesting] [HideIf("UsingGlobalTime")] float _outTime = 1;
    [SerializeField] [AllowNesting] [DisableIf("_running")] DestinationAs _currentPositionIs;
    [SerializeField] Ease _easeIn = Ease.Unset;
    [SerializeField] Ease _easeOut = Ease.Unset;
    [SerializeField] [AllowNesting] [DisableIf("_running")] bool _pixelSnapping = false;

    //Variables
    bool _running = false;
    float _tweenTime;
    Ease _tweenEase;
    List<Tweener> _tweensToUse;
    List<BuildSettings> _listToUse;
    List<BuildSettings> _reversedBuild = new List<BuildSettings>();
    List<BuildSettings> _buildList = new List<BuildSettings>();
    List<Tweener> _inTweeners = new List<Tweener>();
    List<Tweener> _outTweeners = new List<Tweener>();

    //Properties
    public bool UsingGlobalTime { get; set; }

    public void SetUpPositionTweens(PositionTweenType positionTween, List<BuildSettings> buildObjectsList)
    {
        _running = true;
        _buildList = buildObjectsList;
        if (positionTween == PositionTweenType.In || positionTween == PositionTweenType.InAndOut)
        {
            SetUpInTween(_buildList);
        }
        else if (positionTween == PositionTweenType.Out)
        {
            SetUpOutTween(_buildList);
        }
    }

    private void SetUpInTween(List<BuildSettings> buildSettings)
    {
        foreach (var item in buildSettings)
        {
            if (_currentPositionIs == DestinationAs.StartTweenAt)
            {
                item._resetStartPositionStore = item._element.anchoredPosition3D;
                _inTweeners.Add(item._element.DOAnchorPos3D(item._tweenAnchorPosition, _inTime, _pixelSnapping));
                _outTweeners.Add(item._element.DOAnchorPos3D(item._element.anchoredPosition3D, _outTime, _pixelSnapping));
            }
            else
            {
                item._resetStartPositionStore = item._tweenAnchorPosition;
                _inTweeners.Add(item._element.DOAnchorPos3D(item._element.anchoredPosition3D, _inTime, _pixelSnapping));
                _outTweeners.Add(item._element.DOAnchorPos3D(item._tweenAnchorPosition, _outTime, _pixelSnapping));
                item._element.anchoredPosition3D = item._tweenAnchorPosition;
            }
        }
        _reversedBuild = new List<BuildSettings>(buildSettings);
        _reversedBuild.Reverse();
        _outTweeners.Reverse();
    }

    private void SetUpOutTween(List<BuildSettings> buildSettings)
    {
        foreach (var item in buildSettings)
        {
            if (_currentPositionIs == DestinationAs.StartTweenAt)
            {
                item._resetStartPositionStore = item._element.anchoredPosition3D;
                _outTweeners.Add(item._element.DOAnchorPos(item._tweenAnchorPosition, _outTime, _pixelSnapping));
            }
            else
            {
                item._resetStartPositionStore = item._tweenAnchorPosition;
                _outTweeners.Add(item._element.DOAnchorPos(item._element.anchoredPosition3D, _outTime, _pixelSnapping));
                item._element.anchoredPosition3D = item._tweenAnchorPosition;
            }
        }
    }

    public IEnumerator MoveSequence(TweenCallback tweenCallback = null)
    {
        bool finished = false;
        int index = 0;
        while (!finished)
        {
            foreach (var item in _listToUse)
            {
                if (index == _listToUse.Count - 1)
                {
                    _tweensToUse[index].ChangeStartValue(item._element.anchoredPosition3D, _tweenTime).SetEase(_tweenEase)
                                                                                          .Play()
                                                                                          .OnComplete(tweenCallback);
                }
                else
                {
                    _tweensToUse[index].ChangeStartValue(item._element.anchoredPosition3D, _tweenTime).SetEase(_tweenEase).Play();
                    yield return new WaitForSeconds(item._buildNextAfterDelay);
                    index++;
                }
            }
            finished = true;
        }
        yield return null;
    }

    public void RewindInTweens()
    {
        foreach (var item in _inTweeners)
        {
            item.Rewind();
        }
    }

    public void RewindOutTweens()
    {
        foreach (var item in _outTweeners)
        {
           item.Rewind();
        }
    }

    public void PauseInTweens()
    {
        foreach (var item in _inTweeners)
        {
            item.Pause();
        }
    }

    public void PauseOutTweens()
    {
        foreach (var item in _outTweeners)
        {
            item.Pause();
        }
    }


    public void InSettings(float globalTime)
    {
        _tweenEase = _easeIn;
        _tweensToUse = _inTweeners;
        _listToUse = _buildList;
        if(globalTime > 0)
        {
            _tweenTime = globalTime;
        }
        else
        {
            _tweenTime = _inTime;
        }
    }
    public void OutSettings(float globalTime)
    {
        _tweenEase = _easeOut;
        _tweensToUse = _outTweeners;
        _listToUse = _buildList;

        if (globalTime > 0)
        {
            _tweenTime = globalTime;
        }
        else
        {
            _tweenTime = _outTime;
        }
    }
    public void InOutSettings(float globalTime)
    {
        _tweenEase = _easeOut;
        _tweensToUse = _outTweeners;
        _listToUse = _reversedBuild;
        if (globalTime > 0)
        {
            _tweenTime = globalTime;
        }
        else
        {
            _tweenTime = _outTime;
        }
    }
}
