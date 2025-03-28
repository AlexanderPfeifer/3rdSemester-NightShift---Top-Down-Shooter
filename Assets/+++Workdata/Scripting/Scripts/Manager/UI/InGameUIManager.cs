using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

public class InGameUIManager : MonoBehaviour
{
    [Header("Inventory")]
    [SerializeField] private GameObject inventory;
    [HideInInspector] public bool inventoryIsOpened;

    [Header("Inventory Collected Item Information")]
    [SerializeField] private TextMeshProUGUI inventoryText;
    [SerializeField] private TextMeshProUGUI inventoryHeader;
    [SerializeField] private Image inventoryImage;
    [SerializeField] private Button equipWeaponButton;
    [SerializeField] private CollectedCollectibles collectedCollectibles;

    [Header("Collectibles")]
    private Dictionary<string, (GameObject obj, Sprite sprite, string text, string header)> collectedCollectiblesDictionary;

    [Header("Weapons")] 
    public TextMeshProUGUI ammunitionInClipText;
    public TextMeshProUGUI ammunitionInBackUpText;
    private Dictionary<string, (GameObject obj, Sprite sprite, string text, string header, WeaponObjectSO weaponObjectSO)> collectedWeapons;

    [Header("Inventory Preview Items")]
    [SerializeField] private GameObject brokenLights;
    [SerializeField] private GameObject teddy;
    [SerializeField] private GameObject newsPaper;
    [SerializeField] private GameObject popcornPistol;
    [SerializeField] private GameObject frenchFriesAssaultRifle;
    [SerializeField] private GameObject magnumMagnum;
    [SerializeField] private GameObject cornDogHuntingRifle;
    [SerializeField] private GameObject lollipopShotgun;
    
    [Header("Fight")]
    public GameObject inGameUIWeaponVisual;
    public GameObject fightScene;
    public GameObject pressSpace;
    public Image rideTimeImage;
    public Image rideHpImage;
    public Image abilityProgressImage;
    public GameObject abilityFillBar;
    
    [Header("Player UI")]
    public GameObject inGameUIScreen;
    public GameObject firstInventorySelected;
    [SerializeField] private GameObject shopScreen;
    [SerializeField] private GameObject generatorScreen;
    [HideInInspector] public CurrencyUI currencyUI;
    
    [Header("End Sequence")]
    [HideInInspector] public bool changeLight;
    public Animator endScreen;

    [Header("LoadingScreen")]
    public Animator loadingScreenAnim;

    [Header("Dialogue")]
    public Animator radioAnim;
    public Animator dialogueBoxAnim;
    [SerializeField] private List<string> dialogues;
    [SerializeField] private List<string> dialogues2;
    [SerializeField] private List<string> dialogues3;
    [SerializeField] private List<string> dialogues4;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private float standardTextDisplaySpeed = 0.05f;
    [HideInInspector] public float textDisplaySpeed;
    [HideInInspector] public int dialogueCount;
    public float maxTextDisplaySpeed = 0.00005f;
    private int dialogueTextCount;
    static float t;

    [Header("WeaponSwap")]
    public GameObject weaponDecisionWeaponImage;
    public TextMeshProUGUI weaponDecisionWeaponAbilityText;
    public TextMeshProUGUI weaponDecisionWeaponName;
    [SerializeField] public GameObject weaponSwapScreen;

    public static InGameUIManager Instance;
    
    [HideInInspector] public DialogueState dialogueState = DialogueState.DialogueNotPlaying;

    public enum DialogueState
    {
        DialogueNotPlaying,
        DialoguePlaying,
        DialogueAbleToGoNext,
        DialogueAbleToEnd,
    }
    
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        currencyUI = GetComponent<CurrencyUI>();
        equipWeaponButton.gameObject.SetActive(false);
        
        collectedCollectiblesDictionary = new Dictionary<string, (GameObject, Sprite, string, string)>();
        collectedWeapons = new Dictionary<string, (GameObject, Sprite, string, string, WeaponObjectSO)>();

