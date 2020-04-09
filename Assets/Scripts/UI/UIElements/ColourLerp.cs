using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColourLerp : MonoBehaviour
{
    public float _crossFadeTime;
    public Color _startColour;
    public float _percentage;

    Action<Color> _colourToReturn;

    public void StartLerp(Color from, Color too, Action<Color> returnColour )
    {
        StopAllCoroutines();
        _colourToReturn = returnColour;
        StartCoroutine(LerpColour(from, too));
    }

    public IEnumerator LerpColour(Color from, Color too)
    {
        float perc = 0;
        float timer = 0;
        _startColour = Color.clear;
        while (_startColour != too)
        {
            timer += Time.deltaTime;
            perc = timer / _crossFadeTime;
            _percentage = timer;
            _startColour = Color.Lerp(from, too, perc);
            _colourToReturn.Invoke(_startColour);
            yield return null;
        }
        yield return null;
    }

}
