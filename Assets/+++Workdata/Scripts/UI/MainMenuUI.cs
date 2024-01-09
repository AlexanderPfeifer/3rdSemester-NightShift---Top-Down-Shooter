using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject loadMenu;
    
    [SerializeField] private GameObject loadLevelButtonPrefab;
    [SerializeField] private GameObject deleteSaveStateButtonPrefab;

    [SerializeField] private Transform saveStateLayoutGroup;

    private bool gameStateLoaded;

    private GameObject newLoadButton;
    
    private GameObject newDeleteButton;

    [SerializeField] private List<GameObject> loadButtonsList;
    

    public void StartNewGame()
    {
        DateTime dt = DateTime.Now;
        GameSaveStateManager.instance.StartNewGame("SaveState            " + dt.ToString("yyyy-MM-ddTHH-mmZ"));
        
        gameStateLoaded = false;
    }

    private void DeleteLoadMenuButtons()
    {
        loadButtonsList.ForEach(Destroy);
        gameStateLoaded = false;
    }
    
    public void CreateLoadMenuButtons()
    {
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

    private void DeleteSaveState(string saveName)
    {
        SaveFileManager.DeleteSaveState(saveName);
        DeleteLoadMenuButtons();
        CreateLoadMenuButtons();
    }
    
    private void LoadGame(string saveName)
    {
        GameSaveStateManager.instance.LoadFromSave(saveName);
        ChangeMenu();
        gameStateLoaded = false;
    }

    public void ChangeMenu()
    {
        if (mainMenu.activeSelf)
        {
            mainMenu.SetActive(false);
            loadMenu.SetActive(true);
        }
        else
        {
            mainMenu.SetActive(true);
            loadMenu.SetActive(false);
        }
    }
}