        if(DebugMode.Instance.debugMode)
            loadingScreenAnim.SetTrigger("Start");
    }

    private void Update()
    {
        if (PlayerBehaviour.Instance != null)
        {
            if (PlayerBehaviour.Instance.canInteract)
            {
                //ShowInteractionIndicator(1);
            }
            else
            {
                //ShowInteractionIndicator(0.2156862745098039f);
            }
        }

        SimulateDayLight();
    }
    
    public void GoToMainMenu()
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

        dialogueCount = 0;
        dialogueTextCount = 0;
        dialogueText.text = "";
        radioAnim.Rebind();
        dialogueBoxAnim.Rebind();
        dialogueState = DialogueState.DialogueNotPlaying;
        StopAllCoroutines();

        inGameUIWeaponVisual.SetActive(false);
        
        fightScene.SetActive(false);
        
        abilityFillBar.SetActive(false);

        CloseInventory();
        GameSaveStateManager.Instance.GoToMainMenu();
    }
    
    public void PressButtonSound()
    {
        AudioManager.Instance.Play("ButtonClick");
    }
    
    private void SimulateDayLight()
    {
        if (changeLight)
        {
            t += 0.5f * Time.deltaTime;

            PlayerBehaviour.Instance.globalLightObject.gameObject.GetComponent<Light2D>().intensity = Mathf.Lerp( PlayerBehaviour.Instance.globalLightObject.gameObject.GetComponent<Light2D>().intensity, 1, t);
        }
    }

    private void EndScreen()
    {
        AudioManager.Instance.Stop("InGameMusic"); 
        inGameUIScreen.SetActive(false);
        endScreen.gameObject.SetActive(true);
        changeLight = true;
    }

    #region PlayerUI

    public void ActivateInGameUI()
    {
        inGameUIScreen.SetActive(true);
        
        if (GameSaveStateManager.Instance.startedNewGame)
        {
            ActivateRadio();
        }
    }

    public void SetGeneratorUI()
    {
        if (!PlayerBehaviour.Instance.isPlayerBusy)
        {
            generatorScreen.SetActive(true);
            
            return;
        }

        generatorScreen.SetActive(false);
    }

    public void SetShopUI()
    {
        if (dialogueState != DialogueState.DialogueNotPlaying)
        {
            return;
        }
        
        if (!PlayerBehaviour.Instance.isPlayerBusy)
        {
            shopScreen.SetActive(true);
            
            return;
        }

        shopScreen.SetActive(false);
    }

    #endregion

    #region DisplayInventoryInformation

    private void SetWeaponThroughInventory(WeaponObjectSO weaponObjectSO)
    {
        PlayerBehaviour.Instance.weaponBehaviour.GetWeapon(weaponObjectSO);
    }

    private void ResetDisplayInformation()
    {
        inventoryText.text = "";
        inventoryHeader.text = "";
        inventoryImage.gameObject.SetActive(false);
    }
    
    private void DisplayItem(Sprite sprite, string text, string header, WeaponObjectSO weaponObjectSO)
    {
        inventoryImage.gameObject.SetActive(true);
        inventoryText.text += text;
        inventoryHeader.text += header;
        inventoryImage.sprite = sprite;
        
        if (weaponObjectSO != null)
        {
            equipWeaponButton.gameObject.SetActive(true);
            equipWeaponButton.onClick.RemoveAllListeners();
            equipWeaponButton.onClick.AddListener(() => SetWeaponThroughInventory(weaponObjectSO));   
        }
    }
    
    public void DisplayPopCornPistol()
    {
        var _itemInformation = collectedWeapons["Popcorn Launcher"];
        
        ResetDisplayInformation();
        DisplayItem(_itemInformation.sprite, _itemInformation.text, _itemInformation.header, _itemInformation.weaponObjectSO);
    }
    
    public void DisplayMagnumMagnum()
    {
        var _itemInformation = collectedWeapons["Magnum magnum"];

        ResetDisplayInformation();
        DisplayItem(_itemInformation.sprite, _itemInformation.text, _itemInformation.header, _itemInformation.weaponObjectSO);
    }
    
    public void DisplayFrenchFriesAssaultRifle()
    {
        var _itemInformation = collectedWeapons["French Fries AR"];

        ResetDisplayInformation();
        DisplayItem(_itemInformation.sprite, _itemInformation.text, _itemInformation.header, _itemInformation.weaponObjectSO);
    }
    
    public void DisplayCornDogHuntingRifle()
    {
        var _itemInformation = collectedWeapons["Corn Dog Hunting Rifle"];

        ResetDisplayInformation();
        DisplayItem(_itemInformation.sprite, _itemInformation.text, _itemInformation.header, _itemInformation.weaponObjectSO);
    }
    
    public void DisplayLollipopShotgun()
    {
        var _itemInformation = collectedWeapons["Lollipop Shotgun"];

        ResetDisplayInformation();
        DisplayItem(_itemInformation.sprite, _itemInformation.text, _itemInformation.header, _itemInformation.weaponObjectSO);
    }
    
    public void DisplayTeddy()
    {
        var _itemInformation = collectedCollectiblesDictionary["Stuffed Animal"];

        ResetDisplayInformation();
        DisplayItem(_itemInformation.sprite, _itemInformation.text, _itemInformation.header, null);
    }
    
    public void DisplayNewspaper()
    {
        var _itemInformation = collectedCollectiblesDictionary["News Paper"];

        ResetDisplayInformation();
        DisplayItem(_itemInformation.sprite, _itemInformation.text, _itemInformation.header, null);
    }
    
    public void DisplayLights()
    {
        var _itemInformation = collectedCollectiblesDictionary["Broken Lights"];

        ResetDisplayInformation();
        DisplayItem(_itemInformation.sprite, _itemInformation.text, _itemInformation.header, null);
    }

    #endregion

    #region OnInventoryOpening
    
    public void OpenInventory(object sender, EventArgs e)
    {
        if (dialogueState != DialogueState.DialogueNotPlaying || PlayerBehaviour.Instance == null)
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

    private void CloseInventory()
    {
        inventory.SetActive(false);
        PlayerBehaviour.Instance.isPlayerBusy = false;
        EventSystem.current.SetSelectedGameObject(null);
        inventoryIsOpened = false;
        ResetDisplayInformation();
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
                    ActivateCollectible(brokenLights, _headerText, _spriteCollectible, _text);
                    break;
                case "News Paper" :
                    ActivateCollectible(newsPaper, _headerText, _spriteCollectible, _text);
                    break;
                case "Stuffed Animal" :
                    ActivateCollectible(teddy, _headerText, _spriteCollectible, _text);
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
                    ActivateWeapon(magnumMagnum, _headerText, _spriteWeapon, _text, _weapon);
                    break;
                case "French Fries AR" :
                    ActivateWeapon(frenchFriesAssaultRifle, _headerText, _spriteWeapon, _text,_weapon);
                    break;
                case "Lollipop Shotgun" :
                    ActivateWeapon(lollipopShotgun, _headerText, _spriteWeapon, _text, _weapon);
                    break;
                case "Corn Dog Hunting Rifle" :
                    ActivateWeapon(cornDogHuntingRifle, _headerText, _spriteWeapon, _text, _weapon);
                    break;
                case "Popcorn Launcher" :
                    ActivateWeapon(popcornPistol, _headerText, _spriteWeapon, _text, _weapon);
                    break;
            }
        }
    }
    
    private void ActivateWeapon(GameObject weapon, string headerText, Sprite spriteCollectible, string text, WeaponObjectSO weaponObjectSO)
    {
        if (!collectedWeapons.TryGetValue(headerText, out _))
        {
            collectedWeapons[headerText] = (weapon, spriteCollectible, text, headerText, weaponObjectSO); 
            weapon.SetActive(true);
        }
    }

    private void ActivateCollectible(GameObject collectible, string headerText, Sprite spriteCollectible, string text)
    {
        if (!collectedCollectiblesDictionary.TryGetValue(headerText, out _))
        {
            collectedCollectiblesDictionary[headerText] = (collectible, spriteCollectible, text, headerText); 
            collectible.SetActive(true);
        }
    }

    #endregion

    #region Dialogue
    
    private void DisplayDialogue(IReadOnlyList<string> currentDialogue)
    {
        StartCoroutine(TypeTextCoroutine(currentDialogue[dialogueTextCount], currentDialogue));
        
        dialogueTextCount++;
    }

    public void SetDialogueState()
    {
        switch (dialogueState)
        {
            case DialogueState.DialoguePlaying:
                Instance.textDisplaySpeed = Instance.maxTextDisplaySpeed;
                break;
            case DialogueState.DialogueAbleToGoNext:
                Instance.PlayNextDialogue();
                break;
            case DialogueState.DialogueAbleToEnd:
                Instance.EndDialogue();
                break;
            case DialogueState.DialogueNotPlaying:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    private IEnumerator TypeTextCoroutine(string text, IReadOnlyList<string> currentDialogue)
    {
        dialogueState = DialogueState.DialoguePlaying;
        dialogueText.text = "";
        dialogueText.textWrappingMode = TextWrappingModes.Normal;

        var _words = text.Split(' ');
        string _displayText = ""; 
        float _availableWidth = dialogueText.rectTransform.rect.width;

        foreach (var _word in _words)
        {
            string _testLine = _displayText + _word + " ";
            dialogueText.text = _testLine;
            dialogueText.ForceMeshUpdate();
            float _textWidth = dialogueText.preferredWidth;

            if (_textWidth > _availableWidth || _word.Contains("\n")) // If word doesn't fit, move to a new line before typing
            {
                _displayText += "\n";
            }

            // Update the text for each word, not each character, for performance reasons
            foreach (char _letter in _word + " ")
            {
                _displayText += _letter;
                dialogueText.text = _displayText;
                yield return new WaitForSeconds(textDisplaySpeed);
            }
        }
        
        //This checks for the current dialogue that is playing, whether they talked until the end or not
        if (dialogueTextCount == currentDialogue.Count - 1)
        {
            dialogueState = DialogueState.DialogueAbleToEnd;
        }
        else
        {
            dialogueState = DialogueState.DialogueAbleToGoNext;
        }
    }

    public void ActivateRadio()
    {
        if(DebugMode.Instance.debugMode)
            return;
        
        radioAnim.SetTrigger("RadioOn");
        AudioManager.Instance.Play("WalkieTalkie");
    }
    
    public void EndDialogue()
    {
        dialogueTextCount = 0;
        dialogueCount++;
        dialogueText.text = "";
        radioAnim.SetTrigger("RadioOff");
        dialogueBoxAnim.SetTrigger("DialogueBoxOff");
        dialogueState = DialogueState.DialogueNotPlaying;
        
        if (dialogueCount == 4)
        {
            EndScreen();
        }
    }

    public void PlayNextDialogue()
    {
        dialogueState = DialogueState.DialoguePlaying;
        dialogueText.text = "";
        textDisplaySpeed = standardTextDisplaySpeed;

        DisplayDialogueOnCount();
    }

    public void DisplayDialogueOnCount()
    {
        switch (dialogueCount)
        {
            case 0:
                DisplayDialogue(dialogues);
                break;
            case 1:
                DisplayDialogue(dialogues2);
                break;
            case 2:
                DisplayDialogue(dialogues3);
                break;
            case 3:
                DisplayDialogue(dialogues4);
                break;
        }
    }

    #endregion
}