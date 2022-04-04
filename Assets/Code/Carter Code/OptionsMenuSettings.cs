using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenuSettings : MonoBehaviour
{
    [Header("Sliders")]
    [Tooltip("Value boxes")]
    [SerializeField] GameObject[] _sliderValues;
    
    void Start()
    {
        //Sets the initial values of the settings
        SetSpeakerOptions(GetParameterValue("speaker_options"));
        SetMasterVolume(GetParameterValue("master_vol"));
        SetMusicVolume(GetParameterValue("music_vol"));
        SetAmbienceVolume(GetParameterValue("amb_vol"));
        SetSFXVolume(GetParameterValue("sfx_vol"));
        SetUIVolume(GetParameterValue("ui_vol"));
    }

    void Update()
    {
        SetValues();
    }

    public void SetSpeakerOptions(float _speakerOption) {
        SetParameterValue("speaker_options", _speakerOption);
        //Debug.Log("Speaker Options: " + GetParameterValue("speaker_options"));
    }

    public void SetMasterVolume(float _masterVol) {
        SetParameterValue("master_vol", _masterVol);
        //Debug.Log("New Master Volume: " + GetParameterValue("master_vol"));
    }

    public void SetMusicVolume(float _musicVol) {
        SetParameterValue("music_vol", _musicVol);
        //Debug.Log("New Music Volume: " + GetParameterValue("music_vol"));
    }

    public void SetAmbienceVolume(float _ambVol) {
        SetParameterValue("amb_vol", _ambVol);
        //Debug.Log("New Ambience Volume: " + GetParameterValue("amb_vol"));
    }

    public void SetSFXVolume(float _sfxVol) {
        SetParameterValue("sfx_vol", _sfxVol);
        //Debug.Log("New SFX Volume: " + GetParameterValue("sfx_vol"));
    }

    public void SetUIVolume(float _uiVol) {
        SetParameterValue("ui_vol", _uiVol);
        //Debug.Log("New UI Volume: " + GetParameterValue("ui_vol"));
    }

    void SetValues() {
        //set the values of the text fields as the values of the parameters
        
        float _speakerOption = GetParameterValue("speaker_options");
        string _speakerVal = "";
        switch (_speakerOption) {
            case 0: 
                _speakerVal = "Mono";
                break;
            case 1: 
                _speakerVal = "Stereo";
                break;
            case 2:
                _speakerVal = "5.1 Surround";
                break;
        }
        _sliderValues[0].GetComponent<TMPro.TMP_Text>().text = _speakerVal;

        _sliderValues[1].GetComponent<TMPro.TMP_Text>().text = GetParameterValue("master_vol").ToString("0.##");
        _sliderValues[2].GetComponent<TMPro.TMP_Text>().text = GetParameterValue("music_vol").ToString("0.##");
        _sliderValues[3].GetComponent<TMPro.TMP_Text>().text = GetParameterValue("amb_vol").ToString("0.##");
        _sliderValues[4].GetComponent<TMPro.TMP_Text>().text = GetParameterValue("sfx_vol").ToString("0.##");
        _sliderValues[5].GetComponent<TMPro.TMP_Text>().text = GetParameterValue("ui_vol").ToString("0.##");
    }

    void SetParameterValue(string paramName, float paramValue) {
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName(paramName, paramValue);
    }
    float GetParameterValue(string paramName) {
        float paramValue;
        FMODUnity.RuntimeManager.StudioSystem.getParameterByName(paramName, out paramValue);

        return paramValue;
    }
}
