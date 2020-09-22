using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerSettings : MonoBehaviour
{
    [SerializeField] Slider _musicSlider;
    [SerializeField] Slider _sfxSlider;
    //[SerializeField] UINode _hardLevel;
    [SerializeField] Settings _playerSettings;
    [SerializeField] EventManager _Event_Save_File;
    [SerializeField] EventManager _Event_Load_File;
    [SerializeField] EventManager _Event_Check_For_File;
    [SerializeField] EventManager _Event_SetMusicVolume;
    [SerializeField] EventManager _Event_SetSfxVolume; // TODO Not attcahed to anything

    [Serializable]
    public class Settings
    {
        public SaveFileNames _saveFileNames;
        public float _musicVolume = 0.8f;
        public float _sfxVolume = 0.8f;
        public bool _hardLevel = false;
    }

    private void Start()
    {
        bool newInstall = (bool)_Event_Check_For_File.ReturnParameter(_playerSettings._saveFileNames, this);
        if (!newInstall)
        {
            _Event_Save_File.Invoke(_playerSettings,_playerSettings._saveFileNames, this);
            Debug.Log("New Install : Set Up Player Stats");
        }
        _playerSettings = (Settings) _Event_Load_File.ReturnParameter(_playerSettings._saveFileNames, this);

        SetUpHardLevelSettings();
        SetUpMusicSettings();
        SetUpSFXSettings();
    }

    private void SetUpMusicSettings()
    {
        _Event_SetMusicVolume.Invoke(_playerSettings._musicVolume, this);

        if (_musicSlider != null)
        {
            _musicSlider.value = _playerSettings._musicVolume;
        }
    }

    private void SetUpSFXSettings()
    {
        _Event_SetSfxVolume.Invoke(_playerSettings._sfxVolume, this);

        if (_sfxSlider != null)
        {
            _sfxSlider.value = _playerSettings._sfxVolume;
        }
    }

    private void SetUpHardLevelSettings() //TODO Review what is happening here
    {
        // if (_hardLevel != null)
        // {
        //     _hardLevel.IsSelected = _playerSettings._hardLevel;
        // }
    }

    public void AdjustMusicLevel(float newValue)
    {
        _playerSettings._musicVolume = newValue;
        _musicSlider.value = newValue;
        _Event_SetMusicVolume.Invoke(_playerSettings._musicVolume, this);
    }

    public void AdjustSFXLevel(float newValue)
    {
        _playerSettings._sfxVolume = newValue;
        _sfxSlider.value = newValue;
        _Event_SetSfxVolume.Invoke(_playerSettings._sfxVolume, this);
    }

    public void AdjustHardLevel(bool newLevel)
    {
        //_hardLevel.IsSelected = newLevel;
        _playerSettings._hardLevel = newLevel;
    }

    public void SaveSettings()
    {
        _Event_Save_File.Invoke(_playerSettings, _playerSettings._saveFileNames, this);
    }

    public void Selected()
    {
        Debug.Log("Selected");
    }
}
