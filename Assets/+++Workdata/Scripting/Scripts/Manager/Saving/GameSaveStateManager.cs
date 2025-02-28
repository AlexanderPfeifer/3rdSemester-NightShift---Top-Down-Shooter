using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameSaveStateManager : MonoBehaviour
{
    public static GameSaveStateManager Instance;
    
    private const string MainMenuSceneName = "MainMenu";
    public const string InGameSceneName = "InGame";

    [Header("GameSaveText")]
    [SerializeField] private TextMeshProUGUI gameSavingText; 
    [SerializeField] private float textAlphaChangeSpeed = 1;
    private bool changeAlpha;
    
    [HideInInspector] public bool startedNewGame;
    [HideInInspector] public bool gameGotFinished;

    private enum GameState
    {
        InMainMenu = 0,
        InGame = 1,
    }
    
    private GameState CurrentState { get; set; } = GameState.InMainMenu;
    
    public SaveGameDataManager saveGameDataManager = new();
    
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        GoToMainMenu();
    }

    private void Update()
    {
        ChangeAlpha();
    }

    public void GoToMainMenu()
    {
        InGameUIManager.Instance.inGameUIScreen.SetActive(false);
        CurrentState = GameState.InMainMenu;
        SceneManager.Instance.SwitchScene(MainMenuSceneName);
    }
    
    public void StartNewGame(string gameName)
    {
        EventSystem.current.SetSelectedGameObject(null);
        startedNewGame = true;
        saveGameDataManager = new SaveGameDataManager
        {
            saveName = gameName
        };
        
        CurrentState = GameState.InGame;
        AudioManager.Instance.Stop("MainMenuMusic");
        SceneManager.Instance.SwitchScene(saveGameDataManager.loadedSceneName);
    }

    public void LoadFromSave(string saveName)
    {
        if (!SaveFileManager.TryLoadData<SaveGameDataManager>(saveName, out var _loadedData))
            return; 
        
        saveGameDataManager = _loadedData;
        
        CurrentState = GameState.InGame;

        SceneManager.Instance.SwitchScene(saveGameDataManager.loadedSceneName);
    }
    
    public void SaveGame()
    {
        if (CurrentState == GameState.InMainMenu)
            return; 
        
        SaveFileManager.TrySaveData(saveGameDataManager.saveName, saveGameDataManager);

        StartCoroutine(SetSaveGameText());
    }

    #region SetSaveGameText

    private IEnumerator SetSaveGameText()
    {
        gameSavingText.gameObject.SetActive(true);
        changeAlpha = true;
        yield return new WaitForSeconds(3);
        gameSavingText.gameObject.SetActive(false);
        changeAlpha = false;
    }

    private void ChangeAlpha()
    {
        if (changeAlpha)
        {
            gameSavingText.color = new Color(gameSavingText.color.r, gameSavingText.color.g, gameSavingText.color.b, Mathf.PingPong(textAlphaChangeSpeed * Time.time, 1));
        }
    }

    #endregion
}
