using DG.Tweening;

public class Shake : BaseShakePunch, INodeTween
{
    public Shake(IPunchShakeTween node, string iD) : base(node, iD) { }

    private protected override void RunTween()
    {
        base.RunTween();
        _tweenData.MyTransform.DOShakeScale(_tweenData.Time, _tweenData.ChangeBy, 
                                            _tweenData.Vibrato, _tweenData.Randomness, _tweenData.FadeOut)
                  .SetId(_id)
                  .SetLoops(_loopTime, LoopType.Restart)
                  .SetAutoKill(true)
                  .Play();

    }
}
