using DG.Tweening;

public class Shake : BaseShakePunch, INodeTween
{
    public Shake(IPunchShakeTween node, string iD) : base(node, iD) { }

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
