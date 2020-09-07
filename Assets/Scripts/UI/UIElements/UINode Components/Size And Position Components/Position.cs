using DG.Tweening;
using UnityEngine;

public class Position : INodeTween
{
    private readonly UISizeAndPosition _myNode;
    private readonly string _positionId;
    private Vector3 _targetPos;

    private Vector3 TweenEndTarget => _myNode.StartPosition + _myNode.PositionData.pixelsToMoveBy;

    public Position(UISizeAndPosition node)
    {
        _myNode = node;
        _positionId = $"position{_myNode.GameObjectID}";
    }

    public void DoTween(IsActive activate)
    {
        _targetPos = TweenEndTarget;
        
        if (_myNode.IsPressed)
        {
            DoPressedTween();
        }
        else
        {
            DoNotPressedTween(activate);
        }
    }

    private void DoPressedTween() => DoPositionTween(2, _myNode.PositionData.time);

    private void DoNotPressedTween(IsActive activate)
    {
        DOTween.Kill(_positionId);

        if (activate == IsActive.Yes)
        {
            SetUpLoopOrStandardTween();
        }
        else
        {
            TweenToStartPosition();
        }
    }

    private void TweenToStartPosition()
    {
        _targetPos = _myNode.StartPosition;
        DoPositionTween(0, _myNode.PositionData.time);
    }

    private void SetUpLoopOrStandardTween()
    {
        float time = _myNode.PositionData.time;

        if (_myNode.CanLoop && _myNode.MyRect.anchoredPosition3D != _myNode.StartPosition)
        {
            _targetPos = _myNode.StartPosition;
            DoPositionTween(0, time * 0.3f, FinalTweenCall);
        }
        else
        {
            FinalTweenCall();
        }
    }

    private void FinalTweenCall()
    {
        _targetPos = TweenEndTarget;
        int loopingCycles = _myNode.CanLoop ? -1 : 0;
        DoPositionTween(loopingCycles, _myNode.PositionData.time);
    }
    
    private void DoPositionTween (int loop, float time, TweenCallback callback = null)
    {
        _myNode.MyRect.DOAnchorPos3D(_targetPos, time, _myNode.PositionData.snapping)
               .SetId(_positionId)
               .SetLoops(loop, LoopType.Yoyo)
               .SetEase(_myNode.PositionData.ease)
               .SetAutoKill(true)
               .OnComplete(callback)
               .Play();
    }
}
