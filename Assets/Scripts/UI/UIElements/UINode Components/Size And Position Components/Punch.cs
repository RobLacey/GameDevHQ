
using System;
using UnityEngine;
using DG.Tweening;

public interface INodeTween
{
    void DoTween(IsActive activate);
}

public class Punch : INodeTween
{
    private readonly UISizeAndPosition _myNode;
    private readonly string _punchId;
    private Vector3 _startSize;

    public Punch(UISizeAndPosition node)
    {
        _myNode = node;
        _punchId = $"punch{_myNode.GameObjectID}";
    }
    
    public void DoTween(IsActive activate)
    {
        if (activate == IsActive.Yes)
        {
            RunPunchTween();
        }
        else
        {
            DOTween.Kill(_punchId);
        }
    }
    
    //TODO Fix Loop to reset properly
    
    private void RunPunchTween()
    {
        int loopTime = 0;
        if (_myNode.CanLoop) 
            loopTime = -1;

        DOTween.Kill(_punchId);
        _myNode.MyTransform.localScale = _myNode.StartSize;
        var scaleBy = new Vector3(_myNode.PunchData.scaleXBy, _myNode.PunchData.scaleXBy, 0);
        
        _myNode.MyTransform.DOPunchScale(scaleBy, _myNode.PunchData.time, 
                                  _myNode.PunchData.vibrato, _myNode.PunchData.elasticity)
                    .SetId(_punchId)
                    .SetLoops(loopTime, LoopType.Yoyo)
                    .SetAutoKill(true)
                    .Play();
    }
}
