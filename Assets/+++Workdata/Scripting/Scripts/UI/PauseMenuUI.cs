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
        GameInputManager.Instance.OnGamePausedAction += OpenInventory;
    }

    private void OnDisable()
    {
        if(GameInputManager.Instance != null)
            GameInputManager.Instance.OnGamePausedAction -= OpenInventory;
    }
    
    public void OpenInventory(object sender, EventArgs e)
    {
        if (InGameUIManager.Instance.dialogueUI.IsDialoguePlaying() || PlayerBehaviour.Instance == null)
        {
            return;
        }
        
        if (inventoryIsOpened)
        {
            CloseInventory();
        }
        else
        {
            if(PlayerBehaviour.Instance.IsPlayerBusy())
                return;
            
            inventory.SetActive(true);
            PlayerBehaviour.Instance.SetPlayerBusy(true);
            EventSystem.current.SetSelectedGameObject(firstPauseMenuSelected);
            inventoryIsOpened = true;
        }
    }

    public void CloseInventory()
    {
        inventory.SetActive(false);
        PlayerBehaviour.Instance.SetPlayerBusy(false);
        EventSystem.current.SetSelectedGameObject(null);
        inventoryIsOpened = false;
        InGameUIManager.Instance.shopUI.ResetDescriptionsTexts();
    }
}
