using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using NaughtyAttributes;

[System.Serializable]
public class ScaleTweener 
{
    [SerializeField] [AllowNesting] [HideIf("UsingGlobalTime")] public float _InTime = 1;
    [SerializeField] [AllowNesting] [HideIf("UsingGlobalTime")] public float _OutTime = 1;
    [SerializeField]  public Ease _easeIn = Ease.Unset;
    [SerializeField]  public Ease _easeOut = Ease.Unset;
    [SerializeField]  public Vector3 _scaleToTweenTo = new Vector3(0.8f,0.8f,0f);

    //Varibales
    public List<Tweener> _scaleInTweeners = new List<Tweener>();
    public List<Tweener> _scaleOutTweeners = new List<Tweener>();
    [HideInInspector] public List<BuildSettings> _reversedBuildSettings = new List<BuildSettings>();
    [HideInInspector] public Vector3 _resetInOutscale;
    [HideInInspector] public Vector3 _resetOutscale;
    public bool UsingGlobalTime { get; set; }

    public void SetUpInAndOutTween(List<BuildSettings> buildSettings)
    {
        foreach (var item in buildSettings)
        {
            _resetInOutscale = _scaleToTweenTo;
            _scaleInTweeners.Add(item._element.transform.DOScale(item._element.transform.localScale, _InTime));
            _scaleOutTweeners.Add(item._element.transform.DOScale(_scaleToTweenTo, _OutTime));
            item._element.transform.localScale = _scaleToTweenTo;
        }
        _reversedBuildSettings = new List<BuildSettings>(buildSettings);
        _reversedBuildSettings.Reverse();
        _scaleOutTweeners.Reverse();
    }

    public void SetUpOutTween(List<BuildSettings> buildSettings)
    {
        foreach (var item in buildSettings)
        {
            _resetInOutscale = _scaleToTweenTo;
            _resetOutscale = item._element.transform.localScale;
            _scaleInTweeners.Add(item._element.transform.DOScale(item._element.transform.localScale, _InTime));
            _scaleOutTweeners.Add(item._element.transform.DOScale(_scaleToTweenTo, _OutTime));
        }
    }

    public void RewindScaleTweens(List<BuildSettings> buildSettings, List<Tweener> tweeners, Vector3 startScale)
    {
        foreach (var item in tweeners)
        {
            item.Rewind();
        }
        foreach (var item in buildSettings)
        {
            item._element.transform.localScale = startScale;
        }
    }

    public IEnumerator ScaleSequence(List<BuildSettings> buildSettings, List<Tweener> tweeners, 
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
                    tweeners[index].ChangeStartValue(item._element.transform.localScale, time).SetEase(ease)
                                                                                              .Play()
                                                                                              .OnComplete(tweenCallback);
                }
                else
                {
                    tweeners[index].ChangeStartValue(item._element.transform.localScale, time).SetEase(ease).Play();
                    yield return new WaitForSeconds(item._buildNextAfterDelay);
                    index++;
                }
            }
            finished = true;
        }
        yield return null;
    }
}
