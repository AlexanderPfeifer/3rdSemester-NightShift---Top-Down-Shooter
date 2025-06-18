using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class MainMenuUIManager : Singleton<MainMenuUIManager>
{
    [Header("LoadGameScreen")]
    private GameObject newLoadButton;
    private GameObject newDeleteButton;
    [SerializeField] private List<GameObject> loadButtonsList;
    [SerializeField] private GameObject loadLevelButtonPrefab;
    [SerializeField] private GameObject deleteSaveStateButtonPrefab;
    [SerializeField] private Transform saveStateLayoutGroup;
    [SerializeField] private Button deleteSaveStateCheckButton;
    [SerializeField] private GameObject deleteSaveStateCheckPanel;
    [HideInInspector] public bool gameStateLoaded;
    [FormerlySerializedAs("configureButtonsManager")] [SerializeField] private AllButtonsConfiguration allButtonsConfiguration;

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
    [SerializeField] private GameObject wishlistButton;
    [SerializeField] private GameObject joinDiscordButton;

    [Header("Visuals")]
    [SerializeField] private Animator stallShutterAnimator;
    [SerializeField] private GameObject sunnyBackground;
    [SerializeField] private Sprite counterOff;
    [SerializeField] private Sprite counterOn;

    [Header("Fullscreen")]
    private const string FullScreenPlayerPrefs = "Fullscreen";
    private int fullScreenInt = 1;
    [SerializeField] private GameObject fullScreenCheck;
    private bool isFullScreenOn;

    [Header("AudioMixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("AudioMaster")]
    [SerializeField] private List<GameObject> masterPoints;
    private int currentVolumePointMaster = 4;
    private const string MasterVolumePlayerPrefs = "masterVolume";
    private float masterVolume = .5f;
    private int sameDirectionMinusMaster = 1;
    private int sameDirectionPlusMaster;

    [Header("AudioMusic")]
    [SerializeField] private List<GameObject> musicPoints;
    private int currentVolumePointMusic = 4;
    private const string MusicVolumePlayerPrefs = "musicVolume";
    private float musicVolume = .5f;
    private int sameDirectionMinusMusic = 1;
    private int sameDirectionPlusMusic;

    [Header("AudioSFX")]
    [SerializeField] private List<GameObject> sfxPoints;
    private int currentVolumePointSfx = 4;
    private const string SfxVolumePlayerPrefs = "sfxVolume";
    private float sfxVolume = .5f;
    private int sameDirectionMinusSfx = 1;
    private int sameDirectionPlusSfx;

    private void Start()
    {
        EventSystem.current.SetSelectedGameObject(firstMainMenuSelected);

        audioMixer.SetFloat("SFX", Mathf.Log10(sfxVolume) * 20);
        audioMixer.SetFloat("Music", Mathf.Log10(musicVolume) * 20);
        audioMixer.SetFloat("Master", Mathf.Log10(masterVolume) * 20);


        loadScreen.SetActive(false);
        optionsScreen.SetActive(false);
        creditsScreen.SetActive(false);
        
        AudioManager.Instance.Play("MainMenuMusic");
        
        if (GameSaveStateManager.Instance.gameGotFinished)
        {
            sunnyBackground.SetActive(true);
        }

        SetPlayerPrefs();
    }

    private void SetPlayerPrefs()
    {
        fullScreenInt = PlayerPrefs.GetInt(FullScreenPlayerPrefs, fullScreenInt);

        PlayerPrefs.GetFloat(MasterVolumePlayerPrefs, masterVolume);
        for (int i = 0; i < masterPoints.Count; i++)
        {
            if (masterVolume * 10 - 2 < i)
            {
                masterPoints[i].SetActive(true);
            }
        }

        PlayerPrefs.GetFloat(MusicVolumePlayerPrefs, musicVolume);
        for (int i = 0; i < musicPoints.Count; i++)
        {
            if (musicVolume * 10 - 2 < i)
            {
                musicPoints[i].SetActive(true);
            }
        }

        PlayerPrefs.GetFloat(SfxVolumePlayerPrefs, sfxVolume);
        for (int i = 0; i < sfxPoints.Count; i++)
        {
            if (sfxVolume * 10 - 2 < i)
            {
                sfxPoints[i].SetActive(true);
            }
        }

        Screen.fullScreen = fullScreenInt == 1;
        fullScreenCheck.SetActive(fullScreenInt == 1);
    }

    public void ChangeFullScreenMode()
    {
        isFullScreenOn = !isFullScreenOn;

        Screen.fullScreen = isFullScreenOn;

        fullScreenInt = isFullScreenOn ? 1 : 0;

        fullScreenCheck.SetActive(isFullScreenOn);

        PlayerPrefs.SetInt(FullScreenPlayerPrefs, fullScreenInt);
    }

    public void MusicVolumeMinus()
    {
        if (musicVolume <= 0)
            return;

        sameDirectionMinusMusic++;

        if (sameDirectionPlusMusic > 0)
        {
            currentVolumePointMusic++;
        }

        sameDirectionPlusMusic = 0;

        if (!(currentVolumePointMusic <= 0.2f))
        {
            currentVolumePointMusic--;
            musicPoints[currentVolumePointMusic].GetComponent<Image>().sprite = counterOff;
            ChangeMusicVolume(-.1f);
        }
    }

    public void MusicVolumePlus()
    {
        if (musicVolume >= 1)
            return;

        sameDirectionPlusMusic++;

        if (sameDirectionMinusMusic > 0)
        {
            currentVolumePointMusic--;
        }

        sameDirectionMinusMusic = 0;

        if (!(musicVolume > 0.99f))
        {
            currentVolumePointMusic++;
            musicPoints[currentVolumePointMusic].GetComponent<Image>().sprite = counterOn;
            ChangeMusicVolume(.1f);
        }
    }

    public void MasterVolumeMinus()
    {
        if (masterVolume <= 0)
            return;

        sameDirectionMinusMaster++;

        if (sameDirectionPlusMaster > 0)
        {
            currentVolumePointMaster++;
        }

        sameDirectionPlusMaster = 0;

        if (!(currentVolumePointMaster <= 0.2f))
        {
            currentVolumePointMaster--;
            masterPoints[currentVolumePointMaster].GetComponent<Image>().sprite = counterOff;
            ChangeMasterVolume(-.1f);
            AudioManager.Instance.Play("Shooting");
        }
    }

    public void MasterVolumePlus()
    {
        if (masterVolume >= 1)
            return;

        sameDirectionPlusMaster++;

        if (sameDirectionMinusMaster > 0)
        {
            currentVolumePointMaster--;
        }

        sameDirectionMinusMaster = 0;

        if (!(masterVolume > 0.99f))
        {
            currentVolumePointMaster++;
            masterPoints[currentVolumePointMaster].GetComponent<Image>().sprite = counterOn;
            ChangeMasterVolume(.1f);
            AudioManager.Instance.Play("Shooting");
        }
    }

    public void SfxVolumeMinus()
    {
        if (sfxVolume <= 0)
            return;

        sameDirectionMinusSfx++;

        if (sameDirectionPlusSfx > 0)
        {
            currentVolumePointSfx++;
        }

        sameDirectionPlusSfx = 0;

        if (!(currentVolumePointSfx <= 0.2f))
        {
            currentVolumePointSfx--;
            sfxPoints[currentVolumePointSfx].GetComponent<Image>().sprite = counterOff;
            ChangeSfxVolume(-.1f);
            AudioManager.Instance.Play("Shooting");
        }
    }

    public void SfxVolumePlus()
    {
        if (sfxVolume >= 1)
            return;

        sameDirectionPlusSfx++;

        if (sameDirectionMinusSfx > 0)
        {
            currentVolumePointSfx--;
        }

        sameDirectionMinusSfx = 0;

        if (!(sfxVolume > 0.99f))
        {
            currentVolumePointSfx++;
            sfxPoints[currentVolumePointSfx].GetComponent<Image>().sprite = counterOn;
            ChangeSfxVolume(.1f);
            AudioManager.Instance.Play("Shooting");
        }
    }

    private void ChangeMasterVolume(float value)
    {
        masterVolume += value;

        audioMixer.SetFloat("Master", Mathf.Log10(masterVolume) * 20);

        PlayerPrefs.SetFloat(MasterVolumePlayerPrefs, masterVolume);
    }

    private void ChangeMusicVolume(float value)
    {
        musicVolume += value;

        audioMixer.SetFloat("Music", Mathf.Log10(musicVolume) * 20);

        PlayerPrefs.SetFloat(MusicVolumePlayerPrefs, musicVolume);
    }

    private void ChangeSfxVolume(float value)
    {
        sfxVolume += value;

        audioMixer.SetFloat("SFX", Mathf.Log10(sfxVolume) * 20);

        PlayerPrefs.SetFloat(SfxVolumePlayerPrefs, sfxVolume);
    }

    public void JoinOurDiscord()
    {
        Application.OpenURL("https://discord.gg/mmYek3rY/");
    }

    public void WishlistOnSteam()
    {
        Application.OpenURL("https://store.steampowered.com/app/3206760/Night_Shift/");
    }

    public void OpenOptionsMenu()
    {
        StartCoroutine(SetScreen(false, true, false, true, false, true));
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(joinDiscordButton);
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
        deleteSaveStateCheckButton.onClick.RemoveListener(() => DeleteSaveState(saveName));
    }
    
    public void CreateLoadMenuButtons()
    {
        StartCoroutine(SetScreen(true, false, false, false, true, true));

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
                _loadButton.onClick.AddListener(() => AudioManager.Instance.Play("ButtonClick"));
                _loadButton.onClick.AddListener(() => LoadGame(_saveStateName));
                
                newDeleteButton = Instantiate(deleteSaveStateButtonPrefab, saveStateLayoutGroup);
                loadButtonsList.Add(newDeleteButton);
                
                var _deleteButton = newDeleteButton.GetComponent<Button>();
                _deleteButton.onClick.AddListener(() => AudioManager.Instance.Play("ButtonClick"));
                _deleteButton.onClick.AddListener(delegate{SetDeleteSaveStateCheck(_saveStateName);});
            }

            foreach (var _loadButtons in loadButtonsList)
            {
                allButtonsConfiguration.AddHoverEvent(_loadButtons);
            }

            gameStateLoaded = true;
        }
    }

    private void SetDeleteSaveStateCheck(string saveStateName)
    {
        deleteSaveStateCheckPanel.SetActive(true);
        deleteSaveStateCheckButton.onClick.AddListener(() => DeleteSaveState(saveStateName));
    }
    
    private void LoadGame(string saveName)
    {
        EventSystem.current.SetSelectedGameObject(null);
        GameSaveStateManager.Instance.LoadFromSave(saveName);
        AudioManager.Instance.FadeOut("MainMenuMusic", "InGameMusic");
        gameStateLoaded = false;
    }

    public void SetLoadingScreen()
    {
        SceneManager.Instance.loadingScreenAnim.SetTrigger("Start");
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
