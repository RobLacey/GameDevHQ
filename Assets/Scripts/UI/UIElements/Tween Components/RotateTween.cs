using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;


[System.Serializable]
public class RotateTween
{
    [InfoBox("Remeber to set your pivot point in the Inspector i.e Pivot X = 1 for left side hinge, 0 for right side etc.")]
    [SerializeField] [AllowNesting] [HideIf("UsingGlobalTime")] float _inTime = 1;
    [SerializeField] [AllowNesting] [HideIf("UsingGlobalTime")] float _outTime = 1;
    [SerializeField] [AllowNesting] [DisableIf("_running")] CurrentRoatationIs _currentRotationIs;
    [SerializeField] public Ease _easeIn = Ease.Unset;
    [SerializeField] public Ease _easeOut = Ease.Unset;

    //Variables
    bool _running = false;
    float _tweenTime;
    Ease _tweenEase;
    Vector3 _tweenTarget = Vector3.zero;
    List<BuildSettings> _listToUse;
    List<Tweener> _runningTweens = new List<Tweener>();
    List<BuildSettings> _reversedBuild = new List<BuildSettings>();
    List<BuildSettings> _buildList = new List<BuildSettings>();

    //Properties
    public bool UsingGlobalTime { get; set; }

    public void SetUpRotateTweens(RotationTweenType rotateTween, List<BuildSettings> buildObjectsList)
    {
        _running = true;
        _buildList = buildObjectsList;

        if (rotateTween == RotationTweenType.In || rotateTween == RotationTweenType.InAndOut)
        {
            SetUpIn(_buildList);
        }
        else if (rotateTween == RotationTweenType.Out)
        {
            SetUpOut(_buildList);
        }
    }

    private void SetUpIn(List<BuildSettings> buildSettings)
    {
        foreach (var item in buildSettings)
        {
            if (_currentRotationIs == CurrentRoatationIs.StartRotateAt)
            {
                item.rotateFrom = item._element.localRotation.eulerAngles;
                item.rotateTo = item._tweenRotateAngle;
                item._resetStartRotationStore = item._element.localRotation.eulerAngles; ;
                item._element.localRotation = Quaternion.Euler(item.rotateFrom);
            }
            else
            {
                item.rotateFrom = item._tweenRotateAngle;
                item.rotateTo = item._element.localRotation.eulerAngles;
                item._resetStartRotationStore = item._tweenRotateAngle;
                item._element.localRotation = Quaternion.Euler(item.rotateFrom);
            }
        }
        _reversedBuild = new List<BuildSettings>(buildSettings);
        _reversedBuild.Reverse();
    }

    private void SetUpOut(List<BuildSettings> buildSettings)
    {
        foreach (var item in buildSettings)
        {
            if (_currentRotationIs == CurrentRoatationIs.StartRotateAt)
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

    public void RewindRotateTweens()
    {
        foreach (var item in _buildList)
        {
            item._element.localRotation = Quaternion.Euler(item._resetStartRotationStore);
        }
    }

    public IEnumerator MoveSequence(RotationTweenType tweenType, TweenCallback tweenCallback = null)
    {
        bool finished = false;
        int index = 0;

        while (!finished)
        {
            foreach (var item in _listToUse)
            {
                if (tweenType == RotationTweenType.InAndOut)
                {
                    _tweenTarget = item.rotateFrom;
                }
                else
                {
                    _tweenTarget = item.rotateTo;
                }

                if (index == _listToUse.Count - 1)
                {
                    _runningTweens.Add (item._element.DOLocalRotate(_tweenTarget, _tweenTime).SetEase(_tweenEase)
                                                               .Play()
                                                               .SetAutoKill(true)
                                                               .OnComplete(tweenCallback));
                }
                else
                {

                    _runningTweens.Add(item._element.DOLocalRotate(_tweenTarget, _tweenTime).SetEase(_tweenEase)
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

    public void InSettings(float globalTime)
    {
        _tweenEase = _easeIn;
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
