using DG.Tweening;

public class Punch : BaseShakePunch, INodeTween
{
    public Punch(IPunchShakeTween node, string iD) : base(node, iD) { }
    
    private protected override void RunTween()
    {
        base.RunTween();
        _tweenData.MyTransform.DOPunchScale(_scheme.ChangeBy, _scheme.Time, 
                                            _scheme.Vibrato, _scheme.Elasticity)
                    .SetId(_id)
                    .SetLoops(_loopTime, LoopType.Restart)
                    .SetAutoKill(true)
                    .Play();
    }
}
