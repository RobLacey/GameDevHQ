using UnityEngine;

public static class InGameUiExtensionMethod
{
    public static bool DoBranchesOverlap(this RectTransform nodeRectTransform, RectTransform branchRectTransform)
    {
        Rect rect1 = new Rect(nodeRectTransform.rect);
        Rect rect2 = new Rect(branchRectTransform.rect);
        return rect1.Overlaps(rect2);
    }
}