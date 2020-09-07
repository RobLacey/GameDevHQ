
using DG.Tweening;
using UnityEngine;

public class Shake : INodeTween
{
    private readonly UISizeAndPosition _myNode;
    private readonly string _shakeId;
    private Vector3 _startSize;

    public Shake(UISizeAndPosition node)
    {
        _myNode = node;
        _shakeId = $"shake{_myNode.GameObjectID}";

    }
    public void DoTween(IsActive activate)
    {
        if (activate == IsActive.Yes)
        {
            ShakeTo();
        }        
        else
        {
            DOTween.Kill(_shakeId);
        }
    }
    
    //TODO Fix Loop to reset properly

    private void ShakeTo()
    {
        int loopTime = 0;
        if (_myNode.CanLoop)  
            loopTime = -1; 

        DOTween.Kill(_shakeId);
        _myNode.MyTransform.localScale = _myNode.StartSize;
        var scaleBy = new Vector3(_myNode.ShakeData.scaleXBy, _myNode.ShakeData.scaleYBy, 0);
        
        _myNode.MyTransform.DOShakeScale(_myNode.ShakeData.time, scaleBy, _myNode.ShakeData.vibrato, 
                                         _myNode.ShakeData.randomness, _myNode.ShakeData.fadeOut)
                   .SetId(_shakeId)
                   .SetLoops(loopTime, LoopType.Yoyo)
                   .SetAutoKill(true)
                   .Play();
    }
}
