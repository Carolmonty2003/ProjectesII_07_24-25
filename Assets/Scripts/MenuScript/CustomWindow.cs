using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CustomWindow : MonoBehaviour
{
    Dropdown dropdown;
    // Start is called before the first frame update
    private void Awake()
    {
        if (null == dropdown)
        {
            dropdown = GetComponent<Dropdown>();
        }
        dropdown.value = 2;
    }

    public void SetResolutionRatio()
    {
        switch (dropdown.value)
        {
            case 0: Screen.SetResolution(640, 480, true); break;
            case 1: Screen.SetResolution(1280, 720, true); break;
            case 2: Screen.SetResolution(1920, 1080, true); break;
        }
    }
}

 
