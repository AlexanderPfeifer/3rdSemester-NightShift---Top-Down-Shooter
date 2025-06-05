using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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
    public Button fillWeaponAmmoButton;
    [SerializeField] private GameObject buttonsGameObject;
    [SerializeField] private Sprite pinkButtonSprite;
    [SerializeField] private Sprite blueButtonSprite;

    [Header("ShopWindow")] 
    public GameObject fortuneWheel;
    [SerializeField] private GameObject weapons;
    private int currentWeaponSelectionWindow;
    [SerializeField] private string[] selectionWindows;
    public GameObject switchWindowButtons;
    [SerializeField, TextArea(3, 10)] private string fortuneWheelText;
    
    [Header("Controller Selection")]
    [SerializeField] private Button changeWindowButton;
    public Button spinFortuneWheelButton;
    
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

    public void DisplayWeaponWindowAdding(int windowSwitch)
    {
        currentWeaponSelectionWindow += windowSwitch;

        if (currentWeaponSelectionWindow == selectionWindows.Length)
        {
            currentWeaponSelectionWindow = 0;
        }
        else if (currentWeaponSelectionWindow == -1)
        {
            currentWeaponSelectionWindow = selectionWindows.Length - 1;
        }
        
        DisplayItem(selectionWindows[currentWeaponSelectionWindow]);
    }

    private void DisplayWeaponWindowOverride(int number)
    {
        currentWeaponSelectionWindow = number;
        
        DisplayItem(selectionWindows[currentWeaponSelectionWindow]);
    }

    public void UpgradeWeapon(WeaponObjectSO weapon)
    {
        WeaponObjectSO[] _upgradeTiers = weapon.name switch
        {
            "Magnum magnum" => magnumMagnumUpgradeTiers,
            "French Fries AR" => assaultRifleUpgradeTiers,
            "Lollipop Shotgun" => lollipopShotgunUpgradeTiers,
            "Corn Dog Hunting Rifle" => huntingRifleUpgradeTiers,
            "Popcorn Launcher" => popcornLauncherUpgradeTiers,
            _ => null
        };

        int _currentTierOnUpgradingWeapon = weapon.upgradeTier;

        if (_upgradeTiers != null && _currentTierOnUpgradingWeapon < _upgradeTiers.Length && PlayerBehaviour.Instance.playerCurrency.SpendCurrency(tierCosts[_currentTierOnUpgradingWeapon]))
        {
            for (int _i = 0; _i < PlayerBehaviour.Instance.weaponBehaviour.allWeaponPrizes.Count; _i++)
            {
                if (PlayerBehaviour.Instance.weaponBehaviour.allWeaponPrizes[_i] != weapon) 
                    continue;
                
                AudioManager.Instance.Play("Upgrade");
                PlayerBehaviour.Instance.weaponBehaviour.allWeaponPrizes[_i] = _upgradeTiers[_currentTierOnUpgradingWeapon];
                PlayerBehaviour.Instance.weaponBehaviour.GetWeapon(PlayerBehaviour.Instance.weaponBehaviour.allWeaponPrizes[_i]);
                foreach (var _selected in selectionWindows)
                {
                    if (_selected == PlayerBehaviour.Instance.weaponBehaviour.allWeaponPrizes[_i].weaponName)
                    {
                        int _index = System.Array.IndexOf(selectionWindows, _selected);
                        DisplayWeaponWindowOverride(_index);
                    }
                }
                break;
            }
        }

        ResetWeaponDescriptions();
    }

    private void FillWeaponAmmo(WeaponObjectSO weapon)
    {
        //Checks for broken pistol because there refilling ammo does not cost
        if (TutorialManager.Instance.fillAmmoForFree || PlayerBehaviour.Instance.playerCurrency.SpendCurrency(fillAmmoCost))
        {
            PlayerBehaviour.Instance.weaponBehaviour.ObtainAmmoDrop(null, weapon.ammunitionBackUpSize, true);

            TutorialManager.Instance.ExplainGenerator();
            PlayerBehaviour.Instance.ammoText.text = "";
            
            AudioManager.Instance.Play("Reload");
            fillWeaponAmmoButton.GetComponentInChildren<TextMeshProUGUI>().text = "AMMO FULL";
            ApplyHoverOnlyToButton(fillWeaponAmmoButton, false);
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
            Navigation _nav = changeWindowButton.navigation;
            _nav.selectOnUp = fillWeaponAmmoButton;
            changeWindowButton.navigation = _nav;    
            
            fortuneWheel.SetActive(false);
            weapons.SetActive(true);
            ResetWeaponDescriptions();
        }
        else
        {
            Navigation _nav = changeWindowButton.navigation;
            _nav.selectOnUp = spinFortuneWheelButton;
            changeWindowButton.navigation = _nav;   
            
            InGameUIManager.Instance.dialogueUI.StopCurrentAndTypeNewTextCoroutine(fortuneWheelText, null, InGameUIManager.Instance.dialogueUI.currentTextBox);
            fortuneWheel.SetActive(true);
            weapons.SetActive(false);
        }
        
        EventSystem.current.SetSelectedGameObject(changeWindowButton.gameObject);
    }

    private void EquipNewWeapon(WeaponObjectSO weaponObjectSO)
    {
        if (PlayerBehaviour.Instance.weaponBehaviour.currentEquippedWeapon != weaponObjectSO.weaponName)
        {
            AudioManager.Instance.Play("Equip");
            PlayerBehaviour.Instance.weaponBehaviour.GetWeapon(weaponObjectSO);
        }
    }

    private void DisplayItem(string header)
    {
        if (collectedItemsDictionary.Count == 0 || TutorialManager.Instance.hitSigns < 1)
        {
            return;
        }
        
        descriptionImage.gameObject.SetActive(true);
        descriptionHeader.gameObject.SetActive(true);
        levelFillImage.gameObject.SetActive(true);
        bulletDamageTextField.gameObject.SetActive(true);
        bulletDelayTextField.gameObject.SetActive(true);
        reloadSpeedTextField.gameObject.SetActive(true);
        clipSizeTextField.gameObject.SetActive(true);

        InGameUIManager.Instance.dialogueUI.shopText.text = "";
        
        if (!collectedItemsDictionary.TryGetValue(header, out _))
        {
            descriptionImage.sprite = PlayerBehaviour.Instance.weaponBehaviour.allWeaponPrizes.FirstOrDefault(w => w.weaponName == header)?.uiWeaponVisual;
            descriptionImage.color = Color.black;
            descriptionHeader.text = "???";
            bulletDamageTextField.text = "???";
            bulletDelayTextField.text = "???";
            reloadSpeedTextField.text = "???";
            clipSizeTextField.text = "???";
            
            buttonsGameObject.SetActive(false);
            return;
        }

        if (InGameUIManager.Instance.dialogueUI.currentTextBox.text.Length == 0)
        {
            InGameUIManager.Instance.dialogueUI.StopCurrentAndTypeNewTextCoroutine(collectedItemsDictionary[header].weaponDescription, null, InGameUIManager.Instance.dialogueUI.currentTextBox);
        }

        descriptionImage.color = Color.white;
        bulletDamageTextField.text = collectedItemsDictionary[header].bulletDamageText;
        bulletDelayTextField.text = collectedItemsDictionary[header].shotDelayText;
        reloadSpeedTextField.text = collectedItemsDictionary[header].reloadSpeedText;
        clipSizeTextField.text = collectedItemsDictionary[header].clipSizeText;
        descriptionHeader.text = collectedItemsDictionary[header].header;
        descriptionImage.sprite = collectedItemsDictionary[header].sprite;
        levelFillImage.fillAmount = collectedItemsDictionary[header].levelFill;

        buttonsGameObject.SetActive(true);

        if (collectedItemsDictionary[header].weaponObjectSO != null)
        {
            equipWeaponButton.onClick.RemoveAllListeners();
            equipWeaponButton.onClick.AddListener(() => EquipNewWeapon(collectedItemsDictionary[header].weaponObjectSO));

            if (TutorialManager.Instance.fillAmmoForFree)
            {
                fillWeaponAmmoButton.GetComponentInChildren<TextMeshProUGUI>().text = "REFILL" + "\n" + "free";
            }
            else
            {
                fillWeaponAmmoButton.GetComponentInChildren<TextMeshProUGUI>().text = "REFILL" + "\n" + fillAmmoCost;
            }

            fillWeaponAmmoButton.onClick.RemoveAllListeners();
            fillWeaponAmmoButton.onClick.AddListener(() => FillWeaponAmmo(collectedItemsDictionary[header].weaponObjectSO));

            upgradeWeaponButton.onClick.RemoveAllListeners();
            if (header == "Broken Pistol")
            {
                upgradeWeaponButton.GetComponentInChildren<TextMeshProUGUI>().text = "NO UPGRADE";
            }
            else
            {
                upgradeWeaponButton.GetComponentInChildren<TextMeshProUGUI>().text = "UPGRADE" + "\n" + tierCosts[collectedItemsDictionary[header].weaponObjectSO.upgradeTier];
            }
            
            SetUpgradeButton(header);
        }
        
        if (header == PlayerBehaviour.Instance.weaponBehaviour.currentEquippedWeapon)
        {
            ApplyHoverOnlyToButton(equipWeaponButton, false);
        }
        else
        {
            ApplyHoverOnlyToButton(equipWeaponButton, true);
        }
        
        if (PlayerBehaviour.Instance.weaponBehaviour.ammunitionBackUpSize == PlayerBehaviour.Instance.weaponBehaviour.ammunitionInBackUp && 
            PlayerBehaviour.Instance.weaponBehaviour.ammunitionInClip == PlayerBehaviour.Instance.weaponBehaviour.maxClipSize)
        {
            fillWeaponAmmoButton.GetComponentInChildren<TextMeshProUGUI>().text = "AMMO FULL";
            ApplyHoverOnlyToButton(fillWeaponAmmoButton, false);   
        }
        else if (!PlayerBehaviour.Instance.playerCurrency.CheckEnoughCurrency(fillAmmoCost) && !TutorialManager.Instance.fillAmmoForFree)
        {
            //fillWeaponAmmoButton.GetComponentInChildren<TextMeshProUGUI>().text = "NO MONEY";
            ApplyHoverOnlyToButton(fillWeaponAmmoButton, false);
        }
        else
        {
            ApplyHoverOnlyToButton(equipWeaponButton, true);
        }

        if (header == "Broken Pistol")
        {
            ApplyHoverOnlyToButton(upgradeWeaponButton, false);
        }
    }
    
    void ApplyHoverOnlyToButton(Button originalButton, bool clickable)
    {
        if (originalButton == null) 
            return;

        if (originalButton.TryGetComponent(out HoverOnlyButton _hoverOnlyButton))
        {
            _hoverOnlyButton.disableClick = true;
            _hoverOnlyButton.image.sprite = pinkButtonSprite;
            
            return;
        }

        var _transition = originalButton.transition;
        var _onClickEvents = originalButton.onClick;
        var _colors = originalButton.colors;
        var _navigation = originalButton.navigation;
        var _image = originalButton.image;
        var _targetGraphic = originalButton.targetGraphic;
        var _spriteState = originalButton.spriteState;
        var _interactable = originalButton.interactable;
        var _gameObject = originalButton.gameObject;
        
        DestroyImmediate(originalButton);

        var _hoverOnly = _gameObject.AddComponent<HoverOnlyButton>();
        
        _hoverOnly.transition = _transition;
        _hoverOnly.onClick = _onClickEvents;
        _hoverOnly.colors = _colors;
        _hoverOnly.navigation = _navigation;
        _hoverOnly.image = _image;
        _hoverOnly.spriteState = _spriteState;
        _hoverOnly.targetGraphic = _targetGraphic;
        _hoverOnly.interactable = _interactable;

        if (clickable)
        {
            _hoverOnly.disableClick = false;
            SpriteState spriteState = _hoverOnly.spriteState;
            spriteState.selectedSprite = blueButtonSprite;
            _hoverOnly.spriteState = spriteState;        
        }
        else
        {
            if (GameInputManager.Instance.mouseIsLastUsedDevice)
            {
                _hoverOnly.image.sprite = pinkButtonSprite;
            }
            else
            {
                _hoverOnly.disableClick = true;
                SpriteState spriteState = _hoverOnly.spriteState;
                spriteState.selectedSprite = pinkButtonSprite;
                _hoverOnly.spriteState = spriteState;        
            }
        }
    }

    private void SetUpgradeButton(string header)
    {
        //I take a random weapon as a reference of how many upgrades a weapon has, assault rifle in this case
        if (collectedItemsDictionary[header].weaponObjectSO.upgradeTier >= assaultRifleUpgradeTiers.Length)
        {
            ApplyHoverOnlyToButton(upgradeWeaponButton, false);
            upgradeWeaponButton.GetComponentInChildren<TextMeshProUGUI>().text = "MAX LEVEL";
        }
        else if (!PlayerBehaviour.Instance.playerCurrency.CheckEnoughCurrency(tierCosts[collectedItemsDictionary[header].weaponObjectSO.upgradeTier]))
        {
            ApplyHoverOnlyToButton(upgradeWeaponButton, false);
        }
        else
        {
            upgradeWeaponButton.onClick.AddListener(() => UpgradeWeapon(collectedItemsDictionary[header].weaponObjectSO));
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
    
    public void ResetWeaponDescriptions()
    {
        var _collectedWeapons = GameSaveStateManager.Instance.saveGameDataManager.collectedWeaponsIdentifiers;

        foreach (var _identifier in _collectedWeapons)
        {
            var _headerText = "";
            var _weaponDescription = "";
            var _weapon = PlayerBehaviour.Instance.weaponBehaviour.allWeaponPrizes.FirstOrDefault(w => w.weaponName == _identifier);
            
            if (_weapon == null)
                return;
            
            _headerText += _weapon.weaponName;
            _weaponDescription += _weapon.weaponDescription;
            if (_weapon.hasAbilityUpgrade)
            {
                _weaponDescription += " " + _weapon.weaponAbilityDescription;
            }
            
            var _bulletDamageText = "Bullet Damage: " + "\n" + _weapon.bulletDamage;
            var _clipSizeText = "Clip Size: " + "\n" + _weapon.clipSize;
            var _bulletDelayText = "Shoot Delay: " + "\n" + _weapon.shootDelay; 
            var _reloadSpeedText = "Reload Speed: " + "\n" + _weapon.reloadTime;
            
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
        
        foreach (var _selected in selectionWindows)
        {
            if (_selected == PlayerBehaviour.Instance.weaponBehaviour.currentEquippedWeapon)
            {
                int _index = System.Array.IndexOf(selectionWindows, _selected);
                DisplayWeaponWindowOverride(_index);
            }
        }
    }
    
    private void ActivateInventoryItem(float levelFill, string headerText, Sprite spriteItem, string weaponDescription, string bulletDamageText, string bulletDelayText, string reloadSpeedText, string  clipSizeText,WeaponObjectSO weaponObjectSO)
    {
        collectedItemsDictionary[headerText] = (levelFill, spriteItem, weaponDescription, headerText, bulletDamageText, bulletDelayText, reloadSpeedText, clipSizeText, weaponObjectSO);
    }
}
