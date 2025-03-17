using System.Collections;
using System.Collections.Generic;
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
    [FormerlySerializedAs("collectedObjects")] [SerializeField] private CollectedWeaponsSO collectedWeaponsSO;

    [Header("Collectibles")]
    private string brokenLightsText, teddyText, newsPaperText;
    private string brokenLightsHeader, teddyHeader, newsPaperHeader;
    private Sprite brokenLightsSprite, teddySprite, newsPaperSprite;

    [Header("Weapons")] 
    public TextMeshProUGUI ammunitionInClipText;
    public TextMeshProUGUI ammunitionInBackUpText;
    private string popcornPistolText, frenchFriesAssaultRifleText, magnumMagnumText, cornDogHuntingRifleText, lollipopShotgunText;
    private string popcornPistolHeader, frenchFriesAssaultRifleHeader, magnumMagnumHeader, cornDogHuntingRifleHeader, lollipopShotgunHeader;
    private Sprite popcornPistolSprite, frenchFriesAssaultRifleSprite, magnumMagnumSprite, cornDogHuntingRifleSprite, lollipopShotgunSprite;

    private Dictionary<string, (GameObject obj, Sprite sprite, string text, string header)> collectedItems;

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
    [FormerlySerializedAs("inventoryWeapon")] public GameObject equippedWeapon;
    public GameObject fightScene;
    public GameObject pressSpace;
    public Image rideTimeImage;
    public Image rideHpImage;
    public Image abilityProgressImage;
    
    [Header("Player UI")]
    [SerializeField] private GameObject eIndicator;
    [SerializeField] private GameObject inventoryButton; 
    public GameObject inGameUIScreen;
    public GameObject firstInventorySelected;
    public GameObject shopScreen;
    public GameObject generatorScreen;
    
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
    public GameObject firstSelectedWeaponDecision;
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
        collectedItems = new Dictionary<string, (GameObject, Sprite, string, string)>
        {
            { "Broken Lights", (brokenLights, brokenLightsSprite, brokenLightsText, brokenLightsHeader) },
            { "News Paper", (newsPaper, newsPaperSprite, newsPaperText, newsPaperHeader) },
            { "Stuffed Animal", (teddy, teddySprite, teddyText, teddyHeader) },
            { "Magnum magnum", (magnumMagnum, magnumMagnumSprite, magnumMagnumText, magnumMagnumHeader) },
            { "French Fries AR", (frenchFriesAssaultRifle, frenchFriesAssaultRifleSprite, frenchFriesAssaultRifleText, frenchFriesAssaultRifleHeader) },
            { "Lollipop Shotgun", (lollipopShotgun, lollipopShotgunSprite, lollipopShotgunText, lollipopShotgunHeader) },
            { "Corn Dog Hunting Rifle", (cornDogHuntingRifle, cornDogHuntingRifleSprite, cornDogHuntingRifleText, cornDogHuntingRifleHeader) },
            { "Popcorn Pistol", (popcornPistol, popcornPistolSprite, popcornPistolText, popcornPistolHeader) }
        };
        
        if(DebugMode.Instance.debugMode)
            loadingScreenAnim.SetTrigger("Start");
    }

    private void Update()
    {
        if (Player.Instance != null)
        {
            if (Player.Instance.canInteract)
            {
                ShowInteractionIndicator(1);
            }
            else
            {
                ShowInteractionIndicator(0.2156862745098039f);
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

        dialogueCount = 0;
        dialogueTextCount = 0;
        dialogueText.text = "";
        radioAnim.Rebind();
        dialogueBoxAnim.Rebind();
        dialogueState = DialogueState.DialogueNotPlaying;
        StopAllCoroutines();

        equippedWeapon.SetActive(false);
        
        fightScene.SetActive(false);
        
        GameSaveStateManager.Instance.GoToMainMenu();
        OpenInventory();
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

            Player.Instance.globalLightObject.gameObject.GetComponent<Light2D>().intensity = Mathf.Lerp( Player.Instance.globalLightObject.gameObject.GetComponent<Light2D>().intensity, 1, t);
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

    private void ShowInteractionIndicator(float alpha)
    {
        var _componentColor = eIndicator.GetComponent<Image>().color;
        _componentColor.a = alpha;
        eIndicator.GetComponent<Image>().color = _componentColor;
    }

    public void ActivateInGameUI()
    {
        inGameUIScreen.SetActive(true);
        
        if (GameSaveStateManager.Instance.startedNewGame)
        {
            ActivateRadio();
        }
    }

    #endregion

    #region DisplayInventoryInformation

    private void ResetDisplayInformation()
    {
        inventoryText.text = "";
        inventoryHeader.text = "";
        inventoryImage.gameObject.SetActive(true);
    }
    
    private void DisplayItem(Sprite sprite, string text, string header)
    {
        inventoryText.text += text;
        inventoryHeader.text += header;
        inventoryImage.sprite = sprite;
    }
    
    public void DisplayPopCornPistol()
    {
        var _itemInformation = collectedItems["Popcorn Pistol"];
        
        ResetDisplayInformation();
        DisplayItem(_itemInformation.sprite, _itemInformation.text, _itemInformation.header);
    }
    
    public void DisplayMagnumMagnum()
    {
        var _itemInformation = collectedItems["Magnum magnum"];

        ResetDisplayInformation();
        DisplayItem(_itemInformation.sprite, _itemInformation.text, _itemInformation.header);
    }
    
    public void DisplayFrenchFriesAssaultRifle()
    {
        var _itemInformation = collectedItems["French Fries AR"];

        ResetDisplayInformation();
        DisplayItem(_itemInformation.sprite, _itemInformation.text, _itemInformation.header);
    }
    
    public void DisplayCornDogHuntingRifle()
    {
        var _itemInformation = collectedItems["Corn Dog Hunting Rifle"];

        ResetDisplayInformation();
        DisplayItem(_itemInformation.sprite, _itemInformation.text, _itemInformation.header);
    }
    
    public void DisplayLollipopShotgun()
    {
        var _itemInformation = collectedItems["Lollipop Shotgun"];

        ResetDisplayInformation();
        DisplayItem(_itemInformation.sprite, _itemInformation.text, _itemInformation.header);
    }
    
    public void DisplayTeddy()
    {
        var _itemInformation = collectedItems["Stuffed Animal"];

        ResetDisplayInformation();
        DisplayItem(_itemInformation.sprite, _itemInformation.text, _itemInformation.header);
    }
    
    public void DisplayNewspaper()
    {
        var _itemInformation = collectedItems["News Paper"];

        ResetDisplayInformation();
        DisplayItem(_itemInformation.sprite, _itemInformation.text, _itemInformation.header);
    }
    
    public void DisplayLights()
    {
        var _itemInformation = collectedItems["Broken Lights"];

        ResetDisplayInformation();
        DisplayItem(_itemInformation.sprite, _itemInformation.text, _itemInformation.header);
    }

    #endregion

    #region OnInventoryOpening
    
    public void OpenInventory()
    {
        if (inventoryIsOpened)
        {
            inventoryButton.SetActive(true);
            inventory.SetActive(false);
            EventSystem.current.SetSelectedGameObject(null);
            inventoryIsOpened = false;
        }
        else
        {
            inventoryButton.SetActive(false);
            inventory.SetActive(true);
            DisplayCollectedCollectibles();
            DisplayCollectedWeapons();
            EventSystem.current.SetSelectedGameObject(firstInventorySelected);
            inventoryIsOpened = true;
        }
    }

    private void DisplayCollectedCollectibles()
    {
        var _collectedCollectibles = GameSaveStateManager.Instance.saveGameDataManager.collectedCollectiblesIdentifiers;

        foreach (var _identifier in _collectedCollectibles)
        {
            var _text = "";
            var _headerText = "";
            var _collectible = collectedWeaponsSO.GetCollectibleDataByIdentifier(_identifier);
            
            if (_collectible == null)
                return;
            
            _headerText += _collectible.header;
            _text += _collectible.content;
            var _spriteCollectible = _collectible.icon;
            
            switch (_headerText)
            {
                case "Broken Lights" :
                    ActivateCollectible(_headerText, _spriteCollectible, _text);
                    break;
                case "News Paper" :
                    ActivateCollectible(_headerText, _spriteCollectible, _text);
                    break;
                case "Stuffed Animal" :
                    ActivateCollectible(_headerText, _spriteCollectible, _text);
                    break;
            }
        }
    }
    
    private void DisplayCollectedWeapons()
    {
        var _collectedCollectibles = GameSaveStateManager.Instance.saveGameDataManager.collectedWeaponsIdentifiers;

        foreach (var _character in _collectedCollectibles)
        {
            var _headerText = "";
            var _text = "";
            var _weapon = collectedWeaponsSO.GetWeaponDataByIdentifier(_character);
            
            if (_weapon == null)
                return;
            
            _headerText += _weapon.weaponName;
            _text += _weapon.weaponDescription;
            _text += "\n" + "\n" + "Special Ability:" + "\n" + _weapon.weaponAbilityDescription + "\n" + "\n" +
                     "Damage: " + _weapon.bulletDamage +  "\n" + "Clipsize: " + _weapon.clipSize;
            var _spriteWeapon = _weapon.inGameWeaponVisual;
            
            var _itemIdentifier = _headerText;
            
            switch (_itemIdentifier)
            {
                case "Magnum magnum" :
                    ActivateCollectible(_headerText, _spriteWeapon, _text);
                    break;
                case "French Fries AR" :
                    ActivateCollectible(_headerText, _spriteWeapon, _text);
                    break;
                case "Lollipop Shotgun" :
                    ActivateCollectible(_headerText, _spriteWeapon, _text);
                    break;
                case "Corn Dog Hunting Rifle" :
                    ActivateCollectible(_headerText, _spriteWeapon, _text);
                    break;
                case "Popcorn Pistol" :
                    ActivateCollectible(_headerText, _spriteWeapon, _text);
                    break;
            }
        }
    }

    private void ActivateCollectible(string headerText, Sprite spriteCollectible, string text)
    {
        if (collectedItems.TryGetValue(headerText, out var _collectedObject))
        {
            _collectedObject.obj.SetActive(true);
            collectedItems[headerText] = (_collectedObject.obj, spriteCollectible, text, headerText);
        }
    }

    #endregion

    #region Dialogue
    
    private void DisplayDialogue(IReadOnlyList<string> currentDialogue)
    {
        StartCoroutine(TypeTextCoroutine(currentDialogue[dialogueTextCount], currentDialogue));
        
        dialogueTextCount++;
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
