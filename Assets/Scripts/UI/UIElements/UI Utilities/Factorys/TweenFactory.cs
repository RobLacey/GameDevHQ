using System.Collections.Generic;

public class TweenFactory
{
    private static readonly IInjectClass injectClass = new IoC();
    
    public static List<ITweenBase> CreateTypes(TweenScheme scheme)
    {
        var activeTween = new List<ITweenBase>();
        
        if (scheme.Position())
        {
            activeTween.Add(injectClass.NoParams<IPositionTween>());
        }
        
        if (scheme.Rotation())
        {
            activeTween.Add(injectClass.NoParams<IRotationTween>());
        }
        
        if (scheme.Scale())
        {
            activeTween.Add(injectClass.NoParams<IScaleTween>());
        }
        
        if (scheme.Fade())
        {
            activeTween.Add(injectClass.NoParams<IFadeTween>());
        }

        if (scheme.Punch())
        {
            activeTween.Add(injectClass.NoParams<IPunchTween>());
        }
        
        if (scheme.Shake())
        {
            activeTween.Add(injectClass.NoParams<IShakeTween>());
        }
        return activeTween;
    }
    
}