using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public bool isEnter = false;
    public string nextSceneName;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            LoadNextLevel();
        }
    }

    public void LoadNextLevel()
    {
        if (!string.IsNullOrEmpty(nextSceneName) && isEnter)
        {
            SceneManager.LoadScene(nextSceneName);
        }
        
    }
}
