using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreController : MonoBehaviour
{
    [SerializeField] int _score = 0;
    [SerializeField] Text _scoreText = default;
    [SerializeField] Text _waveWipeText = default;
    [SerializeField] Text _waveWipeTitle = default;
    [SerializeField] Color _colorOn;
    [SerializeField] Color _colorOff;
    [SerializeField] int _flashColor = 3;
    [SerializeField] float _flashingPeriod = 0.5f;
    [SerializeField] bool _waveWipeBonusOn = false;
    [SerializeField] int _waveWipeBonus = 1;
    [SerializeField] int _scorePad = 6;
    [SerializeField] int _waveWipePad = 2;
    [SerializeField] AudioClip _waveWipeResetSFX = default;
    [SerializeField] float _volume = 0.8f;
    [SerializeField] EventManager _Event_AddToScore = default;
    [SerializeField] EventManager _Event_WaveWiped = default;
    [SerializeField] EventManager _Event_GetTarget;
    [SerializeField] EventManager _Event_AddHighScore;
    [SerializeField] EventManager _Event_PlayerDead;

    //Variable
    char _pad = '0';
    Camera _camera;

    public bool WaveWipe { set { _waveWipeBonusOn = value; } }

    public RectTransform ReturnPosition { get { return _waveWipeText.GetComponent<RectTransform>(); } }

    private void Awake()
    {
        _camera = Camera.main;
    }

    private void OnEnable()
    {
        _Event_AddToScore.AddListener(x => AddToScore(x),this);
        _Event_WaveWiped.AddListener((x, y) => WaveWipeBonusScore(x, y), this);
        _Event_GetTarget.AddReturnParameter(() => ReturnPosition, this);
        _Event_PlayerDead.AddListener(() => _Event_AddHighScore.Invoke(_score, this), this);
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
            StartCoroutine(FlashColour());
            AudioSource.PlayClipAtPoint(_waveWipeResetSFX, _camera.transform.position, _volume);
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

    IEnumerator FlashColour()
    {
        for (int count = 0; count < _flashColor; count++)
        {
            yield return StartCoroutine(FlickerTimer());
        }
        yield return null;
    }

    IEnumerator FlickerTimer()
    {
        _waveWipeText.color = _colorOn;
        _waveWipeTitle.color = _colorOn;
        yield return new WaitForSeconds(_flashingPeriod);
        _waveWipeText.color = _colorOff;
        _waveWipeTitle.color = _colorOff;
        yield return new WaitForSeconds(_flashingPeriod);
    }

}
