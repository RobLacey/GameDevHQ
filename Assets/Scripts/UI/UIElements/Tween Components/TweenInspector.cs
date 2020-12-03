using System;
using System.Collections.Generic;
using UnityEngine;

public interface ITweenInspector
{
    void UpdateInspector();
    ITweenInspector CurrentScheme(TweenScheme scheme);
    ITweenInspector CurrentBuildList(List<BuildTweenData> newBuildList);
}

public class TweenInspector : ITweenInspector
{
    private bool _hasScheme;
    private bool _schemeSet;
    private bool _listSet;
    private TweenScheme _lastTweenScheme;
    private List<BuildTweenData> _buildList = new List<BuildTweenData>();
    private TweenScheme _scheme;

    public ITweenInspector CurrentScheme(TweenScheme scheme)
    {
        _schemeSet = true;
        _scheme = scheme;
        return this;
    }
    
    public ITweenInspector CurrentBuildList(List<BuildTweenData> newBuildList)
    {
        _listSet = true;
        _buildList = newBuildList;
        return this;
    }
    
    public void UpdateInspector()
    {
        if (CheckForCorrectUsage()) return;
        
        PassInSchemeToBuildObjects();
        
        if(_scheme is null && _hasScheme)
        {
            SchemeHasBeenDeleted();
            return;
        }

        if (_scheme && !_hasScheme)
        {
            SchemeHasBeenAdded();
        }
        ConfigureSettings();
    }

    private bool CheckForCorrectUsage()
    {
        if (_listSet && _schemeSet) return false;
        Debug.Log("Missing Scheme or list");
        return true;
    }

    private void PassInSchemeToBuildObjects()
    {
        foreach (var element in _buildList)
        {
            element.SetElement();
        }
    }

    private void SchemeHasBeenDeleted()
    {
        _hasScheme = false;
        if (_lastTweenScheme != null)
            _lastTweenScheme.Unsubscribe(ConfigureSettings);
        _lastTweenScheme = null;
        ClearTweenSettings();
    }

    private void SchemeHasBeenAdded()
    {
        _hasScheme = true;
        _lastTweenScheme = _scheme;
        _scheme.Subscribe(ConfigureSettings);
    }

    private void ConfigureSettings()
    {
        if(_scheme is null) return;
        foreach (var item in _buildList)
        {
            item.ActivateTweenSettings(_scheme);
        }
    }
    
    private void ClearTweenSettings()
    {
        foreach (var item in _buildList)
        {
            item.ClearSettings(TweenStyle.NoTween);
        }
    }

}
