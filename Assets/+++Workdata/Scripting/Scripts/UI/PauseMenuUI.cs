using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class PauseMenuUI : MonoBehaviour
{
    [HideInInspector] public bool inventoryIsOpened;
    [SerializeField] private GameObject inventory;
    public GameObject firstPauseMenuSelected;

    private void OnEnable()
    {
        GameInputManager.Instance.OnGamePausedAction += OpenPauseMenu;
    }

    private void OnDisable()
    {
        GameInputManager.Instance.OnGamePausedAction -= OpenPauseMenu;
    }
    
    public void OpenPauseMenu(object sender, EventArgs e)
    {
        if (PlayerBehaviour.Instance == null || (PlayerBehaviour.Instance.IsPlayerBusy() && !inventoryIsOpened) || InGameUIManager.Instance.dialogueUI.IsDialoguePlaying())
        {
            return;
        }
        
        if (inventoryIsOpened)
        {
            ClosePauseMenu();
        }
        else
        {
            inventory.SetActive(true);
            PlayerBehaviour.Instance.SetPlayerBusy(true);
            EventSystem.current.SetSelectedGameObject(firstPauseMenuSelected);
            inventoryIsOpened = true;
        }
    }

    public void ClosePauseMenu()
    {
        inventory.SetActive(false);
        PlayerBehaviour.Instance.SetPlayerBusy(false);
        EventSystem.current.SetSelectedGameObject(null);
        inventoryIsOpened = false;
    }
}
