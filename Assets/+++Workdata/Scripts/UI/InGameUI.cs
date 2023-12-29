using System.Collections;
using TMPro;
using UnityEngine;

public class InGameUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI collectibleText;
    [SerializeField] private InGameCollectedObjects collectedObjects;
    [SerializeField] private GameObject pauseScreen;
    [HideInInspector] public bool gameIsPaused;

    private void Start()
    {
        GameSaveStateManager.instance.OnStateChanged += OnStateChange;
    }
    
    private void OnStateChange(GameSaveStateManager.GameState newState)
    {
        //we toggle the availability of the inGame menu whenever the game state changes
        bool isInGame = newState == GameSaveStateManager.GameState.InGame;
    }
    
    public void GoToMainMenu()
    {
        GameSaveStateManager.instance.GoToMainMenu();
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
            gameIsPaused = true;
            pauseScreen.SetActive(true);
        }
    }
}
