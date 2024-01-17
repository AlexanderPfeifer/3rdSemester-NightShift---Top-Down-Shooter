using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class InGameUI : MonoBehaviour
{
    public static InGameUI instance;
    
    [SerializeField] private TextMeshProUGUI inventoryText;
    [SerializeField] private TextMeshProUGUI inventoryHeader;
    [SerializeField] private Image inventoryImage;
    
    [Header("Collectibles")]
    private string brokenLightsText;
    private string teddyText;
    private string newsPaperText;
    private string brokenLightsHeader;
    private string teddyHeader;
    private string newsPaperHeader;
    private Sprite brokenLightsSprite;
    private Sprite teddySprite;
    private Sprite newsPaperSprite;

    [Header("Weapons")]
    private string popcornPistolText;
    private string frenchFriesAssaultRifleText;
    private string magnumMagnumText;
    private string cornDogHuntingRifleText;
    private string lollipopShotgunText;
    private string popcornPistolHeader;
    private string frenchFriesAssaultRifleHeader;
    private string magnumMagnumHeader;
    private string cornDogHuntingRifleHeader;
    private string lollipopShotgunHeader;
    private Sprite popcornPistolSprite;
    private Sprite frenchFriesAssaultRifleSprite;
    private Sprite magnumMagnumSprite;
    private Sprite cornDogHuntingRifleSprite;
    private Sprite lollipopShotgunSprite;

    [Header("PreviewObjects")]
    [SerializeField] private GameObject brokenLights;
    [SerializeField] private GameObject teddy;
    [SerializeField] private GameObject newsPaper;
    [SerializeField] private GameObject popcornPistol;
    [SerializeField] private GameObject frenchFriesAssaultRifle;
    [SerializeField] private GameObject magnumMagnum;
    [SerializeField] private GameObject cornDogHuntingRifle;
    [SerializeField] private GameObject lollipopShotgun;
    
    [SerializeField] private InGameCollectedObjects collectedObjects;
    [SerializeField] public GameObject fightScene;
    [SerializeField] private GameObject inventory;
    [SerializeField] private GameObject inGameUIScreen;
    [HideInInspector] public bool gameIsPaused;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [SerializeField] public GameObject pressSpace;
    
    private Player player;
    
    [SerializeField] private GameObject abilityProgress;
    [SerializeField] public GameObject rideTimeSlider;
    [SerializeField] public GameObject rideHpSlider;
    [SerializeField] public GameObject inventoryWeapon;

    public bool textIsPlaying;
    
    public bool canEndDialogue;
    
    public float textDisplaySpeed = 0.04f;

    [SerializeField] public Animator radioAnim;
    [SerializeField] private Animator dialogueBoxAnim;
    
    [SerializeField] GameObject eIndicator;
    private bool isplayerNotNull;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        GameSaveStateManager.instance.OnStateChanged += OnStateChange;
    }

    private void Update()
    {
        if (player != null)
        {
            UpdateAbilityProgress();

            if (player.playerCanInteract)
            {
                var componentColor = eIndicator.GetComponent<Image>().color;
                componentColor.a = 1;
                eIndicator.GetComponent<Image>().color = componentColor;
            }
            else
            {
                var componentColor = eIndicator.GetComponent<Image>().color;
                componentColor.a = 0.2156862745098039f;
                eIndicator.GetComponent<Image>().color = componentColor;
            }
        }
    }

    private void UpdateAbilityProgress()
    {
        abilityProgress.GetComponent<Slider>().value =
            player.currentAbilityProgress / player.maxAbilityProgress;
    }
    
    private void OnStateChange(GameSaveStateManager.GameState newState)
    {
        //we toggle the availability of the inGame menu whenever the game state changes
        //displayFirstDialogue = newState == GameSaveStateManager.GameState.InGame;
    }
    
    public void GoToMainMenu()
    {
        GameSaveStateManager.instance.GoToMainMenu();
        PauseGame();
    }

    //this is called via the "save game" button
    public void SaveGame()
    {
        GameSaveStateManager.instance.SaveGame();
    }

    private void DisplayNothing()
    {
        inventoryText.text = "";
        inventoryHeader.text = "";
        inventoryImage.gameObject.SetActive(true);
    }
    
    public void DisplayPopCornPistol()
    {
        DisplayNothing();
        inventoryText.text += popcornPistolText;
        inventoryHeader.text += popcornPistolHeader;
        inventoryImage.sprite = popcornPistolSprite;
    }
    
    public void DisplayMagnumMagnum()
    {
        DisplayNothing();
        inventoryText.text += magnumMagnumText;
        inventoryHeader.text += magnumMagnumHeader;
        inventoryImage.sprite = magnumMagnumSprite;
    }
    
    public void DisplayFrenchFriesAssaultRifle()
    {
        DisplayNothing();
        inventoryText.text += frenchFriesAssaultRifleText;
        inventoryHeader.text += frenchFriesAssaultRifleHeader;
        inventoryImage.sprite = frenchFriesAssaultRifleSprite;
    }
    
    public void DisplayCornDogHuntingRifle()
    {
        DisplayNothing();
        inventoryText.text += cornDogHuntingRifleText;
        inventoryHeader.text += cornDogHuntingRifleHeader;
        inventoryImage.sprite = cornDogHuntingRifleSprite;
    }
    
    public void DisplayLollipopShotgun()
    {
        DisplayNothing();
        inventoryText.text += lollipopShotgunText;
        inventoryHeader.text += lollipopShotgunHeader;
        inventoryImage.sprite = lollipopShotgunSprite;
    }
    
    public void DisplayTeddy()
    {
        DisplayNothing();
        inventoryText.text += teddyText;
        inventoryHeader.text += teddyHeader;
        inventoryImage.sprite = teddySprite;
    }
    
    public void DisplayNewspaper()
    {
        DisplayNothing();
        inventoryText.text += newsPaperText;
        inventoryHeader.text += newsPaperHeader;
        inventoryImage.sprite = newsPaperSprite;
    }
    
    public void DisplayLights()
    {
        DisplayNothing();
        inventoryText.text += brokenLightsText;
        inventoryHeader.text += brokenLightsHeader;
        inventoryImage.sprite = brokenLightsSprite;
    }

    private void DisplayCollectedLetters()
    {
        var collectedCollectibles = GameSaveStateManager.instance.saveGameDataManager.collectedCollectiblesIdentifiers;

        for (int index = 0; index < collectedCollectibles.Count; index++)
        {
            var text = "";
        
            var headerText = "";
            
            var collectible = collectedObjects.GetCollectibleDataByIdentifier(collectedCollectibles[index]);
            if (collectible == null)
                return;
            headerText += collectible.header;
            text += collectible.content;
            var spriteCollectible = collectible.icon;
            
            switch (headerText)
            {
                case "Broken Lights" :
                    brokenLights.SetActive(true);
                    brokenLightsSprite = spriteCollectible;
                    brokenLightsText = text;
                    brokenLightsHeader = headerText;
                    break;
                case "News Paper" :
                    newsPaper.SetActive(true);
                    newsPaperSprite = spriteCollectible;
                    newsPaperText = text;
                    newsPaperHeader = headerText;
                    break;
                case "Stuffed Animal" :
                    teddy.SetActive(true);
                    teddySprite = spriteCollectible;
                    teddyText = text;
                    teddyHeader = headerText;
                    break;
            }
            
        }
    }
    
    private void DisplayCollectedWeapons()
    {
        string text = "";
        
        string headerText = "";
        
        Sprite spriteWeapon = null;

        var collectedCollectibles = GameSaveStateManager.instance.saveGameDataManager.collectedWeaponsIdentifiers;

        for (int index = 0; index < collectedCollectibles.Count; index++)
        {
            var weapon = collectedObjects.GetWeaponDataByIdentifier(collectedCollectibles[index]);
            if (weapon == null)
                return;
            headerText += weapon.weaponName;
            text += weapon.weaponDescription;
            spriteWeapon = weapon.inGameWeaponVisual;
        }

        var itemIdentifier = headerText;

        switch (itemIdentifier)
        {
            case "Magnum magnum" :
                magnumMagnum.SetActive(true);
                magnumMagnumSprite = spriteWeapon;
                magnumMagnumText = text;
                magnumMagnumHeader = headerText;
                break;
            case "French Fries Assault Rifle" :
                frenchFriesAssaultRifle.SetActive(true);
                frenchFriesAssaultRifleSprite = spriteWeapon;
                frenchFriesAssaultRifleText = text;
                frenchFriesAssaultRifleHeader = headerText;
                break;
            case "Lollipop Shotgun" :
                lollipopShotgun.SetActive(true);
                lollipopShotgunSprite = spriteWeapon;
                lollipopShotgunText = text;
                lollipopShotgunHeader = headerText;
                break;
            case "Corn Dog Hunting Rifle" :
                cornDogHuntingRifle.SetActive(true);
                cornDogHuntingRifleSprite = spriteWeapon;
                cornDogHuntingRifleText = text;
                cornDogHuntingRifleHeader = headerText;
                break;
            case "Popcorn Pistol" :
                popcornPistol.SetActive(true);
                popcornPistolSprite = spriteWeapon;
                popcornPistolText = text;
                popcornPistolHeader = headerText;
                break;
        }
    }

    private IEnumerator DisplayFirstDialogue()
    {
        player.isPlayingDialogue = true;
        
        var dialogue = "Hey Looser, pass auf, dass die rides gut funktionieren nech!";

        yield return new WaitForSeconds(1);

        radioAnim.SetTrigger("RadioOn");

        yield return new WaitForSeconds(1);
        
        dialogueBoxAnim.SetTrigger("DialogueBoxOn");
        
        yield return new WaitForSeconds(0.24f);

        StartCoroutine(LetterByLetterTextCoroutine(dialogueText, dialogue));
    }
    
    private IEnumerator LetterByLetterTextCoroutine(TextMeshProUGUI textField, string text)
    {
        //an implementation for displaying a text letter by letter.
        string currentText = "";

        textIsPlaying = true;
        
        for (int index = 0; index < text.Length; index++)
        {
            currentText += text[index];
            textField.text = currentText;
            yield return new WaitForSeconds(textDisplaySpeed);
        }

        textIsPlaying = false;
        textDisplaySpeed = 0.04f;
        canEndDialogue = true;
    }

    public void EndDialogue()
    {
        dialogueText.text = "";
        radioAnim.SetTrigger("RadioOff");
        dialogueBoxAnim.SetTrigger("DialogueBoxOff");
        canEndDialogue = false;
        player.isPlayingDialogue = false;
    }
    
    public void PauseGame()
    {
        if (gameIsPaused)
        {
            gameIsPaused = false;
            inventory.SetActive(false);
        }
        else
        {
            gameIsPaused = true;
            inventory.SetActive(true);
            DisplayCollectedLetters();
            DisplayCollectedWeapons();
        }
    }

    public void ActivateInGameUI()
    {
        inGameUIScreen.SetActive(true);
        player = FindObjectOfType<Player>();
        if (GameSaveStateManager.instance.startedNewGame)
        {
            StartCoroutine(DisplayFirstDialogue());
        }

        GameSaveStateManager.instance.startedNewGame = false;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
