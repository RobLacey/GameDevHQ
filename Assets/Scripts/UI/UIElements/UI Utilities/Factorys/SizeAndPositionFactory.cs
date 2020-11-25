
public static class SizeAndPositionFactory
{
    private static readonly IInjectClass injectClass = new InjectClass();
    
    public static INodeTween AssignType(TweenEffect tweenEffect, ISizeAndPosition parent)
    {
        switch (tweenEffect)
        {
            case TweenEffect.Punch:
            {
                return injectClass.WithParams<IPunch>(parent);
            }            
            case TweenEffect.Shake:
            {
                return injectClass.WithParams<IShake>(parent);
            }
            case TweenEffect.Position:
            {
                return injectClass.WithParams<IPosition>(parent);
            }
            case TweenEffect.Scale:
            {
                return injectClass.WithParams<IScale>(parent);
            }
        }

        return null;
    }
}
