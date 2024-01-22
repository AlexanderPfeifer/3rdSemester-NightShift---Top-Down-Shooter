using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManager : MonoBehaviour
{
    public static SceneManager Instance;
    private string currentScene;

    private void Awake()
    {
        Instance = this;
    }

    public void SwitchScene(string newScene)
    {
        StartCoroutine(LoadNewSceneCoroutine(newScene));
    }

    //Switches scene and loads a new scene 
    private IEnumerator LoadNewSceneCoroutine(string newSceneName)
    {
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
        
        currentScene = newSceneName;
    }
}
