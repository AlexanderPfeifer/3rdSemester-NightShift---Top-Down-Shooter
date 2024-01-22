using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.EventSystems;

public class MainMenuUI : MonoBehaviour
{
    [Header("LoadGameComponents")]
    private GameObject newLoadButton;
    private GameObject newDeleteButton;
    [SerializeField] private List<GameObject> loadButtonsList;
    [SerializeField] private GameObject loadLevelButtonPrefab;
    [SerializeField] private GameObject deleteSaveStateButtonPrefab;
    [SerializeField] private Transform saveStateLayoutGroup;
    public bool gameStateLoaded;

    [Header("MainMenuScreens")]
    [SerializeField] private GameObject loadScreen;
    [SerializeField] private GameObject optionsScreen;
    [SerializeField] private GameObject creditsScreen;
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
    public AudioMixer audioMixer;

    //Starts game with main menu music
    private void Start()
    {
        EventSystem.current.SetSelectedGameObject(firstMainMenuSelected);
        loadScreen.SetActive(false);
        optionsScreen.SetActive(false);
        creditsScreen.SetActive(false);
        AudioManager.Instance.Play("MainMenuMusic");
    }

    //The link to the programmers linked in for the button when clicked
    public void LinkedInAlexanderPfeifer()
    {
        Application.OpenURL("https://www.linkedin.com/in/alexander-pfeifer-5b858128b/");
    }

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
    
    //Starts Button Clicked Sound
    public void PressButtonSound()
    {
        InGameUI.Instance.PressButtonSound();
    }

    //Opens options menu and closes every other screen
    public void OpenOptionsMenu()
    {
        StartCoroutine(SetScreen(false, true, false, true, false, true));
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(loadButton);
    }
    
    //Opens credits menu and closes every other screen
    public void OpenCreditsMenu()
    {
        StartCoroutine(SetScreen(false, false, true, true, true, false));
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(optionsButton);
    }
    
    //Deletes load game buttons
    private void DeleteLoadMenuButtons()
    {
        loadButtonsList.ForEach(Destroy);
        gameStateLoaded = false;
    }
    
    //Creates load game buttons
    public void CreateLoadMenuButtons()
    {
        StartCoroutine(SetScreen(true, false, false, false, true, true));
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstMainMenuSelected);

        if (!gameStateLoaded)
        {
            string[] saveGameNames = SaveFileManager.GetAllSaveFileNames();
        
            for (int i = 0; i < saveGameNames.Length; i++)
            {
                newLoadButton = Instantiate(loadLevelButtonPrefab, saveStateLayoutGroup);
                loadButtonsList.Add(newLoadButton);
                
                TextMeshProUGUI buttonText = newLoadButton.GetComponentInChildren<TextMeshProUGUI>();
                buttonText.text = saveGameNames[i];
                string saveStateName = saveGameNames[i];

                var loadButton = newLoadButton.GetComponent<Button>();
                loadButton.onClick.AddListener(PressButtonSound);
                loadButton.onClick.AddListener(() => LoadGame(saveStateName));
                
                newDeleteButton = Instantiate(deleteSaveStateButtonPrefab, saveStateLayoutGroup);
                loadButtonsList.Add(newDeleteButton);
                
                var deleteButton = newDeleteButton.GetComponent<Button>();
                deleteButton.onClick.AddListener(PressButtonSound);
                deleteButton.onClick.AddListener(() => DeleteSaveState(saveStateName));
            }

            gameStateLoaded = true;
        }
    }

    //Sets game to fullscreen
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    //Sets volume of master
    public void SetVolume(float volume)
    {
        audioMixer.SetFloat("Master", Mathf.Log10(volume) * 20);
    }
    
    //Sets volume of SFX
    public void SetSfxVolume(float volume)
    {
        audioMixer.SetFloat("SFX", Mathf.Log10(volume) * 20);
    }
    
    //Sets volume of Music
    public void SetMusicVolume(float volume)
    {
        audioMixer.SetFloat("Music", Mathf.Log10(volume) * 20);
    }

    //Deletes a save state by name
    private void DeleteSaveState(string saveName)
    {
        SaveFileManager.DeleteSaveState(saveName);
        DeleteLoadMenuButtons();
        CreateLoadMenuButtons();
    }
    
    //Loads save state by name
    private void LoadGame(string saveName)
    {
        EventSystem.current.SetSelectedGameObject(null);
        GameSaveStateManager.Instance.LoadFromSave(saveName);
        AudioManager.Instance.Stop("MainMenuMusic");
        gameStateLoaded = false;
    }

    //Sets loading screen when starting game
    public void SetLoadingScreen()
    {
        InGameUI.Instance.loadingScreenAnim.SetTrigger("Start");
    }

    //Changes screen according to the booleans that are given with the method
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

    //Start Stall Shutter Animation
    public void SetAnimation()
    {
        if (stallShutterAnimator.GetBool("GoUp"))
        {
            stallShutterAnimator.SetTrigger("ChangeScreen");
        }

        stallShutterAnimator.SetBool("GoUp", true);
    }
    
    //Quits game
    public void QuitGame()
    {
        AudioManager.Instance.Stop("MainMenuMusic");
        Application.Quit();
    }
}
