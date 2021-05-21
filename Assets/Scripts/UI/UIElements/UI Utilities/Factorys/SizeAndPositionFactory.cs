
using EZ.Inject;

public static class SizeAndPositionFactory
{
    private static readonly IEZInject injector = new EZInject();
    
    public static INodeTween AssignType(TweenEffect tweenEffect, ISizeAndPosition parent)
    {
        switch (tweenEffect)
        {
            case TweenEffect.Punch:
            {
                return injector.WithParams<IPunch>(parent);
            }            
            case TweenEffect.Shake:
            {
                return injector.WithParams<IShake>(parent);
            }
            case TweenEffect.Position:
            {
                return injector.WithParams<IPosition>(parent);
            }
            case TweenEffect.Scale:
            {
                return injector.WithParams<IScale>(parent);
            }
        }

        return null;
    }
}
