
public static class SizeAndPositionFactory
{
    private static readonly IEJect ieJect = new EJect();
    
    public static INodeTween AssignType(TweenEffect tweenEffect, ISizeAndPosition parent)
    {
        switch (tweenEffect)
        {
            case TweenEffect.Punch:
            {
                return ieJect.WithParams<IPunch>(parent);
            }            
            case TweenEffect.Shake:
            {
                return ieJect.WithParams<IShake>(parent);
            }
            case TweenEffect.Position:
            {
                return ieJect.WithParams<IPosition>(parent);
            }
            case TweenEffect.Scale:
            {
                return ieJect.WithParams<IScale>(parent);
            }
        }

        return null;
    }
}
