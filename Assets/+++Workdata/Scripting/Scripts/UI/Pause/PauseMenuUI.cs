using System;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
{
    [Header("Pause Menu")]
    [HideInInspector] public bool pauseMenuActive;
    [SerializeField] private GameObject pauseMenu;
    public GameObject firstPauseMenuSelected;
    [SerializeField] private GameObject firstPauseSettingsSelected;
    [SerializeField] private GameObject pauseScreen;
    [SerializeField] private GameObject pauseSettingsScreen;

    [Header("AudioMaster")]
    [SerializeField] private GameObject[] masterPoints;
    [SerializeField] Button minusMasterButton;
    [SerializeField] Button plusMasterButton;

    [Header("AudioMusic")]
    [SerializeField] private GameObject[] musicPoints;
    [SerializeField] Button minusMusicButton;
    [SerializeField] Button plusMusicButton;

    [Header("AudioSFX")]
    [SerializeField] private GameObject[] sfxPoints;
    [SerializeField] Button minusSFXButton;
    [SerializeField] Button plusSFXButton;

    [Header("Fullscreen")]
    [SerializeField] private GameObject fullScreenCheck;
    [SerializeField] private Button fullScreenButton;

    private void OnEnable()
    {
        AudioManager.Instance.SetAudioPlayerPrefs(masterPoints, musicPoints, sfxPoints);

        GameInputManager.Instance.OnGamePausedAction += OpenPauseMenu;
    }

    private void OnDisable()
    {
        GameInputManager.Instance.OnGamePausedAction -= OpenPauseMenu;
    }

    private void Start()
    {
        AudioManager.Instance.SetAudioPlayerPrefs(masterPoints, musicPoints, sfxPoints);
        SceneManager.Instance.SetFullscreenPlayerPrefs(fullScreenCheck);

        minusMasterButton.onClick.AddListener(() => AudioManager.Instance.ChangeSpecificVolume(AudioManager.VolumeType.Master, masterPoints, false));
        plusMasterButton.onClick.AddListener(() => AudioManager.Instance.ChangeSpecificVolume(AudioManager.VolumeType.Master, masterPoints, true));

        minusMusicButton.onClick.AddListener(() => AudioManager.Instance.ChangeSpecificVolume(AudioManager.VolumeType.Music, musicPoints, false));
        plusMusicButton.onClick.AddListener(() => AudioManager.Instance.ChangeSpecificVolume(AudioManager.VolumeType.Music, musicPoints, true));

        minusSFXButton.onClick.AddListener(() => AudioManager.Instance.ChangeSpecificVolume(AudioManager.VolumeType.Sfx, sfxPoints, false));
        plusSFXButton.onClick.AddListener(() => AudioManager.Instance.ChangeSpecificVolume(AudioManager.VolumeType.Sfx, sfxPoints, true));

        fullScreenButton.onClick.AddListener(() => SceneManager.Instance.ChangeFullScreenMode(fullScreenCheck));
    }

    public void OpenSettings()
    {
        GameInputManager.Instance.SetNewButtonAsSelected(firstPauseSettingsSelected);
        pauseScreen.SetActive(false);
        pauseSettingsScreen.SetActive(true);
    }

    public void CloseSettings()
    {
        GameInputManager.Instance.SetNewButtonAsSelected(firstPauseMenuSelected);
        pauseScreen.SetActive(true);
        pauseSettingsScreen.SetActive(false);
    }

    public void OpenPauseMenu(object sender, EventArgs e)
    {
        if (PlayerBehaviour.Instance == null || (PlayerBehaviour.Instance.IsPlayerBusy() && !pauseMenuActive) || InGameUIManager.Instance.dialogueUI.IsDialoguePlaying())
        {
            return;
        }
        
        if (pauseMenuActive)
        {
            ClosePauseMenu();
        }
        else
        {
            pauseMenu.SetActive(true);
            PlayerBehaviour.Instance.SetPlayerBusy(true);
            GameInputManager.Instance.SetNewButtonAsSelected(firstPauseMenuSelected);
            pauseMenuActive = true;
        }
    }

    public void ClosePauseMenu()
    {
        pauseMenu.SetActive(false);
        PlayerBehaviour.Instance.SetPlayerBusy(false);
        pauseMenuActive = false;
    }
}
