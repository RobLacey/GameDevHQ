using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColourLerp : MonoBehaviour
{
    Color _currentColour;

    public Color StartColour { set { _currentColour = value; } }

    public void NewLerp()
    {
        StopAllCoroutines();
    }

    public void StartLerp(Color too, float crossFadeTime, Action<Color> returnColour)
    {
        StartCoroutine(LerpColour(too, crossFadeTime, returnColour));
    }

    private IEnumerator LerpColour(Color too, float crossFadeTime, Action<Color> colourToReturn)
    {
        float perc = 0;
        float timer = 0;
        Color startColour = _currentColour;
        while (_currentColour != too)
        {
            timer += Time.deltaTime;
            perc = timer / crossFadeTime;
            _currentColour = Color.Lerp(startColour, too, perc);
            colourToReturn.Invoke(_currentColour);
            yield return null;
        }
        yield return null;
    }

    public void StartFlash(Color too, float flashTime, Action<Color> colourToReturn, Color targetColour)
    {
        StartCoroutine(OnOffControl(too, flashTime, colourToReturn, targetColour));
    }

    private IEnumerator OnOffControl(Color too, float flashTime, Action<Color> colourToReturn, Color targetColour)
    {
        yield return StartCoroutine(LerpColour(too, flashTime, colourToReturn));
        yield return StartCoroutine(LerpColour(targetColour, flashTime, colourToReturn));
    }
}
