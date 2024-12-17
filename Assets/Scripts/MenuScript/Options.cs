using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OptionMenu : MonoBehaviour
{
    public Slider volumeMusicSlider;
    public Slider volumeFXSlider;
    public AudioMixer audioMixer;
    public AudioMixer audioMixer2;
    public bool fullScreen;
    public Image tick;

    // Start is called before the first frame update
    void Awake()
    {
        audioMixer.SetFloat("MusicVolumen", SaveSettings.Instance.musicVolum);
        audioMixer2.SetFloat("FXVolumen", SaveSettings.Instance.fxVolum);

        volumeMusicSlider.value = SaveSettings.Instance.musicVolum;
        volumeFXSlider.value = SaveSettings.Instance.fxVolum;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetMusicAudio(float musicValue)
    {
        SaveSettings.Instance.musicVolum = musicValue;
        audioMixer.SetFloat("MusicVolumen", musicValue);
    }

    public void SetFXAudio(float FXValue)
    {
        SaveSettings.Instance.fxVolum = FXValue;
        audioMixer2.SetFloat("FXVolumen", FXValue);
    }

    public void SetFullScreen()
    {

        fullScreen = !fullScreen;

        if(fullScreen)
        {
            Screen.fullScreen = true;
            tick.enabled = true; 
        }
        else
        {
            Screen.fullScreen = false;
            tick.enabled = false;
        }
    }
}
