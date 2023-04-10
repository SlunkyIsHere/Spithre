using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OptionsScreen : MonoBehaviour
{
    [SerializeField] private Toggle fullscreenTog, vsyncTog;

    public List<ResItem> Resolutions = new List<ResItem>();
    private int _selectedResoultion;

    public TMP_Text resolutionLabel;

    public AudioMixer theMixer;

    public TMP_Text masterLabel, musicLabel, SFXLabel;

    public Slider masterSlider, musicSlider, SFXSlider;

    void Start()
    {
        fullscreenTog.isOn = Screen.fullScreen;

        vsyncTog.isOn = QualitySettings.vSyncCount != 0;

        bool foundRes = false;
        for (int i = 0; i < Resolutions.Count; i++)
        {
            if (Screen.width == Resolutions[i].horizontal && Screen.height == Resolutions[i].vertical)
            {
                foundRes = true;

                _selectedResoultion = i;
                
                UpdateResLabel();
            }
        }

        if (!foundRes)
        {
            ResItem newRes = new ResItem();
            newRes.horizontal = Screen.width;
            newRes.vertical = Screen.height;
            
            Resolutions.Add(newRes);
            _selectedResoultion = Resolutions.Count - 1;
            UpdateResLabel();
        }

        float vol = 0f;
        theMixer.GetFloat("MasterVol", out vol);
        masterSlider.value = vol;
        theMixer.GetFloat("MusicVol", out vol);
        musicSlider.value = vol;
        theMixer.GetFloat("SFXVol", out vol);
        SFXSlider.value = vol;
        
        masterLabel.text = Mathf.RoundToInt(masterSlider.value + 80).ToString();
        musicLabel.text = Mathf.RoundToInt(musicSlider.value + 80).ToString();
        SFXLabel.text = Mathf.RoundToInt(SFXSlider.value + 80).ToString();
    }

    void Update()
    {
        
    }

    public void ResLeft()
    {
        _selectedResoultion--;
        if (_selectedResoultion < 0)
        {
            _selectedResoultion = 0;
        }
        
        UpdateResLabel();
    }

    public void ResRight()
    {
        _selectedResoultion++;
        if (_selectedResoultion > Resolutions.Count - 1)
        {
            _selectedResoultion = Resolutions.Count - 1;
        }
        
        UpdateResLabel();
    }

    public void UpdateResLabel()
    {
        resolutionLabel.text = Resolutions[_selectedResoultion].horizontal + " X " + Resolutions[_selectedResoultion].vertical;
    }

    public void ApplyGraphics()
    {
        if (vsyncTog.isOn)
        {
            QualitySettings.vSyncCount = 1;
        }
        else
        {
            QualitySettings.vSyncCount = 0;
        }
        
        Screen.SetResolution(Resolutions[_selectedResoultion].horizontal,Resolutions[_selectedResoultion].vertical, fullscreenTog.isOn);
    }

    public void SetMasterVolume()
    {
        masterLabel.text = Mathf.RoundToInt(masterSlider.value + 80).ToString();

        theMixer.SetFloat("MasterVol", masterSlider.value);
        
        PlayerPrefs.SetFloat("MasterVol", masterSlider.value);
    }
    
    public void SetMusicVolume()
    {
        musicLabel.text = Mathf.RoundToInt(musicSlider.value + 80).ToString();

        theMixer.SetFloat("MusicVol", musicSlider.value);
        
        PlayerPrefs.SetFloat("MusicVol", musicSlider.value);
    }
    
    public void SetSFXVolume()
    {
        SFXLabel.text = Mathf.RoundToInt(SFXSlider.value + 80).ToString();

        theMixer.SetFloat("SFXVol", SFXSlider.value);
        
        PlayerPrefs.SetFloat("SFXVol", SFXSlider.value);
    }
}

[System.Serializable]
public class ResItem
{
    public int horizontal, vertical;
}
