using DG.Tweening;

public abstract class BaseShakePunch
{
    protected readonly IPunchShakeTween _tweenData;
    protected readonly string _id;
    protected int _loopTime;

    protected BaseShakePunch(IPunchShakeTween node, string iD)
    {
        _tweenData = node;
        _id = $"ShakeOrPunch{iD}";
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
        _loopTime = _tweenData.CanLoop ? -1 : 0;
        DOTween.Kill(_id);
        _tweenData.MyTransform.localScale = _tweenData.StartSize;
    }

}
