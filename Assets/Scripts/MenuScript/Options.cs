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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetVolume(float value)
    {
        audioMixer.SetFloat("BGMVolume",value);
    }

    public void SetSound(float value)
    {
        audioMixer.SetFloat("Sound", value);
    }
}
