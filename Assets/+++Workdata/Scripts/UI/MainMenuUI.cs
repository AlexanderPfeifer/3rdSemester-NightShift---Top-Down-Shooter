using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject loadLevelButtonPrefab;
    [SerializeField] private GameObject deleteSaveStateButtonPrefab;

    [SerializeField] private Transform saveStateLayoutGroup;

    [SerializeField] private Animator stallShutterAnimator;

    private bool gameStateLoaded;

    public AudioMixer audioMixer;
    public AudioMixer audioSFXMixer;
    public AudioMixer audioMusicMixer;

    private GameObject newLoadButton;
    private GameObject newDeleteButton;

    [SerializeField] private List<GameObject> loadButtonsList;

    [SerializeField] private GameObject loadScreen;
    [SerializeField] private GameObject optionsScreen;
    [SerializeField] private GameObject creditsScreen;
    
    [SerializeField] private GameObject loadButton;
    [SerializeField] private GameObject optionsButton;
    [SerializeField] private GameObject creditsButton;


    private void Start()
    {
        loadScreen.SetActive(false);
        optionsScreen.SetActive(false);
        creditsScreen.SetActive(false);
    }

    public void StartNewGame()
    {
        DateTime dt = DateTime.Now;
        GameSaveStateManager.instance.StartNewGame("SaveState               " + dt.ToString("yyyy-MM-ddTHH-mm"));
        
        gameStateLoaded = false;
    }

    public void OpenOptionsMenu()
    {
        StartCoroutine(SetScreen(false, true, false, true, false, true));

    }
    
    public void OpenCreditsMenu()
    {
        StartCoroutine(SetScreen(false, false, true, true, true, false));

    }
    
    private void DeleteLoadMenuButtons()
    {
        loadButtonsList.ForEach(Destroy);
        gameStateLoaded = false;
    }
    
    public void CreateLoadMenuButtons()
    {
        StartCoroutine(SetScreen(true, false, false, false, true, true));
        
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
                loadButton.onClick.AddListener(() => LoadGame(saveStateName));
                
                newDeleteButton = Instantiate(deleteSaveStateButtonPrefab, saveStateLayoutGroup);
                loadButtonsList.Add(newDeleteButton);
                
                var deleteButton = newDeleteButton.GetComponent<Button>();
                deleteButton.onClick.AddListener(() => DeleteSaveState(saveStateName));
            }

            gameStateLoaded = true;
        }
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void SetVolume(float volume)
    {
        audioMixer.SetFloat("volume", volume);
    }
    
    public void SetSfxVolume(float volume)
    {
        audioSFXMixer.SetFloat("volume", volume);
    }
    
    public void SetMusicVolume(float volume)
    {
        audioMusicMixer.SetFloat("volume", volume);
    }

    private void DeleteSaveState(string saveName)
    {
        SaveFileManager.DeleteSaveState(saveName);
        DeleteLoadMenuButtons();
        CreateLoadMenuButtons();
    }
    
    private void LoadGame(string saveName)
    {
        GameSaveStateManager.instance.LoadFromSave(saveName);
        gameStateLoaded = false;
    }

    private IEnumerator SetScreen(bool loadScreen, bool optionsScreen, bool creditsScreen, bool loadButton, bool optionsButton, bool creditsButton)
    {
        yield return new WaitForSeconds(0.7f);
        this.loadScreen.SetActive(loadScreen);
        this.optionsScreen.SetActive(optionsScreen);
        this.creditsScreen.SetActive(creditsScreen);
        
        this.loadButton.GetComponent<Button>().interactable = loadButton;
        this.optionsButton.GetComponent<Button>().interactable = optionsButton;
        this.creditsButton.GetComponent<Button>().interactable = creditsButton;
    }

    public void SetAnimation()
    {
        if (stallShutterAnimator.GetBool("GoUp"))
        {
            stallShutterAnimator.SetTrigger("ChangeScreen");
        }

        stallShutterAnimator.SetBool("GoUp", true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
