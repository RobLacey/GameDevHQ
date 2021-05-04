using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartCountDown : MonoBehaviour
{
    [SerializeField] EventManager _Event_StartSpawning;
    [SerializeField] EventManager _Event_StartLevel;

    Text _startCountDownText = default;
    Canvas _canvas;

    private void Awake()
    {
        _startCountDownText = GetComponentInChildren<Text>();
        _canvas = GetComponentInChildren<Canvas>();
    }
    private void OnEnable()
    {
        _Event_StartLevel.AddListener(() => StartLevel(), this);
    }

    private void Start()
    {
        _canvas.enabled = false;
    }

    private void StartLevel()
    {
        StartCoroutine(Countdown());
    }

    private IEnumerator Countdown()
    {
        _canvas.enabled = true;
        int index = 3;
        _startCountDownText.text = index--.ToString();
        yield return new WaitForSeconds(1);
        _startCountDownText.text = index--.ToString();
        yield return new WaitForSeconds(1);
        _startCountDownText.text = index--.ToString();
        yield return new WaitForSeconds(1);
        _startCountDownText.text = "GO";
        yield return new WaitForSeconds(1);
        _canvas.enabled = false;
        _Event_StartSpawning.Invoke(this);
    }

}
