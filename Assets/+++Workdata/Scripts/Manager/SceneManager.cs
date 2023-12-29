using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManager : MonoBehaviour
{
    public static SceneManager instance;
    private string currentScene;
    [SerializeField] private GameObject loadingScreen;


    private void Awake()
    {
        instance = this;
        loadingScreen.SetActive(false);
    }

    public void SwitchScene(string newScene)
    {
        StartCoroutine(LoadNewSceneCoroutine(newScene));
    }

    private IEnumerator LoadNewSceneCoroutine(string newSceneName)
    {
        Time.timeScale = 1f;
        
        loadingScreen.SetActive(true);
        
        var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(currentScene);
        if (scene.isLoaded)
        {
            yield return UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(currentScene);
        }
        
        Scene newScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(newSceneName);
        if (!newScene.isLoaded)
        {
            yield return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(newSceneName, LoadSceneMode.Additive);
        }
        
        yield return null;
        newScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(newSceneName);
        UnityEngine.SceneManagement.SceneManager.SetActiveScene(newScene);
        
        loadingScreen.SetActive(false);
        currentScene = newSceneName;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
