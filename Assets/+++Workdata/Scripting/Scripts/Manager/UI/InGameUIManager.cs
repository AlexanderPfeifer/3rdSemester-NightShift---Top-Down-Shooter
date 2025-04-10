using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

public class InGameUIManager : MonoBehaviour
{
    [Header("Ammunition")] 
    public TextMeshProUGUI ammunitionInClipText;
    public TextMeshProUGUI ammunitionInBackUpText;

    [Header("Weapon")] 
    public GameObject weaponSlot;
    public GameObject inGameUIWeaponVisual;
    
    [Header("Ride")]
    public Image rideTimeImage;
    public Image rideHpImage;
    
    [Header("Ability")]
    public GameObject pressSpace;
    public Image abilityProgressImage;
    public GameObject abilityFillBar;
    
    [Header("UI Screens")]
    public GameObject fightUI;
    [FormerlySerializedAs("inGameUIScreen")] public GameObject playerHUD;
    [SerializeField] private GameObject shopScreen;
    [SerializeField] private GameObject generatorScreen;
    [SerializeField] public GameObject weaponSwapScreen;
    
    [Header("End Sequence")]
    [HideInInspector] public bool changeLight;
    public Animator endScreen;
    static float t;
    
    [Header("References")]
    [HideInInspector] public CurrencyUI currencyUI;
    [HideInInspector] public DialogueUI dialogueUI;
    [HideInInspector] public InventoryUI inventoryUI;

    [Header("WeaponSwap")]
    public GameObject weaponDecisionWeaponImage;
    public TextMeshProUGUI weaponDecisionWeaponAbilityText;
    public TextMeshProUGUI weaponDecisionWeaponName;

    public static InGameUIManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        currencyUI = GetComponent<CurrencyUI>();
        dialogueUI = GetComponent<DialogueUI>();
        inventoryUI = GetComponent<InventoryUI>();
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
        inventoryUI.ResetInventoryElements();
        dialogueUI.ResetDialogueElements();
        StopAllCoroutines();

        inGameUIWeaponVisual.SetActive(false);
        
        fightUI.SetActive(false);
        
        abilityFillBar.SetActive(false);

        inventoryUI.CloseInventory();
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

    public void EndScreen()
    {
        AudioManager.Instance.Stop("InGameMusic"); 
        playerHUD.SetActive(false);
        endScreen.gameObject.SetActive(true);
        changeLight = true;
    }

    public void SetGeneratorUI()
    {
        if (!PlayerBehaviour.Instance.isPlayerBusy && TutorialManager.Instance.canActivateGenerator)
        {
            if (!TutorialManager.Instance.talkedAboutCurrency)
            {
                dialogueUI.SetRadioState(true, true);
                TutorialManager.Instance.talkedAboutCurrency = true;
                return;
            }
            
            generatorScreen.SetActive(true);
        }
        else
        {
            generatorScreen.SetActive(false);
        }
    }

    public void SetShopUI()
    {
        if (dialogueUI.IsDialoguePlaying())
        {
            return;
        }
        
        if (!PlayerBehaviour.Instance.isPlayerBusy)
        {
            shopScreen.SetActive(true);
            
            playerHUD.SetActive(true);

            if (dialogueUI.dialogueCount <= 0)
            {
                dialogueUI.dialogueBoxAnim.SetBool("DialogueBoxOn", true);
                //Add the normal dialogue box instead
            }
            
            return;
        }

        dialogueUI.SetRadioState(false, true);
        shopScreen.SetActive(false);
    }
}