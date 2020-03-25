using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Random = UnityEngine.Random;

public class TipController : MonoBehaviour
{
    [SerializeField] Text _tipText;
    [SerializeField] Tip[] _tips;
    [SerializeField] float _fadeTime = 2f;
    [SerializeField] float _displayTime = 3f;

    [Serializable]
    public class Tip
    {
        public string _name;
        public string _tip;
    }

    private void Start()
    {
        _tipText.color = new Color(_tipText.color.r, _tipText.color.g, _tipText.color.b, 0);
        StartCoroutine(RotateTips());
    }

    IEnumerator RotateTips()
    {
        int index = 0;
        while (true)
        {
            _tipText.text = _tips[index]._tip;

            yield return FadeInText();

            yield return new WaitForSeconds(_displayTime);

            yield return FadeOutText();

            if (index == _tips.Length -1)
            {
                index = 0;
            }
            else
            {
                index++;
            }
        }
    }

    IEnumerator FadeInText()
    {
        float perc;
        float timer = 0;

        while (_tipText.color.a < 1)
        {
            timer += Time.deltaTime;
            perc = timer / _fadeTime;
            _tipText.color = _tipText.color.FadeUp(perc);
            yield return null;
        }

    }

    IEnumerator FadeOutText()
    {
        float perc;
        float timer = 0;

        while (_tipText.color.a > 0)
        {
            timer += Time.deltaTime;
            perc = timer / _fadeTime;
            _tipText.color = _tipText.color.FadeDown(perc);
            yield return null;
        }

    }
}
