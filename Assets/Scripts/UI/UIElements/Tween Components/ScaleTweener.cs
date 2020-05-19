using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using NaughtyAttributes;

[System.Serializable]
public class ScaleTweener 
{
    [SerializeField] [AllowNesting] [HideIf("UsingGlobalTime")] float _inTime = 1;
    [SerializeField] [AllowNesting] [HideIf("UsingGlobalTime")] float _outTime = 1;
    [SerializeField] Ease _easeIn = Ease.Unset;
    [SerializeField] Ease _easeOut = Ease.Unset;
    [SerializeField] [AllowNesting] [DisableIf("_running")] Vector3 _scaleToTweenTo = new Vector3(0.8f,0.8f,0f);

    //Varibales
    bool _running = false;
    float _tweenTime;
    Ease _tweenEase;
    List<Tweener> _tweensToUse;
    List<BuildSettings> _listToUse;
    Vector3 _resetInOutscale;
    Vector3 _resetOutscale;
    List<Tweener> _scaleInTweeners = new List<Tweener>();
    List<Tweener> _scaleOutTweeners = new List<Tweener>();
    List<BuildSettings> _reversedBuildSettings = new List<BuildSettings>();
    List<BuildSettings> _buildList = new List<BuildSettings>();

    //Properties
    public bool UsingGlobalTime { get; set; }

    public void SetUpScaleTweens(ScaleTween scaleTweenWType, List<BuildSettings> buildObjectsList)
    {
        _running = true;
        _buildList = buildObjectsList;

        if (scaleTweenWType == ScaleTween.Scale_InOnly || scaleTweenWType == ScaleTween.Scale_InAndOut)
        {
            SetUpInAndOutTween(_buildList);
        }

        if (scaleTweenWType == ScaleTween.Scale_OutOnly)
        {
            SetUpOutTween(_buildList);
        }
    }

    private void SetUpInAndOutTween(List<BuildSettings> buildSettings)
    {
        foreach (var item in buildSettings)
        {
            _resetInOutscale = _scaleToTweenTo;
            _scaleInTweeners.Add(item._element.transform.DOScale(item._element.transform.localScale, _inTime));
            _scaleOutTweeners.Add(item._element.transform.DOScale(_scaleToTweenTo, _outTime));
            item._element.transform.localScale = _scaleToTweenTo;
        }
        _reversedBuildSettings = new List<BuildSettings>(buildSettings);
        _reversedBuildSettings.Reverse();
        _scaleOutTweeners.Reverse();
    }

    private void SetUpOutTween(List<BuildSettings> buildSettings)
    {
        foreach (var item in buildSettings)
        {
            _resetInOutscale = _scaleToTweenTo;
            _resetOutscale = item._element.transform.localScale;
            _scaleInTweeners.Add(item._element.transform.DOScale(item._element.transform.localScale, _inTime));
            _scaleOutTweeners.Add(item._element.transform.DOScale(_scaleToTweenTo, _outTime));
        }
    }

    public IEnumerator ScaleSequence(TweenCallback tweenCallback = null)
    {
        bool finished = false;
        int index = 0;
        while (!finished)
        {
            foreach (var item in _listToUse)
            {
                if (index == _listToUse.Count - 1)
                {
                    _tweensToUse[index].ChangeStartValue(item._element.transform.localScale, _tweenTime).SetEase(_tweenEase)
                                                                                              .Play()
                                                                                              .OnComplete(tweenCallback);
                }
                else
                {
                    _tweensToUse[index].ChangeStartValue(item._element.transform.localScale, _tweenTime).SetEase(_tweenEase).Play();
                    yield return new WaitForSeconds(item._buildNextAfterDelay);
                    index++;
                }
            }
            finished = true;
        }
        yield return null;
    }

    public void PauseInTweens()
    {
        foreach (var item in _scaleInTweeners)
        {
            item.Pause();
        }
    }
    public void PauseOutTweens()
    {
        foreach (var item in _scaleOutTweeners)
        {
            item.Pause();
        }
    }

    public void RewindScaleInTweens()
    {
        foreach (var item in _scaleInTweeners)
        {
            item.Rewind();
        }
        foreach (var item in _buildList) 
        {
            item._element.transform.localScale = _resetInOutscale;
        }
    }

    public void RewindScaleOutTweens()
    {
        foreach (var item in _scaleOutTweeners)
        {
            item.Rewind();
        }
        foreach (var item in _buildList) 
        {
            item._element.transform.localScale = _resetOutscale;
        }
    }

    public void InSettings(float globalTime)
    {
        _tweenEase = _easeIn;
        _tweensToUse = _scaleInTweeners;
        _listToUse = _buildList;
        if (globalTime > 0)
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
        _tweensToUse = _scaleOutTweeners;
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
        _tweensToUse = _scaleOutTweeners;
        _listToUse = _reversedBuildSettings;
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
