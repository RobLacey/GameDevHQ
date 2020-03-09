using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreController : MonoBehaviour
{
    [SerializeField] int _score = 0;
    [SerializeField] Text _scoreText = default;
    [SerializeField] Text _waveWipeText = default;
    [SerializeField] EventManager _Event_AddToScore = default;
    [SerializeField] EventManager _Event_WaveWiped = default;
    [SerializeField] bool _waveWipeBonusOn = false;
    [SerializeField] int _waveWipeBonus = 1;
    [SerializeField] int _scorePad = 6;
    [SerializeField] int _waveWipePad = 2;

    char _pad = '0';
    public bool WaveWipe { set { _waveWipeBonusOn = value; } }

    private void OnEnable()
    {
        _Event_AddToScore.AddListener(x => AddToScore(x));
        _Event_WaveWiped.AddListener((x, y) => WaveWipeBonusScore(x, y));
    }

    private void Start()
    {
        AddToScore(0);
    }

    public void AddToScore(object points)
    {
        int pointsToAdd = (int)points;
        _score += pointsToAdd * _waveWipeBonus;
        _waveWipeText.text = _waveWipeBonus.ToString().PadLeft(_waveWipePad, _pad); ;
        _scoreText.text = _score.ToString().PadLeft(_scorePad, _pad);
    }

    private void WaveWipeBonusScore(object score, object activate)
    {
        bool active = (bool)activate;
        if (_waveWipeBonusOn && active)
        {
            _waveWipeBonus = _waveWipeBonus + 2;
        }
        else if(_waveWipeBonusOn && !active)
        {
            _waveWipeBonusOn = false;
            _waveWipeBonus = 1;
        }
        else if (!_waveWipeBonusOn && active)
        {
            _waveWipeBonusOn = true;
            _waveWipeBonus = 2;
        }
        AddToScore((int) score);
    }


}
