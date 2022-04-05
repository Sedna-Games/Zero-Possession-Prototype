using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenuSettings : MonoBehaviour
{
    [Header("Audio Settings Sliders")]
    [Tooltip("Value boxes")]
    [SerializeField] GameObject[] _audioSliderValues;
    [Tooltip("Sliders")]
    [SerializeField] GameObject[] _audioSliders;

    [Header("Graphics Settings")]
    [Tooltip("Value boxes")]
    [SerializeField] GameObject[] _graphicsSliderValues;

    [Header("Controls Settings")]
    [Tooltip("Value boxes")]
    [SerializeField] GameObject[] _controlsSliderValues;
    float _sensitivity;
    
    void Start()
    {
        //Sets the initial values of the settings
        //Audio Settings
        InitAudio();

        //Graphics Settings


        //Controls Settings

    }
    void InitAudio() {
        //This runs on Start. Purpose is to set the sliders and values to the proper places
        float[] audioParams = new float[6];
        audioParams[0] = GetParameterValue("speaker_options");
        audioParams[1] = GetParameterValue("master_vol");
        audioParams[2] = GetParameterValue("music_vol");
        audioParams[3] = GetParameterValue("amb_vol");
        audioParams[4] = GetParameterValue("sfx_vol");
        audioParams[5] = GetParameterValue("ui_vol");

        SetSpeakerOptions(audioParams[0]);
        SetMasterVolume(audioParams[1]);
        SetMusicVolume(audioParams[2]);
        SetAmbienceVolume(audioParams[3]);
        SetSFXVolume(audioParams[4]);
        SetUIVolume(audioParams[5]);

        for (int i = 0; i < 6; i++)
        {
            _audioSliders[i].GetComponent<Slider>().value = audioParams[i];
        }
    }

    void Update()
    {
        SetAudioValues();
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

    void SetAudioValues() {
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
        _audioSliderValues[0].GetComponent<TMPro.TMP_Text>().text = _speakerVal;

        _audioSliderValues[1].GetComponent<TMPro.TMP_Text>().text = GetParameterValue("master_vol").ToString("0.##");
        _audioSliderValues[2].GetComponent<TMPro.TMP_Text>().text = GetParameterValue("music_vol").ToString("0.##");
        _audioSliderValues[3].GetComponent<TMPro.TMP_Text>().text = GetParameterValue("amb_vol").ToString("0.##");
        _audioSliderValues[4].GetComponent<TMPro.TMP_Text>().text = GetParameterValue("sfx_vol").ToString("0.##");
        _audioSliderValues[5].GetComponent<TMPro.TMP_Text>().text = GetParameterValue("ui_vol").ToString("0.##");
    }

    void SetGraphicsValues() {

    }

    public void SetLookSensitivityValue(float _sensivity) {
        this._sensitivity = _sensivity;
    }
    void SetControlsValues() {
        _controlsSliderValues[0].GetComponent<TMPro.TMP_Text>().text = _sensitivity.ToString("0.##");
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
