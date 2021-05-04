using System.Collections.Generic;
using UIElements;
using UnityEngine;
using UnityEngine.UI;

public static class SetUpToolTipClass
{
    public static LayoutGroup[] SetUpList(LayoutGroup[] listOfTooltips)
    {
        int count = 1;
        var correctList = new List<LayoutGroup>();
        var tooltipBinTransform = GetToolTipFolder();
        
        foreach (var tooltip in listOfTooltips)
        {
            if (tooltip.gameObject.GetIsAPrefab())
            {
                CreateNewTooltip(tooltip, tooltipBinTransform, correctList);
                continue;
            }
            AddAlreadyExistingTooltip(tooltip, count, correctList);
        }
        return correctList.ToArray();
    }

    public static void SetFixedPositionName(RectTransform fixedPosition, string nodeName)
    {
        if (fixedPosition == null) return;
        
        fixedPosition.name = $"Fixed Position : {nodeName}";
    }

    private static void CreateNewTooltip(LayoutGroup tooltip, Transform tooltipBinTransform, 
                                      List<LayoutGroup> correctList)
    {
        var newTooltip = Object.Instantiate(tooltip, tooltipBinTransform);
        newTooltip.transform.position = Vector3.zero;
        correctList.Add(newTooltip);
    }

    private static void AddAlreadyExistingTooltip(LayoutGroup tooltip, int count, List<LayoutGroup> correctList)
    {
        tooltip.name = $"{tooltip.name} : {count}";
        correctList.Add(tooltip);
    }

    private static Transform GetToolTipFolder()
    {
        var hub = Object.FindObjectOfType<UIHub>();
        return new CreateNewObjects().CreateToolTipFolder(hub.transform).GetTooltipBin();
    }

    public static void SetRunTimeSetter(UITooltip tooltipClass, LayoutGroup[] tooltips, IRunTimeSetter myRunTimeSetter)
    {
        myRunTimeSetter.SetWorldFixedPosition = tooltipClass.SetFixedPositionAtRuntime;
        myRunTimeSetter.SetToolTipObjects(tooltips);
    }
}