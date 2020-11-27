using System.Collections.Generic;

public class TweenFactory
{
    private static readonly IEJect ieJect = new EJect();
    
    public static List<ITweenBase> CreateTypes(TweenScheme scheme)
    {
        var activeTween = new List<ITweenBase>();
        
        if (scheme.Position())
        {
            activeTween.Add(ieJect.NoParams<IPositionTween>());
        }
        
        if (scheme.Rotation())
        {
            activeTween.Add(ieJect.NoParams<IRotationTween>());
        }
        
        if (scheme.Scale())
        {
            activeTween.Add(ieJect.NoParams<IScaleTween>());
        }
        
        if (scheme.Fade())
        {
            activeTween.Add(ieJect.NoParams<IFadeTween>());
        }

        if (scheme.Punch())
        {
            activeTween.Add(ieJect.NoParams<IPunchTween>());
        }
        
        if (scheme.Shake())
        {
            activeTween.Add(ieJect.NoParams<IShakeTween>());
        }
        return activeTween;
    }
    
}