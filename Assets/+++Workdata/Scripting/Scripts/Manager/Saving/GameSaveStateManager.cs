using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameSaveStateManager : SingletonPersistent<GameSaveStateManager>
{
    private const string MainMenuSceneName = "MainMenu";
    public const string InGameSceneName = "InGame";

    [Header("GameSaveText")]
    [SerializeField] private float textAlphaChangeSpeed = 1;
    private bool changeAlpha;
    
    [HideInInspector] public bool gameGotFinished;

    private enum GameState
    {
        InMainMenu = 0,
        InGame = 1,
    }
    
    private GameState CurrentState { get; set; } = GameState.InMainMenu;
    
    public SaveGameDataManager saveGameDataManager = new();

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
        CurrentState = GameState.InMainMenu;
        SceneManager.Instance.SwitchScene(MainMenuSceneName);
    }
    
    public void StartNewGame(string gameName)
    {
        EventSystem.current.SetSelectedGameObject(null);
        saveGameDataManager = new SaveGameDataManager
        {
            saveName = gameName
        };
        
        CurrentState = GameState.InGame;
        AudioManager.Instance.FadeOut("MainMenuMusic", "");
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

        //StartCoroutine(SetSaveGameText());
    }

    #region SetSaveGameText

    private IEnumerator SetSaveGameText()
    {
        InGameUIManager.Instance.gameSavingText.gameObject.SetActive(true);
        changeAlpha = true;
        yield return new WaitForSeconds(3);
        InGameUIManager.Instance.gameSavingText.gameObject.SetActive(false);
        changeAlpha = false;
    }

    private void ChangeAlpha()
    {
        if (changeAlpha)
        {
            InGameUIManager.Instance.gameSavingText.color = new Color(InGameUIManager.Instance.gameSavingText.color.r, 
                InGameUIManager.Instance.gameSavingText.color.g, InGameUIManager.Instance.gameSavingText.color.b, 
                Mathf.PingPong(textAlphaChangeSpeed * Time.time, 1));
        }
    }

    #endregion
}
