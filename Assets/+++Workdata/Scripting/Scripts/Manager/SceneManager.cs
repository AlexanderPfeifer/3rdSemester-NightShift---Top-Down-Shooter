using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class SceneManager : SingletonPersistent<SceneManager>
{
    private string currentScene;
    
    [Header("LoadingScreen")]
    public Animator loadingScreenAnim;

    [Header("Fullscreen")]
    private const string FullScreenPlayerPrefs = "Fullscreen";
    private int fullScreenInt = 1;

    protected override void Awake()
    {
        base.Awake();
        OnLoad();
    }

    private void Start()
    {
        if(DebugMode.Instance != null)
            loadingScreenAnim.SetTrigger("Start");
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
        GraphicsSettings.transparencySortAxis = new Vector3(0.0f, 1, 0.0f);
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

    public void SetFullscreenPlayerPrefs(GameObject fullScreenCheck)
    {
        fullScreenInt = PlayerPrefs.GetInt(FullScreenPlayerPrefs, fullScreenInt);
        Screen.fullScreen = fullScreenInt == 1;
        fullScreenCheck.SetActive(fullScreenInt == 1);
    }

    public void ChangeFullScreenMode(GameObject fullScreenCheck)
    {
        if (fullScreenInt == 0)
        {
            Screen.fullScreen = true;
            fullScreenInt = 1;
            fullScreenCheck.SetActive(true);
        }
        else
        {
            Screen.fullScreen = false;
            fullScreenInt = 0;
            fullScreenCheck.SetActive(false);
        }

        PlayerPrefs.SetInt(FullScreenPlayerPrefs, fullScreenInt);
    }
}
