using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

public class GameSaveStateManager : MonoBehaviour
{
    public static GameSaveStateManager Instance;
    
    private const string MainMenuSceneName = "MainMenu";
    public const string InGameSceneName = "InGame";

    public bool startedNewGame;
    private bool changeAlpha;
    public TextMeshProUGUI gameSavingText; 
    [SerializeField] private float textAlphaChangeSpeed = 1;
    public bool gameGotFinished;

    private enum GameState
    {
        InMainMenu = 0,
        InGame = 1,
    }
    
    private GameState CurrentState { get; set; } = GameState.InMainMenu;
    
    public SaveGameDataManager saveGameDataManager = new SaveGameDataManager();
    
    private void Awake()
    {
        Instance = this;
        OnLoad();
    }

    //changes the transparency sort mode of the sprites
    [RuntimeInitializeOnLoadMethod]
    private static void OnLoad()
    {
        GraphicsSettings.transparencySortMode = TransparencySortMode.CustomAxis;
        GraphicsSettings.transparencySortAxis = new Vector3(0.0f, 1.0f, 0.0f);
    }

    //Starts the game in the main menu
    private void Start()
    {
        GoToMainMenu();
    }

    private void Update()
    {
        ChangeAlpha();
    }

    //goes to main menu
    public void GoToMainMenu()
    {
        InGameUI.Instance.inGameUIScreen.SetActive(false);
        CurrentState = GameState.InMainMenu;
        SceneManager.Instance.SwitchScene(MainMenuSceneName);
    }
    
    //Starts a fresh new game
    public void StartNewGame(string gameName)
    {
        EventSystem.current.SetSelectedGameObject(null);
        startedNewGame = true;
        saveGameDataManager = new SaveGameDataManager();
        saveGameDataManager.saveName = gameName;
            CurrentState = GameState.InGame;
            AudioManager.Instance.Stop("MainMenuMusic");
        SceneManager.Instance.SwitchScene(saveGameDataManager.loadedSceneName);
    }

    //Loads game from a specific game by string
    public void LoadFromSave(string saveName)
    {
        //first, we try to load the game from via the save manager.
        if (!SaveFileManager.TryLoadData<SaveGameDataManager>(saveName, out var loadedData))
            return; //if we cannot load the save, we cancel the action.
        
        //after we successfully loaded the save, we set the data correctly. 
        saveGameDataManager = loadedData;
        
        //we set the correct state (as we want to enter the inGame-State) and give out the callback
        CurrentState = GameState.InGame;

        //we load the scene we last saved the game in, as it is set within the data.
        SceneManager.Instance.SwitchScene(saveGameDataManager.loadedSceneName);
    }
    
    /// We call this method to save the current game state.
    public void SaveGame()
    {
        if (CurrentState == GameState.InMainMenu)
            return; //if we are in the main menu, we dont save anything
        
        //we give the current data and the wanted save name to the SaveManger
        SaveFileManager.TrySaveData(saveGameDataManager.saveName, saveGameDataManager);

        StartCoroutine(SetSaveGameText());
    }

    //Sets save game text when game saves
    private IEnumerator SetSaveGameText()
    {
        gameSavingText.gameObject.SetActive(true);
        changeAlpha = true;
        yield return new WaitForSeconds(3);
        gameSavingText.gameObject.SetActive(false);
        changeAlpha = false;
    }

    //Changes Alpha of the game save text
    private void ChangeAlpha()
    {
        if (changeAlpha)
        {
            gameSavingText.color = new Color(gameSavingText.color.r, gameSavingText.color.g, gameSavingText.color.b, Mathf.PingPong(textAlphaChangeSpeed * Time.time, 1));
        }
    }
}
