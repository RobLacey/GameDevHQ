using UnityEngine.UI;

public class NameToolTips
{
    public static void NameTooltips(LayoutGroup[] listOfToolTips, IUiEvents uiEvents)
    {
        int count = 1;
        foreach (var tooltip in listOfToolTips)
        {
            tooltip.name = $"{uiEvents.ReturnMasterNode.MyBranch.ThisBranchesGameObject.name} : " +
                           $"{uiEvents.ReturnMasterNode.name} : {count}";
            count++;
        }
    }
}