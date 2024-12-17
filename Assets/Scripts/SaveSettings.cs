using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveSettings : MonoBehaviour
{
    #region Singleton
    private static SaveSettings _instance;
    public static SaveSettings Instance
    {
        get
        {
            // If the instance is null, find it in the scene
            if (_instance == null)
            {
                _instance = Object.FindFirstObjectByType<SaveSettings>();
                if (_instance == null)
                {
                    Debug.LogError("Save settings Missed");
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        // Check if instance already exists
        if (_instance != null)
        {
            Destroy(gameObject); // Destroy this instance if it already exists
        }
        else
        {
            _instance = this; // Set the instance to this
        }
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    public float musicVolum = 10;
    public float fxVolum = 10;
}
