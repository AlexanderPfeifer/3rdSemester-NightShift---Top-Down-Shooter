using UnityEngine;
using UnityEngine.Rendering;

public class GameSaveStateManager : MonoBehaviour
{
    public static GameSaveStateManager instance;
    
    public const string MainMenuSceneName = "MainMenu";
    public const string InGameSceneName = "InGame";
    
    public enum GameState
    {
        InMainMenu = 0,
        InGame = 1,
    }
    
    public event System.Action<GameState> OnStateChanged;
    
    public GameState CurrentState { get; private set; } = GameState.InMainMenu;
    
    public SaveGameDataManager saveGameDataManager = new SaveGameDataManager();
    
    private void Awake()
    {
        instance = this;
        OnLoad();
    }

    [RuntimeInitializeOnLoadMethod]
    static void OnLoad()
    {
        GraphicsSettings.transparencySortMode = TransparencySortMode.CustomAxis;
        GraphicsSettings.transparencySortAxis = new Vector3(0.0f, 1.0f, 0.0f);
    }

    
    private void Start()
    {
        GoToMainMenu();
    }
    
    public void GoToMainMenu()
    {
        CurrentState = GameState.InMainMenu;
        if (OnStateChanged != null)
            OnStateChanged(CurrentState);
        SceneManager.instance.SwitchScene(MainMenuSceneName);
    }
    
    public void StartNewGame(string gameName)
    {
        saveGameDataManager = new SaveGameDataManager();
        saveGameDataManager.saveName = gameName;
            CurrentState = GameState.InGame;
        if (OnStateChanged != null)
            OnStateChanged(CurrentState);
        SceneManager.instance.SwitchScene(saveGameDataManager.loadedSceneName);
    }

    public void LoadFromSave(string saveName)
    {
        //first, we try to load the game from via the save manager.
        if (!SaveFileManager.TryLoadData<SaveGameDataManager>(saveName, out var loadedData))
            return; //if we cannot load the save, we cancel the action.
        
        //after we successfuly loaded the save, we set the data correctly. 
        saveGameDataManager = loadedData;
        
        //we set the correct state (as we want to enter the inGame-State) and give out the callback
        CurrentState = GameState.InGame;
        if (OnStateChanged != null)
            OnStateChanged(CurrentState);
        
        //we load the scene we last saved the game in, as it is set within the data.
        SceneManager.instance.SwitchScene(saveGameDataManager.loadedSceneName);
    }
    
    /// <summary>
    /// We call this method to save the current game state.
    /// </summary>
    public void SaveGame()
    {
        if (CurrentState == GameState.InMainMenu)
            return; //if we are in the main menu, we dont save anything
        
        //we give the current data and the wanted save name to the SaveManger
        SaveFileManager.TrySaveData(saveGameDataManager.saveName, saveGameDataManager);
    }
}
