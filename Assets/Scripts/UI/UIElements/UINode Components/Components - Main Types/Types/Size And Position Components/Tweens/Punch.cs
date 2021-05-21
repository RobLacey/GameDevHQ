using DG.Tweening;
using UnityEngine;

public interface IPunch : INodeTween{ }


public class Punch : BaseShakePunch, IPunch
{
    public Punch(IPunchShakeTween node) : base(node) { }
    
    private protected override void RunTween()
    {
        Debug.Log("Punch");
        base.RunTween();
        _tweenData.MyTransform.DOPunchScale(_scheme.ChangeBy, _scheme.Time, 
                                            _scheme.Vibrato, _scheme.Elasticity)
                    .SetId(_id)
                    .SetLoops(_loopTime, LoopType.Restart)
                    .SetAutoKill(true)
                    .Play();
    }
}
