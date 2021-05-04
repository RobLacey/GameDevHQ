using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Fader : MonoBehaviour
{
    [SerializeField] Image _foreground;
    [SerializeField] bool _fadeIn;
    [SerializeField] bool _fadeOut;
    [SerializeField] float _fadeInTime = 3f;
    [SerializeField] float _fadeOutTime = 2f;
    [SerializeField] float _fadeInDelays = 1f;
    [SerializeField] float _FadeOutDelays = 1f;
    [SerializeField] EventManager _Event_Start_Level_Exit_Process;

    private void Awake()
    {
        _foreground.gameObject.SetActive(true);
    }

    private void OnEnable()
    {
        _Event_Start_Level_Exit_Process.AddListener((x) => StartFadeOut(x), this);
    }

    private void Start()
    {
        StartFadeIn();
    }

    private void StartFadeIn()
    {
        if (_fadeIn)
        {
            StartCoroutine(FadeIn());
        }
        else
        {
            _foreground.color = new Color(_foreground.color.r, _foreground.color.g, _foreground.color.b, 0);
        }
    }

    IEnumerator FadeIn()
    {
        float perc;
        float timer = 0;

        yield return new WaitForSeconds(_fadeInDelays);

        while (_foreground.color.a > 0)
        {
            timer += Time.deltaTime;
            perc = timer / _fadeInTime;
            _foreground.color = _foreground.color.FadeDown(perc);
            yield return null;
        }

        yield return new WaitForSeconds(_fadeInDelays);
    }

    private void StartFadeOut(object newCallback)
    {
        StartCoroutine(FadeOut((Action) newCallback));
    }

    private IEnumerator FadeOut(Action callback)
    {
        float perc;
        float timer = 0; 

        if (_fadeOut)
        {
            yield return new WaitForSeconds(_FadeOutDelays);

            while (_foreground.color.a < 1)
            {
                timer += Time.deltaTime;
                perc = timer / _fadeOutTime;
                _foreground.color = _foreground.color.FadeUp(perc);
                yield return null;
            }
            yield return new WaitForSeconds(_FadeOutDelays);

        }        
        callback.Invoke();
    }
}
