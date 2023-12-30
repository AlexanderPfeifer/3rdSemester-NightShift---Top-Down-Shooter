using System.Collections;
using TMPro;
using UnityEngine;

public class InGameUI : MonoBehaviour
{
    public static InGameUI instance;
    [SerializeField] private TextMeshProUGUI collectibleText;
    [SerializeField] private TextMeshProUGUI weaponText;
    [SerializeField] private InGameCollectedObjects collectedObjects;
    [SerializeField] private GameObject pauseScreen;
    [HideInInspector] public bool gameIsPaused;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        GameSaveStateManager.instance.OnStateChanged += OnStateChange;
    }
    
    private void OnStateChange(GameSaveStateManager.GameState newState)
    {
        //we toggle the availability of the inGame menu whenever the game state changes
        //bool iwas = newState == GameSaveStateManager.GameState.InGame;
    }
    
    public void GoToMainMenu()
    {
        GameSaveStateManager.instance.GoToMainMenu();
        PauseGame();
    }

    //this is called via the "save game" button
    public void SaveGame()
    {
        GameSaveStateManager.instance.SaveGame("NightShiftSaveState");
    }

    private void DisplayCollectedLetters()
    {
        string text = "";
        
        var collectedCollectibles = GameSaveStateManager.instance.saveGameDataManager.collectedCollectiblesIdentifiers;

        for (int index = 0; index < collectedCollectibles.Count; index++)
        {
            var letter = collectedObjects.GetCollectibleDataByIdentifier(collectedCollectibles[index]);
            if (letter == null)
                return;
            text += letter.header.ToUpper() + "\n";
            text += letter.content + "\n\n";
        }

        StartCoroutine(LetterByLetterTextCoroutine(collectibleText, text));
    }
    
    private void DisplayCollectedWeapons()
    {
        string text = "";
        
        var collectedCollectibles = GameSaveStateManager.instance.saveGameDataManager.collectedWeaponsIdentifiers;

        for (int index = 0; index < collectedCollectibles.Count; index++)
        {
            var letter = collectedObjects.GetWeaponDataByIdentifier(collectedCollectibles[index]);
            if (letter == null)
                return;
            text += letter.weaponName.ToUpper() + "\n";
            text += letter.weaponDescription + "\n\n";
        }

        StartCoroutine(LetterByLetterTextCoroutine(weaponText, text));
    }

    private IEnumerator LetterByLetterTextCoroutine(TextMeshProUGUI textField, string text)
    {
        //an implementation for displaying a text letter by letter.
        string currentText = "";
        for (int index = 0; index < text.Length; index++)
        {
            currentText += text[index];
            textField.text = currentText;
            yield return new WaitForSeconds(0.05f);
        }
    }
    
    public void PauseGame()
    {
        if (gameIsPaused)
        {
            gameIsPaused = false;
            pauseScreen.SetActive(false);
        }
        else
        {
            DisplayCollectedLetters();
            DisplayCollectedWeapons();
            gameIsPaused = true;
            pauseScreen.SetActive(true);
        }
    }
}
