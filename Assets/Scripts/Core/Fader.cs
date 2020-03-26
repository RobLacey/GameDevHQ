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
    [SerializeField] EventManager _Event_StartNextScene;
    [SerializeField] EventManager _Event_StartFadeOut;
    [SerializeField] EventManager _Event_StartFadeIn;
    [SerializeField] EventManager _Event_StartLoading;

    private void OnEnable()
    {
        _Event_StartFadeIn.AddListener(() => StartFadeIn());
        _Event_StartFadeOut.AddListener(() => StartFadeOut());
    }

    private void Awake()
    {
        _foreground.gameObject.SetActive(true);
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

    public void StartFadeOut()
    {
        if (_fadeOut)
        {
            StartCoroutine(FadeUpProcess());
        }
        else
        {
            _Event_StartNextScene.Invoke();
        }
    }

    IEnumerator FadeIn()
    {
        float perc;
        if (_fadeIn)
        {
            yield return new WaitForSeconds(_fadeInDelays);

            float timer = 0;
            while (_foreground.color.a > 0)
            {
                timer += Time.deltaTime;
                perc = timer / _fadeInTime;
                _foreground.color = _foreground.color.FadeDown(perc);
                yield return null;
            }
            yield return new WaitForSeconds(_fadeInDelays);
            _Event_StartLoading.Invoke();
        }
    }

    private IEnumerator FadeUpProcess()
    {
        float perc;
        float timer = 0;

        yield return new WaitForSeconds(_FadeOutDelays);

        while (_foreground.color.a < 1)
        {
            timer += Time.deltaTime;
            perc = timer / _fadeOutTime;
            _foreground.color = _foreground.color.FadeUp(perc);
            yield return null;
        }
        yield return new WaitForSeconds(_FadeOutDelays);
        _Event_StartNextScene.Invoke();
    }
}
