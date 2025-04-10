using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject inventory;
    [HideInInspector] public bool inventoryIsOpened;
    public GameObject firstInventorySelected;

    [Header("Inventory Collected Item Information")]
    [SerializeField] private TextMeshProUGUI inventoryText;
    [SerializeField] private TextMeshProUGUI inventoryHeader;
    [SerializeField] private Image inventoryImage;
    [SerializeField] private Button equipWeaponButton;
    [SerializeField] private CollectedCollectibles collectedCollectibles;
    private Dictionary<string, (GameObject obj, Sprite sprite, string text, string header, WeaponObjectSO weaponObjectSO)> collectedItemsDictionary;

    [Header("Inventory Collected Items")] 
    [SerializeField] private GameObject brokenLights;
    [SerializeField] private GameObject teddy;
    [SerializeField] private GameObject newsPaper;
    [SerializeField] private GameObject popcornPistol;
    [SerializeField] private GameObject frenchFriesAssaultRifle;
    [SerializeField] private GameObject magnumMagnum;
    [SerializeField] private GameObject cornDogHuntingRifle;
    [SerializeField] private GameObject lollipopShotgun;
    

    private void Start()
    {
        equipWeaponButton.gameObject.SetActive(false);
        
        collectedItemsDictionary = new Dictionary<string, (GameObject, Sprite, string, string, WeaponObjectSO)>();
    }

    public void ResetInventoryElements()
    {
        brokenLights.SetActive(false);
        newsPaper.SetActive(false);
        teddy.SetActive(false);
        magnumMagnum.SetActive(false);
        cornDogHuntingRifle.SetActive(false);
        popcornPistol.SetActive(false);
        lollipopShotgun.SetActive(false);
        frenchFriesAssaultRifle.SetActive(false);
        
        inventoryText.text = "";
        inventoryHeader.text = "";
        inventoryImage.gameObject.SetActive(false);
        equipWeaponButton.gameObject.SetActive(false);
    }
    
    private void SetWeaponThroughInventory(WeaponObjectSO weaponObjectSO)
    {
        PlayerBehaviour.Instance.weaponBehaviour.GetWeapon(weaponObjectSO);
    }

    private void ResetInventoryInformationText()
    {
        inventoryText.text = "";
        inventoryHeader.text = "";
        inventoryImage.gameObject.SetActive(false);
    }
    
    private void DisplayItem(string header)
    {
        ResetInventoryInformationText();
        
        inventoryImage.gameObject.SetActive(true);
        
        inventoryText.text += collectedItemsDictionary[header].text;
        inventoryHeader.text += collectedItemsDictionary[header].header;
        inventoryImage.sprite = collectedItemsDictionary[header].sprite;

        if (collectedItemsDictionary[header].weaponObjectSO != null)
        {
            equipWeaponButton.gameObject.SetActive(true);
            equipWeaponButton.onClick.RemoveAllListeners();
            equipWeaponButton.onClick.AddListener(() => SetWeaponThroughInventory(collectedItemsDictionary[header].weaponObjectSO));   
        }
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
            if(PlayerBehaviour.Instance.isPlayerBusy)
                return;
            
            inventory.SetActive(true);
            PlayerBehaviour.Instance.isPlayerBusy = true;
            DisplayCollectedCollectibles();
            DisplayCollectedWeapons();
            EventSystem.current.SetSelectedGameObject(firstInventorySelected);
            inventoryIsOpened = true;
        }
    }

    public void CloseInventory()
    {
        inventory.SetActive(false);
        PlayerBehaviour.Instance.isPlayerBusy = false;
        EventSystem.current.SetSelectedGameObject(null);
        inventoryIsOpened = false;
        ResetInventoryInformationText();
    }

    private void DisplayCollectedCollectibles()
    {
        var _collectedCollectibles = GameSaveStateManager.Instance.saveGameDataManager.collectedCollectiblesIdentifiers;

        foreach (var _identifier in _collectedCollectibles)
        {
            var _text = "";
            var _headerText = "";

            var _collectible = collectedCollectibles.GetCollectibleDataByIdentifier(_identifier);
            
            if (_collectible == null)
                return;
            
            _headerText += _collectible.header;
            _text += _collectible.content;
            var _spriteCollectible = _collectible.icon;
            
            switch (_headerText)
            {
                case "Broken Lights" :
                    ActivateInventoryItem(brokenLights, _headerText, _spriteCollectible, _text, null);
                    break;
                case "News Paper" :
                    ActivateInventoryItem(newsPaper, _headerText, _spriteCollectible, _text, null);
                    break;
                case "Stuffed Animal" :
                    ActivateInventoryItem(teddy, _headerText, _spriteCollectible, _text, null);
                    break;
            }
        }
    }
    
    private void DisplayCollectedWeapons()
    {
        var _collectedWeapons = GameSaveStateManager.Instance.saveGameDataManager.collectedWeaponsIdentifiers;

        foreach (var _identifier in _collectedWeapons)
        {
            var _headerText = "";
            var _text = "";
            var _weapon = PlayerBehaviour.Instance.weaponBehaviour.allWeaponPrizes.FirstOrDefault(w => w.weaponName == _identifier);
            
            if (_weapon == null)
                return;
            
            _headerText += _weapon.weaponName;
            _text += _weapon.weaponDescription;
            if (_weapon.hasAbilityUpgrade)
            {
                _text += "\n" + "\n" + "Special Ability:" + "\n" + _weapon.weaponAbilityDescription + "\n" + "\n" + "Damage: " + _weapon.bulletDamage +  "\n" + "Clipsize: " + _weapon.clipSize;
            }
            else
            {
                _text += "\n" + "\n" + "Damage: " + _weapon.bulletDamage +  "\n" + "Clipsize: " + _weapon.clipSize;
            }
            
            var _spriteWeapon = _weapon.uiWeaponVisual;
            
            var _itemIdentifier = _headerText;
            
            switch (_itemIdentifier)
            {
                case "Magnum magnum" :
                    ActivateInventoryItem(magnumMagnum, _headerText, _spriteWeapon, _text, _weapon);
                    break;
                case "French Fries AR" :
                    ActivateInventoryItem(frenchFriesAssaultRifle, _headerText, _spriteWeapon, _text,_weapon);
                    break;
                case "Lollipop Shotgun" :
                    ActivateInventoryItem(lollipopShotgun, _headerText, _spriteWeapon, _text, _weapon);
                    break;
                case "Corn Dog Hunting Rifle" :
                    ActivateInventoryItem(cornDogHuntingRifle, _headerText, _spriteWeapon, _text, _weapon);
                    break;
                case "Popcorn Launcher" :
                    ActivateInventoryItem(popcornPistol, _headerText, _spriteWeapon, _text, _weapon);
                    break;
            }
        }
    }
    
    private void ActivateInventoryItem(GameObject item, string headerText, Sprite spriteItem, string text, WeaponObjectSO weaponObjectSO)
    {
        if (!collectedItemsDictionary.TryGetValue(headerText, out _))
        {
            collectedItemsDictionary[headerText] = (item, spriteItem, text, headerText, weaponObjectSO); 
            item.SetActive(true);
            item.GetComponent<Button>().onClick.AddListener(() => DisplayItem(headerText));
        }
    }
}
