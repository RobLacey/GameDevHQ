
public static class SizeAndPositionFactory
{
    public static INodeTween AssignType(TweenEffect tweenEffect, UISizeAndPosition parent, string iD)
    {
        switch (tweenEffect)
        {
            case TweenEffect.Punch:
                return new Punch(parent, iD);
            case TweenEffect.Shake:
                return new Shake(parent, iD);
            case TweenEffect.Position:
                return new Position(parent, iD);
            case TweenEffect.Scale:
                return new Scale(parent, iD);
        }

        return null;
    }
}
