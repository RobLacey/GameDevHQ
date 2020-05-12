using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;


[System.Serializable]
public class RotateTween
{
    [InfoBox("Remeber to set your pivot point in the Inspector i.e Pivot X = 1 for left side hinge, 0 for right side etc.")]
    [SerializeField] [AllowNesting] [HideIf("UsingGlobalTime")] public float _inTime = 1;
    [SerializeField] [AllowNesting] [HideIf("UsingGlobalTime")] public float _outTime = 1;
    [SerializeField] DestinationAs _currentRotationIs;
    [SerializeField] public Ease _easeIn = Ease.Unset;
    [SerializeField] public Ease _easeOut = Ease.Unset;

    //Variables
    enum DestinationAs { StartRotateAt, MidPointForInAndOut, EndRotateAt }
    public List<Tweener> _runningTweens = new List<Tweener>();
    [HideInInspector] public List<BuildSettings> _reversedBuild = new List<BuildSettings>();
    public bool UsingGlobalTime { get; set; }

    public void SetUpIn(List<BuildSettings> buildSettings)
    {
        foreach (var item in buildSettings)
        {
            if (_currentRotationIs == DestinationAs.StartRotateAt)
            {
                item.rotateFrom = item._element.localRotation.eulerAngles;
                item.rotateTo = item._tweenRotateAngle;
            }
            else
            {
                item.rotateFrom = item._tweenRotateAngle;
                item.rotateTo = item._element.localRotation.eulerAngles;
                item._resetStartRotationStore = item._tweenRotateAngle;
                item._element.localRotation = Quaternion.Euler(item._tweenRotateAngle);
            }
        }
        _reversedBuild = new List<BuildSettings>(buildSettings);
        _reversedBuild.Reverse();
    }

    public void SetUpOut(List<BuildSettings> buildSettings)
    {
        foreach (var item in buildSettings)
        {
            if (_currentRotationIs == DestinationAs.StartRotateAt)
            {
                item.rotateTo = item._tweenRotateAngle;
                item._resetStartPositionStore = item._element.localRotation.eulerAngles; ;
            }
            else
            {
                item.rotateTo = item._element.localRotation.eulerAngles;
                item._resetStartPositionStore = item._tweenRotateAngle;
                item._element.anchoredPosition = item._tweenRotateAngle;
            }
        }
    }

    public void KIllRunningTweens()
    {
        for (int i = 0; i < _runningTweens.Count; i++)
        {
            _runningTweens[i].Kill();
        }
        _runningTweens.Clear();

    }

    public void RewindRotateTweens(List<BuildSettings> buildSettings)
    {
        foreach (var item in buildSettings)
        {
            item._element.localRotation = Quaternion.Euler(item._resetStartRotationStore);
        }
    }

    public IEnumerator MoveSequence(List<BuildSettings> buildSettings, RotationTweenType tweenType,
                                    float time, Ease ease, TweenCallback tweenCallback = null)
    {
        bool finished = false;
        int index = 0;
        Vector3 tweenTarget = Vector3.zero;

        while (!finished)
        {
            foreach (var item in buildSettings)
            {
                if(tweenType == RotationTweenType.InAndOut)
                {
                    tweenTarget = item.rotateFrom;
                }
                else
                {
                    tweenTarget = item.rotateTo;
                }

                if (index == buildSettings.Count - 1)
                {
                    _runningTweens.Add (item._element.DOLocalRotate(tweenTarget, time).SetEase(ease)
                                                               .Play()
                                                               .SetAutoKill(true)
                                                               .OnComplete(tweenCallback));
                }
                else
                {
                    _runningTweens.Add(item._element.DOLocalRotate(tweenTarget, time).SetEase(ease)
                                                              .SetAutoKill(true)
                                                              .Play());
                    yield return new WaitForSeconds(item._buildNextAfterDelay);
                    index++;
                }
            }
            finished = true;
        }
        yield return null;
    }
}
