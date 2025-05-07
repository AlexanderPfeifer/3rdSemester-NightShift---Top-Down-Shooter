using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

public class InGameUIManager : SingletonPersistent<InGameUIManager>
{
    [Header("Ammunition")] 
    public TextMeshProUGUI ammunitionInClipText;
    public TextMeshProUGUI ammunitionInBackUpText;

    [Header("Weapon")] 
    public GameObject weaponSlot;
    public GameObject inGameUIWeaponVisual;
    
    [Header("Ride")]
    public TextMeshProUGUI rideTimeText;
    public Image rideHpImage;
    
    [Header("Ability")]
    public GameObject pressSpace;
    public Image abilityProgressImage;
    public GameObject abilityFillBar;

    [Header("Shop")] 
    public GameObject changeShopWindowButton;
    
    [Header("UI Screens")]
    public GameObject fightUI;
    public GameObject weaponSwapScreen;
    [FormerlySerializedAs("inGameUIScreen")] public GameObject playerHUD;
    public GameObject shopScreen;
    [SerializeField] private GameObject generatorScreen;
    
    [Header("End Sequence")]
    [HideInInspector] public bool changeLight;
    public Animator endScreen;
    static float t;
    
    [Header("References")]
    public GeneratorUI generatorUI;
    [HideInInspector] public CurrencyUI currencyUI;
    [HideInInspector] public DialogueUI dialogueUI;
    [FormerlySerializedAs("weaponDescriptionUI")] [FormerlySerializedAs("inventoryUI")] [HideInInspector] public ShopUI shopUI;
    [HideInInspector] public PauseMenuUI pauseMenuUI;

    [Header("WeaponSwap")]
    public GameObject weaponDecisionWeaponImage;
    public TextMeshProUGUI weaponDecisionWeaponAbilityText;
    public TextMeshProUGUI weaponDecisionWeaponName;
    
    private void Start()
    {
        currencyUI = GetComponent<CurrencyUI>();
        dialogueUI = GetComponent<DialogueUI>();
        shopUI = GetComponent<ShopUI>();
    }

    private void Update()
    {
        SimulateDayLight();
    }
    
    public void GoToMainMenu()
    {
        shopUI.ResetShopElements();
        dialogueUI.ResetDialogueElements();
        StopAllCoroutines();

        inGameUIWeaponVisual.SetActive(false);
        
        fightUI.SetActive(false);
        
        abilityFillBar.SetActive(false);

        pauseMenuUI.CloseInventory();
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
        if (!PlayerBehaviour.Instance.IsPlayerBusy())
        {
            if (!TutorialManager.Instance.talkedAboutCurrency)
            {
                TutorialManager.Instance.ExplainCurrency();
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
        
        if (!PlayerBehaviour.Instance.IsPlayerBusy())
        {
            shopScreen.SetActive(true);
            
            
            
            AudioManager.Instance.FadeOut("InGameMusic", "ShopMusic");

            foreach (GameObject _enemy in Ride.Instance.enemyParent.transform)
            {
                Destroy(_enemy);
            }

            dialogueUI.SetDialogueBox(true); 
            dialogueUI.SetDialogueBoxState(false, false);

            PlayerBehaviour.Instance.SetPlayerBusy(true);

            TutorialManager.Instance.CheckDialogue();
                
            shopUI.DisplayCollectedWeapons();
            
            return;
        }

        dialogueUI.SetDialogueBox(false);
        dialogueUI.SetDialogueBoxState(false, true);

        shopScreen.SetActive(false);
        
        AudioManager.Instance.FadeOut("ShopMusic", "InGameMusic");

        PlayerBehaviour.Instance.SetPlayerBusy(false);
    }
}