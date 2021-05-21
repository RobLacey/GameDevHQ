using DG.Tweening;
using UnityEngine;

public interface IShake : INodeTween { }

public class Shake : BaseShakePunch, IShake
{
    public Shake(IPunchShakeTween node) : base(node) { }

    private protected override void RunTween()
    {
        base.RunTween();
        _tweenData.MyTransform.DOShakeScale(_scheme.Time, _scheme.ChangeBy, 
                                            _scheme.Vibrato, _scheme.Randomness, _scheme.FadeOut)
                  .SetId(_id)
                  .SetLoops(_loopTime, LoopType.Restart)
                  .SetAutoKill(true)
                  .Play();

    }
}
