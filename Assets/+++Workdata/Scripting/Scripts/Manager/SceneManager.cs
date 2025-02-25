using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class SceneManager : MonoBehaviour
{
    public static SceneManager Instance;
    private string currentScene;

    private void Awake()
    {
        Instance = this;
        OnLoad();
    }

    public void SwitchScene(string newScene)
    {
        StartCoroutine(LoadNewSceneCoroutine(newScene));
    }
    
    //Change the transparency sort mode, because I want higher Sprites always to be rendered in front of lowers
    [RuntimeInitializeOnLoadMethod]
    private static void OnLoad()
    {
        GraphicsSettings.transparencySortMode = TransparencySortMode.CustomAxis;
        GraphicsSettings.transparencySortAxis = new Vector3(0.0f, 1.0f, 0.0f);
    }

    private IEnumerator LoadNewSceneCoroutine(string newSceneName)
    {
        var _scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(currentScene);
        if (_scene.isLoaded)
        {
            yield return UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(currentScene);
        }
        
        Scene _newScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(newSceneName);
        if (!_newScene.isLoaded)
        {
            yield return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(newSceneName, LoadSceneMode.Additive);
        }
        
        yield return null;
        _newScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(newSceneName);
        UnityEngine.SceneManagement.SceneManager.SetActiveScene(_newScene);
        
        currentScene = newSceneName;
    }
}
