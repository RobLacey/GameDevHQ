using DG.Tweening;

public class Punch : BaseShakePunch, INodeTween
{
    public Punch(IPunchShakeTween node, string iD) : base(node, iD) { }
    
    private protected override void RunTween()
    {
        base.RunTween();
        _tweenData.MyTransform.DOPunchScale(_tweenData.ChangeBy, _tweenData.Time, 
                                            _tweenData.Vibrato, _tweenData.Elasticity)
                    .SetId(_id)
                    .SetLoops(_loopTime, LoopType.Restart)
                    .SetAutoKill(true)
                    .Play();
    }
}
