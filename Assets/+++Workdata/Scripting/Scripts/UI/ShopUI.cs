using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [Header("Upgraded Weapons")] 
    [SerializeField] private WeaponObjectSO[] lollipopShotgunUpgradeTiers;
    [SerializeField] private WeaponObjectSO[] assaultRifleUpgradeTiers;
    [SerializeField] private WeaponObjectSO[] magnumMagnumUpgradeTiers;
    [SerializeField] private WeaponObjectSO[] huntingRifleUpgradeTiers;
    [SerializeField] private WeaponObjectSO[] popcornLauncherUpgradeTiers;
    
    [Header("Shop Weapon Buttons")]
    [SerializeField] private Button equipWeaponButton;
    [SerializeField] private Button upgradeWeaponButton;
    [SerializeField] private Button fillWeaponAmmoButton;
    [SerializeField] private GameObject buttonsGameObject;

    [Header("ShopWindow")] 
    [SerializeField] private GameObject fortuneWheel;
    [SerializeField] private GameObject weapons;
    private int currentWeaponSelectionWindow;
    [SerializeField] private string[] selectionWindows;
    public GameObject switchWindowButtons;
    
    [Header("ShopCosts")]
    [SerializeField] private int[] tierCosts;
    [SerializeField] private int fillAmmoCost;

    [Header("Collected Item Description")]
    [FormerlySerializedAs("inventoryHeader")] [SerializeField] private TextMeshProUGUI descriptionHeader;
    [FormerlySerializedAs("inventoryImage")] [SerializeField] private Image descriptionImage;
    [FormerlySerializedAs("inventoryImage")] [SerializeField] private Image levelFillImage;
    [SerializeField] private TextMeshProUGUI bulletDamageTextField;
    [SerializeField] private TextMeshProUGUI bulletDelayTextField;
    [SerializeField] private TextMeshProUGUI reloadSpeedTextField;
    [SerializeField] private TextMeshProUGUI clipSizeTextField;
    [SerializeField] private CollectedCollectibles collectedCollectibles;
    private Dictionary<string, (float levelFill, Sprite sprite, string weaponDescription, string header, string bulletDamageText, string shotDelayText, string reloadSpeedText, string clipSizeText, WeaponObjectSO weaponObjectSO)> collectedItemsDictionary;

    [Header("Collected Items")] 
    [SerializeField] private GameObject brokenLights;
    [SerializeField] private GameObject teddy;
    [SerializeField] private GameObject newsPaper;

    private void Start()
    {
        collectedItemsDictionary = new Dictionary<string, (float, Sprite, string, string, string, string, string, string, WeaponObjectSO)>();
    }

    public void SwitchWindow(int windowSwitch)
    {
        currentWeaponSelectionWindow += windowSwitch;
        
        if (collectedItemsDictionary.TryGetValue(selectionWindows[currentWeaponSelectionWindow], out _))
        {
            Debug.Log("Displaying");
            DisplayItem(selectionWindows[currentWeaponSelectionWindow]);        
        }
        else if(TutorialManager.Instance.shotSigns >= 3)
        { 
            ResetDescriptionsTexts();
        }
    }

    private void UpgradeWeapon(WeaponObjectSO weapon, IReadOnlyList<WeaponObjectSO> upgradeTiers)
    {
        int _currentTierOnUpgradingWeapon = weapon.upgradeTier;

        if (_currentTierOnUpgradingWeapon < upgradeTiers.Count && PlayerBehaviour.Instance.playerCurrency.SpendCurrency(tierCosts[_currentTierOnUpgradingWeapon]))
        {
            for (int _i = 0; _i < PlayerBehaviour.Instance.weaponBehaviour.allWeaponPrizes.Count; _i++)
            {
                if (PlayerBehaviour.Instance.weaponBehaviour.allWeaponPrizes[_i] != weapon) 
                    continue;
                
                PlayerBehaviour.Instance.weaponBehaviour.allWeaponPrizes[_i] = upgradeTiers[_currentTierOnUpgradingWeapon];
                PlayerBehaviour.Instance.weaponBehaviour.GetWeapon(PlayerBehaviour.Instance.weaponBehaviour.allWeaponPrizes[_i]);
                break;
            }
        }
        else
        {
            //Show that the upgrade cannot be achieved
        }
    }

    private void FillWeaponAmmo(WeaponObjectSO weapon)
    {
        //Checks for broken pistol because there refilling ammo does not cost
        if (TutorialManager.Instance.fillAmmoForFree || 
            (PlayerBehaviour.Instance.playerCurrency.SpendCurrency(fillAmmoCost) && 
             weapon.ammunitionInBackUp != PlayerBehaviour.Instance.weaponBehaviour.ammunitionInBackUp))
        {
            PlayerBehaviour.Instance.weaponBehaviour.ObtainAmmoDrop(null, weapon.ammunitionInBackUp);

            TutorialManager.Instance.ExplainGenerator();
        }
        else
        {
            //Show that the fill ammo cannot be achieved
        }
    }

    public void SetShopWindow()
    {
        if (fortuneWheel.activeSelf)
        {
            fortuneWheel.SetActive(false);
            weapons.SetActive(true);
        }
        else
        {
            fortuneWheel.SetActive(true);
            weapons.SetActive(false);
        }
    }
    
    public void ResetShopElements()
    {
        brokenLights.SetActive(false);
        newsPaper.SetActive(false);
        teddy.SetActive(false);

        InGameUIManager.Instance.dialogueUI.shopText.text = "";
        descriptionHeader.text = "";
        descriptionImage.color = Color.black;
    }
    
    private void EquipNewWeapon(WeaponObjectSO weaponObjectSO)
    {
        if(PlayerBehaviour.Instance.weaponBehaviour.currentEquippedWeapon != weaponObjectSO.weaponName)
            PlayerBehaviour.Instance.weaponBehaviour.GetWeapon(weaponObjectSO);
    }

    public void ResetDescriptionsTexts()
    {
        descriptionImage.gameObject.SetActive(true);
        descriptionHeader.gameObject.SetActive(true);
        levelFillImage.gameObject.SetActive(true);
        bulletDamageTextField.gameObject.SetActive(true);
        bulletDelayTextField.gameObject.SetActive(true);
        reloadSpeedTextField.gameObject.SetActive(true);
        clipSizeTextField.gameObject.SetActive(true);
        
        InGameUIManager.Instance.dialogueUI.shopText.text = "???";
        descriptionHeader.text = "???";
        bulletDamageTextField.text = "???";
        bulletDelayTextField.text = "???";
        reloadSpeedTextField.text = "???";
        clipSizeTextField.text = "???";
        
        buttonsGameObject.SetActive(false);
    }
    
    private void DisplayItem(string header)
    {
        ResetDescriptionsTexts();
        
        InGameUIManager.Instance.dialogueUI.shopText.text = "";

        StartCoroutine(InGameUIManager.Instance.dialogueUI.TypeTextCoroutine(collectedItemsDictionary[header].weaponDescription, null));

        descriptionHeader.text = collectedItemsDictionary[header].header;
        descriptionImage.sprite = collectedItemsDictionary[header].sprite;
        bulletDamageTextField.text = collectedItemsDictionary[header].bulletDamageText;
        bulletDelayTextField.text = collectedItemsDictionary[header].shotDelayText;
        reloadSpeedTextField.text = collectedItemsDictionary[header].reloadSpeedText;
        clipSizeTextField.text = collectedItemsDictionary[header].clipSizeText;
        buttonsGameObject.SetActive(true);

        if (collectedItemsDictionary[header].weaponObjectSO != null)
        {
            equipWeaponButton.interactable = true;
            equipWeaponButton.onClick.RemoveAllListeners();
            equipWeaponButton.onClick.AddListener(() => EquipNewWeapon(collectedItemsDictionary[header].weaponObjectSO));

            fillWeaponAmmoButton.interactable = true;
            fillWeaponAmmoButton.onClick.RemoveAllListeners();
            fillWeaponAmmoButton.onClick.AddListener(() => FillWeaponAmmo(collectedItemsDictionary[header].weaponObjectSO));

            upgradeWeaponButton.interactable = true;
            upgradeWeaponButton.onClick.RemoveAllListeners();
            switch (header)
            {
                case "Magnum magnum" :
                    upgradeWeaponButton.onClick.AddListener(() => UpgradeWeapon(collectedItemsDictionary[header].weaponObjectSO, 
                        magnumMagnumUpgradeTiers));
                    break;
                            
                case "French Fries AR" :
                    upgradeWeaponButton.onClick.AddListener(() => UpgradeWeapon(collectedItemsDictionary[header].weaponObjectSO, 
                        assaultRifleUpgradeTiers));
                    break;
                            
                case "Lollipop Shotgun" :
                    upgradeWeaponButton.onClick.AddListener(() => UpgradeWeapon(collectedItemsDictionary[header].weaponObjectSO, 
                        lollipopShotgunUpgradeTiers));
                    break;
                            
                case "Corn Dog Hunting Rifle" :
                    upgradeWeaponButton.onClick.AddListener(() => UpgradeWeapon(collectedItemsDictionary[header].weaponObjectSO, 
                        huntingRifleUpgradeTiers));
                    break;
                            
                case "Popcorn Launcher" :
                    upgradeWeaponButton.onClick.AddListener(() => UpgradeWeapon(collectedItemsDictionary[header].weaponObjectSO, 
                        popcornLauncherUpgradeTiers));
                    break;
            }
        }
    }

    public void DisplayCollectedCollectibles()
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
                    //ActivateInventoryItem(brokenLights, _headerText, _spriteCollectible, _text, null);
                    break;
                case "News Paper" :
                    //ActivateInventoryItem(newsPaper, _headerText, _spriteCollectible, _text, null);
                    break;
                case "Stuffed Animal" :
                    //ActivateInventoryItem(teddy, _headerText, _spriteCollectible, _text, null);
                    break;
            }
        }
    }
    
    public void DisplayCollectedWeapons()
    {
        var _collectedWeapons = GameSaveStateManager.Instance.saveGameDataManager.collectedWeaponsIdentifiers;

        foreach (var _identifier in _collectedWeapons)
        {
            var _headerText = "";
            var _weaponDescription = "";
            var _bulletDamageText = "";
            var _bulletDelayText = "";
            var _reloadSpeedText = "";
            var _clipSizeText = "";
            var _weapon = PlayerBehaviour.Instance.weaponBehaviour.allWeaponPrizes.FirstOrDefault(w => w.weaponName == _identifier);
            
            if (_weapon == null)
                return;
            
            _headerText += _weapon.weaponName;
            _weaponDescription += _weapon.weaponDescription;
            if (_weapon.hasAbilityUpgrade)
            {
                _weaponDescription += "\n" + "\n" + "Special Ability:" + "\n" + _weapon.weaponAbilityDescription;
            }
            else
            {
                _bulletDamageText = "Bullet Damage: " + "\n" + _weapon.bulletDamage;
                _clipSizeText = "Clip Size: " + "\n" + _weapon.clipSize;
                _bulletDelayText = "Shoot Delay: " + "\n" + _weapon.shootDelay; 
                _reloadSpeedText = "Reload Speed: " + "\n" + _weapon.reloadTime;
            }
            
            var _spriteWeapon = _weapon.uiWeaponVisual;
            
            var _itemIdentifier = _headerText;
            
            switch (_itemIdentifier)
            {
                case "Magnum magnum" :
                    ActivateInventoryItem((float)_weapon.upgradeTier / 3, _headerText, _spriteWeapon, _weaponDescription, _bulletDamageText, _bulletDelayText, _reloadSpeedText, _clipSizeText, _weapon);
                    break;
                case "French Fries AR" :
                    ActivateInventoryItem((float)_weapon.upgradeTier / 3, _headerText, _spriteWeapon, _weaponDescription, _bulletDamageText, _bulletDelayText, _reloadSpeedText, _clipSizeText, _weapon);
                    break;
                case "Lollipop Shotgun" :
                    ActivateInventoryItem((float)_weapon.upgradeTier / 3, _headerText, _spriteWeapon, _weaponDescription, _bulletDamageText, _bulletDelayText, _reloadSpeedText, _clipSizeText, _weapon);
                    break;
                case "Corn Dog Hunting Rifle" :
                    ActivateInventoryItem((float)_weapon.upgradeTier / 3, _headerText, _spriteWeapon, _weaponDescription, _bulletDamageText, _bulletDelayText, _reloadSpeedText, _clipSizeText, _weapon);
                    break;
                case "Popcorn Launcher" :
                    ActivateInventoryItem((float)_weapon.upgradeTier / 3, _headerText, _spriteWeapon, _weaponDescription, _bulletDamageText, _bulletDelayText, _reloadSpeedText, _clipSizeText, _weapon);
                    break;
                case "Broken Pistol" :
                    ActivateInventoryItem((float)_weapon.upgradeTier / 3, _headerText, _spriteWeapon, _weaponDescription, _bulletDamageText, _bulletDelayText, _reloadSpeedText, _clipSizeText, _weapon);
                    break;
            }
        }
    }
    
    private void ActivateInventoryItem(float levelFill, string headerText, Sprite spriteItem, string weaponDescription, string bulletDamageText, string bulletDelayText, string reloadSpeedText, string  clipSizeText,WeaponObjectSO weaponObjectSO)
    {
        if (!collectedItemsDictionary.TryGetValue(headerText, out _))
        {
            collectedItemsDictionary[headerText] = (levelFill, spriteItem, weaponDescription, headerText, bulletDamageText, bulletDelayText, reloadSpeedText, clipSizeText, weaponObjectSO); 
        }
    }
}
