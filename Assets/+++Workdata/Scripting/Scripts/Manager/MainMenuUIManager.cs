using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.EventSystems;

public class MainMenuUIManager : MonoBehaviour
{
    [Header("LoadGameScreen")]
    private GameObject newLoadButton;
    private GameObject newDeleteButton;
    [SerializeField] private List<GameObject> loadButtonsList;
    [SerializeField] private GameObject loadLevelButtonPrefab;
    [SerializeField] private GameObject deleteSaveStateButtonPrefab;
    [SerializeField] private Transform saveStateLayoutGroup;
    [HideInInspector] public bool gameStateLoaded;

    [Header("MainMenuScreens")]
    [SerializeField] private GameObject loadScreen;
    [SerializeField] private GameObject optionsScreen;
    [SerializeField] private GameObject creditsScreen;
    
    [Header("Controls")]
    [SerializeField] private GameObject keyboardControls;
    [SerializeField] private GameObject gamePadControls;
    
    [Header("MainMenuButtons")]
    [SerializeField] private GameObject loadButton;
    [SerializeField] private GameObject optionsButton;
    [SerializeField] private GameObject creditsButton;
    [SerializeField] private GameObject firstMainMenuSelected;

    [Header("ShutterAnim")]
    [SerializeField] private Animator stallShutterAnimator;
    
    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer;

    [SerializeField] private GameObject sunnyBackground;
    
    public static MainMenuUIManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        EventSystem.current.SetSelectedGameObject(firstMainMenuSelected);
        
        loadScreen.SetActive(false);
        optionsScreen.SetActive(false);
        creditsScreen.SetActive(false);
        
        AudioManager.Instance.Play("MainMenuMusic");
        
        if (GameSaveStateManager.Instance.gameGotFinished)
        {
            sunnyBackground.SetActive(true);
        }
    }

    #region Links

    public void LinkedInAlexanderPfeifer()
    {
        Application.OpenURL("https://www.linkedin.com/in/alexander-pfeifer-5b858128b/");
    }
    
    public void LinkedInMartinViegehls()
    {
        Application.OpenURL("https://www.linkedin.com/in/martin-viegehls-41a959279/");
    }

    #endregion

    #region Options

    public void SetControlsImage()
    {
        if (keyboardControls.activeSelf)
        {
            keyboardControls.SetActive(false);
            gamePadControls.SetActive(true);
        }
        else
        {
            keyboardControls.SetActive(true);
            gamePadControls.SetActive(false);
        }
    }
    
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    #endregion
    
    #region MainMenuButtons

    public void OpenOptionsMenu()
    {
        StartCoroutine(SetScreen(false, true, false, true, false, true));
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(loadButton);
    }
    
    public void CreateLoadMenuButtons()
    {
        StartCoroutine(SetScreen(true, false, false, false, true, true));
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstMainMenuSelected);

        if (!gameStateLoaded)
        {
            string[] _saveGameNames = SaveFileManager.GetAllSaveFileNames();
        
            foreach (var _saveGameName in _saveGameNames)
            {
                newLoadButton = Instantiate(loadLevelButtonPrefab, saveStateLayoutGroup);
                loadButtonsList.Add(newLoadButton);
                
                TextMeshProUGUI _buttonText = newLoadButton.GetComponentInChildren<TextMeshProUGUI>();
                _buttonText.text = _saveGameName;
                string _saveStateName = _saveGameName;

                var _loadButton = newLoadButton.GetComponent<Button>();
                _loadButton.onClick.AddListener(PressButtonSound);
                _loadButton.onClick.AddListener(() => LoadGame(_saveStateName));
                
                newDeleteButton = Instantiate(deleteSaveStateButtonPrefab, saveStateLayoutGroup);
                loadButtonsList.Add(newDeleteButton);
                
                var _deleteButton = newDeleteButton.GetComponent<Button>();
                _deleteButton.onClick.AddListener(PressButtonSound);
                _deleteButton.onClick.AddListener(() => DeleteSaveState(_saveStateName));
            }

            gameStateLoaded = true;
        }
    }
    
    public void OpenCreditsMenu()
    {
        StartCoroutine(SetScreen(false, false, true, true, true, false));
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(optionsButton);
    }
    
    public void QuitGame()
    {
        AudioManager.Instance.Stop("MainMenuMusic");
        Application.Quit();
    }

    #endregion
    
    #region SaveStates

    private void DeleteLoadMenuButtons()
    {
        loadButtonsList.ForEach(Destroy);
        gameStateLoaded = false;
    }

    private void DeleteSaveState(string saveName)
    {
        SaveFileManager.DeleteSaveState(saveName);
        DeleteLoadMenuButtons();
        CreateLoadMenuButtons();
    }
    
    private void LoadGame(string saveName)
    {
        EventSystem.current.SetSelectedGameObject(null);
        GameSaveStateManager.Instance.LoadFromSave(saveName);
        AudioManager.Instance.Stop("MainMenuMusic");
        gameStateLoaded = false;
    }

    #endregion
    
    #region Volume

    public void SetVolume(float volume)
    {
        audioMixer.SetFloat("Master", Mathf.Log10(volume) * 20);
    }
    
    public void SetSfxVolume(float volume)
    {
        audioMixer.SetFloat("SFX", Mathf.Log10(volume) * 20);
    }
    
    public void SetMusicVolume(float volume)
    {
        audioMixer.SetFloat("Music", Mathf.Log10(volume) * 20);
    }

    #endregion
    
    public void PressButtonSound()
    {
        InGameUIManager.Instance.PressButtonSound();
    }

    public void SetLoadingScreen()
    {
        InGameUIManager.Instance.loadingScreenAnim.SetTrigger("Start");
    }

    private IEnumerator SetScreen(bool shouldSetLoadScreen, bool shouldSetOptionsScreen, bool shouldSetCreditsScreen, bool loadButtonIsInteractable, bool optionsButtonIsInteractable, bool creditsButtonIsInteractable)
    {
        yield return new WaitForSeconds(0.7f);
        loadScreen.SetActive(shouldSetLoadScreen);
        optionsScreen.SetActive(shouldSetOptionsScreen);
        creditsScreen.SetActive(shouldSetCreditsScreen);
        
        loadButton.GetComponent<Button>().interactable = loadButtonIsInteractable;
        optionsButton.GetComponent<Button>().interactable = optionsButtonIsInteractable;
        creditsButton.GetComponent<Button>().interactable = creditsButtonIsInteractable;
    }

    public void SetAnimation()
    {
        if (stallShutterAnimator.GetBool("GoUp"))
        {
            stallShutterAnimator.SetTrigger("ChangeScreen");
        }

        stallShutterAnimator.SetBool("GoUp", true);
    }
}
