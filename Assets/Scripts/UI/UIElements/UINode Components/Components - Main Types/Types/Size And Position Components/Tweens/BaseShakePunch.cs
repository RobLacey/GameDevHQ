using DG.Tweening;

public abstract class BaseShakePunch
{
    protected readonly IPunchShakeTween _tweenData;
    protected readonly SizeAndPositionScheme _scheme;
    protected readonly string _id;
    protected int _loopTime;

    protected BaseShakePunch(IPunchShakeTween node)
    {
        _tweenData = node;
        _scheme = node.Scheme;
        _id = $"ShakeOrPunch{node.GameObjectID}";
    }
    
    public void DoTween(IsActive activate)
    {
        if (activate == IsActive.Yes)
        {
            RunTween();
        }
        else
        {
            DOTween.Kill(_id);
            _tweenData.MyTransform.localScale = _tweenData.StartSize;
        }
    }
    
    private protected virtual void RunTween()
    {
        _loopTime = _scheme.CanLoop ? -1 : 0;
        DOTween.Kill(_id);
        _tweenData.MyTransform.localScale = _tweenData.StartSize;
    }

}
