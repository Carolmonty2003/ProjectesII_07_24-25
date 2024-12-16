using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OptionMenu : MonoBehaviour
{
    public Slider volumeSlider;
    public AudioMixer audioMixer;
    public AudioMixer audioMixer2;
    public bool fullScreen;
    public Image tick;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetMusicAudio(float muiscValue)
    {
        audioMixer.SetFloat("MusicVolumen", muiscValue);
    }

    public void SetFXAudio(float FXValue)
    {
        audioMixer.SetFloat("FXVolumen", FXValue);
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
