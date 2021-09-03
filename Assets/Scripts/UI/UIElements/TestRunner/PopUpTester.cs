using System;
using System.Collections.Generic;
using System.Linq;
using EZ.Service;
using UIElements;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class PopUpTester : IMonoEnable, IServiceUser
{
    [SerializeField] private GameObject _resolvePrefab;
    [SerializeField] private GameObject _optionalPrefab;
    [SerializeField] private GameObject _timedPrefab;
    
    
    private int _popUpCounter;
    private List<IBranch> _resolvePopUps = new List<IBranch>();
    private List<IBranch> _optionalPopUps = new List<IBranch>();
    private List<IBranch> _timedPopUps = new List<IBranch>();
    private IDataHub _myDataHub;
    
    public void OnEnable() => UseEZServiceLocator();

    public void UseEZServiceLocator()
    {
        _myDataHub = EZService.Locator.Get<IDataHub>(this);
    }

    public void MakeResolvePopUp() => MakePopUp(_resolvePrefab, _resolvePopUps, "Resolve");
    public void MakeOptionalPopUp()
    {
        if(!_myDataHub.NoResolvePopUp) return;
        MakePopUp(_optionalPrefab, _optionalPopUps, "Optional");
    }
    public void MakeTimedPopUp()
    {
        if(!_myDataHub.NoResolvePopUp) return;
        MakePopUp(_timedPrefab, _timedPopUps, "Timed");
    }

    private void MakePopUp(GameObject popUpPrefab, List<IBranch> popUpList, string nameOfPopUp)
    {
        IBranch newPopUp;
        foreach (var popUp in popUpList)
        {
            if (popUp.CanvasIsEnabled) continue;
            newPopUp = popUp;
            newPopUp.StartPopUp_RunTimeCall(true);
            return;
        }

        newPopUp = new RuntimeCreateBranch().CreatePopUp(popUpPrefab.GetComponent<UIBranch>())
                                            .NewName($"New {nameOfPopUp} PopUp : {_popUpCounter}");
        Vector2 randomPos = new Vector2(Random.Range(200, -200), Random.Range(200, -200));
        newPopUp.ThisBranchesGameObject.GetComponent<RectTransform>().anchoredPosition = randomPos;
        newPopUp.StartPopUp_RunTimeCall(false);
        _popUpCounter++;
        popUpList.Add(newPopUp);
    }
    
    public void RemoveResolvePopUp() => RemovePopUp(_resolvePopUps);

    public void RemoveOptionalPopUp() => RemovePopUp(_optionalPopUps);

    public void RemoveTimedPopUp() => RemovePopUp(_timedPopUps);

    private void RemovePopUp(List<IBranch> popUpList)
    {
        if (popUpList.Count <= 0) return;
        
        foreach (var popUp in popUpList.Where(popUp => popUp.CanvasIsEnabled))
        {
            popUp.OnDisable();
            return;
        }
    }

}