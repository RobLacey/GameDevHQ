using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSettings : MonoBehaviour
{
    [SerializeField] Toggle _hardLevelToggle;
    [SerializeField] Slider _musicVolumeSlider;
    [SerializeField] Slider _sFXVolumeSlider;
    [SerializeField] EventManager _Event_SetMusicVolume;
    [SerializeField] EventManager _Event_SetSfxVolume;

    public const string _musicSettings = "MUSIC SETTINGS";
    public const string _SFXSettings = "SFX SETTINGS";
    public const string _hardLevel = "HARD LEVEL";

    private void Awake()
    {
        //PlayerPrefs.DeleteAll();
        SetUpHardLevel();
        SetUpMusicSettings();
        SetUpSFXSettings();
    }

    private void SetUpHardLevel()
    {
        if (!PlayerPrefs.HasKey(_hardLevel))
        {
            PlayerPrefs.SetInt(_hardLevel, 0);
            _hardLevelToggle.isOn = false;
            PlayerPrefs.Save();
        }
        else
        {
            if (PlayerPrefs.GetInt(_hardLevel) == 1)
            {
                _hardLevelToggle.isOn = true;
            }
            else
            {
                _hardLevelToggle.isOn = false;
            }
        }
    }
    private void SetUpMusicSettings()
    {
        if (!PlayerPrefs.HasKey(_musicSettings))
        {
            _musicVolumeSlider.value = 1f;
            SetMusicSlider();
        }
        else
        {
            _musicVolumeSlider.value = PlayerPrefs.GetFloat(_musicSettings);
            _Event_SetMusicVolume.Invoke(PlayerPrefs.GetFloat(_musicSettings));
        }
    }

    private void SetUpSFXSettings()
    {
        if (!PlayerPrefs.HasKey(_SFXSettings))
        {
            _sFXVolumeSlider.value = 1f;
            SetSFXSlider();
        }
        else
        {
            _sFXVolumeSlider.value = PlayerPrefs.GetFloat(_SFXSettings);
            _Event_SetSfxVolume.Invoke(PlayerPrefs.GetFloat(_SFXSettings));
        }
    }


    public void ToggleHardLevel()
    {
        if (_hardLevelToggle.isOn == true)
        {
            PlayerPrefs.SetInt(_hardLevel, 1);
        }
        else
        {
            PlayerPrefs.SetInt(_hardLevel, 0);
        }
        PlayerPrefs.Save();
        //Debug.Log(PlayerPrefs.GetInt(_hardLevel));
    }

    public void SetSFXSlider()
    {
        PlayerPrefs.SetFloat(_SFXSettings, _sFXVolumeSlider.value);
        _Event_SetSfxVolume.Invoke(PlayerPrefs.GetFloat(_SFXSettings));
        PlayerPrefs.Save();
        //Debug.Log(PlayerPrefs.GetFloat(_SFXSettings));
    }

    public void SetMusicSlider()
    {
        PlayerPrefs.SetFloat(_musicSettings, _musicVolumeSlider.value);
        _Event_SetMusicVolume.Invoke(PlayerPrefs.GetFloat(_musicSettings));
        PlayerPrefs.Save();
        //Debug.Log(PlayerPrefs.GetFloat(_musicSettings));
    }
}
