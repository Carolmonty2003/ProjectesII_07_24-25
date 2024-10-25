using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Pause : MonoBehaviour
{
    public GameObject Pausemenu;
    public bool isStop = true;
    public AudioSource Music;//For Stop Music when use Pause Menu
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (isStop)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Pausemenu.SetActive(true);
                isStop = false;
                Time.timeScale = (0);//Stop the Game
                Music.Pause();//Stop the Music
            }
        } else if (Input.GetKeyDown(KeyCode.Escape))
        {
            Pausemenu.SetActive(false);
            isStop = true;
            Time.timeScale = (1);//Continue the Game
            Music.Play();//Start the Music
        }

    }

    public void Resume()
    {
        Pausemenu.SetActive(false);
        isStop = true;
        Time.timeScale = (1);//Continue the Game
        Music.Play();//Start the Music
    }

    public void Restart()
    {
        SceneManager.LoadScene(0);
        Time.timeScale = (1);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
