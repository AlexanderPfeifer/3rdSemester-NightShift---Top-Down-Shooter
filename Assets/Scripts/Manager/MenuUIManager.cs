using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuUIManager : MonoBehaviour
{
    public static bool GameIsPaused;
    [SerializeField] private GameObject pauseScreen;

    private const string MainMenu = "MainMenu";
    private const string InGame = "InGame";

    public void PauseGame()
    {
        if (GameIsPaused)
        {
            GameIsPaused = false;
            Time.timeScale = 1;
            pauseScreen.SetActive(false);
        }
        else
        {
            GameIsPaused = true;
            Time.timeScale = 0;
            pauseScreen.SetActive(true);
        }
    }

    public void LoadMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(MainMenu);
    }

    public void LoadGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(InGame);
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
}
