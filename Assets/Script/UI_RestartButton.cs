using UnityEngine;
using UnityEngine.SceneManagement;

public class GameRestartHandler : MonoBehaviour
{
    // This static bool stays true even after the scene reloads
    public static bool WasRestarted = false;

    public void ClickRestart()
    {
        Time.timeScale = 1f;

        // SET THE FLAG HERE
        WasRestarted = true;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}