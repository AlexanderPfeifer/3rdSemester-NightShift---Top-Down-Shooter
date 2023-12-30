using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManager : MonoBehaviour
{
    public static SceneManager instance;
    private string currentScene;

    private void Awake()
    {
        instance = this;
    }

    public void SwitchScene(string newScene)
    {
        StartCoroutine(LoadNewSceneCoroutine(newScene));
    }

    private IEnumerator LoadNewSceneCoroutine(string newSceneName)
    {
        UIManager.instance.ToggleLoadingScreen(true);
        
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
        
        UIManager.instance.ToggleLoadingScreen(false);
        currentScene = newSceneName;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
