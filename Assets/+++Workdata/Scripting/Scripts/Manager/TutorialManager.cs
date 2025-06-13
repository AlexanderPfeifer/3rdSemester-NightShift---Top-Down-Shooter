using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class TutorialManager : SingletonPersistent<TutorialManager>
{
    [HideInInspector] public int shotSigns;
    public int shotSignsToGoAhead = 5;

    [HideInInspector] public bool explainedRideSequences;

    [FormerlySerializedAs("openShutterWheelOfFortune")] [HideInInspector] public bool newWeaponsCanBeUnlocked;
    [HideInInspector] public bool fillAmmoForFree;
    [HideInInspector] public bool talkedAboutCurrency;
    private bool playedFirstDialogue;
    private bool openedShopAfterFirstFight;
    [HideInInspector] public bool tutorialDone;
    [SerializeField] private GameObject escapeInShop;
    private bool toldAboutAmmoRefill;

    [Header("QuestLogTexts")] 
    [SerializeField] private string fillAmmo;
    [SerializeField] private string activateGen;
    [SerializeField] public string activateRide;
    public string getNewWeapons;
    [SerializeField] private string doYourJob;

    [Header("Dialogue")] 
    [HideInInspector] public bool isExplainingCurrencyDialogue;

    protected override void Awake()
    {
        fillAmmoForFree = true;
        talkedAboutCurrency = false;
    }

    public void ExplainCurrency()
    {
        if (!talkedAboutCurrency)
        {
            isExplainingCurrencyDialogue = true;
            InGameUIManager.Instance.dialogueUI.SetDialogueBoxState(true, true);
            InGameUIManager.Instance.currencyUI.GetCurrencyText().gameObject.SetActive(true);
            talkedAboutCurrency = true;
        }    
    }

    public void CheckDialogue()
    {
        if (!playedFirstDialogue)
        {
            PlayStartingDialogue();
        }
        else if (Ride.Instance.GetCurrentWaveAsInt() > 0 && !openedShopAfterFirstFight)
        {
            FinishedFirstFight();
        }
        else if (shotSigns >= shotSignsToGoAhead && !toldAboutAmmoRefill)
        {
            InGameUIManager.Instance.dialogueUI.DisplayDialogue();         
            toldAboutAmmoRefill = true;
        }
        else if(!InGameUIManager.Instance.shopUI.fortuneWheel.activeSelf)
        {
            InGameUIManager.Instance.shopUI.ResetWeaponDescriptions();
        }
    }

    public void ExplainStartupSequences()
    {
        AudioManager.Instance.Play("RideShutDown");
        foreach (var _light in Ride.Instance.rideLight)
        {
            _light.SetActive(false);
        }
        StartCoroutine(WaitForShutdown());
        
        explainedRideSequences = true;
    }

    private IEnumerator WaitForShutdown()
    {
        while (AudioManager.Instance.IsPlaying("RideShutDown"))
        {
            yield return null;
        }
        
        InGameUIManager.Instance.generatorUI.gameObject.SetActive(true);

        yield return new WaitForSeconds(.5f);

        for (int i = 0; i < 6; i++)
        {
            foreach (var fuse in Ride.Instance.fuses)
            {
                fuse.sprite = (i % 2 == 0) ? Ride.Instance.DeactivateFuse() : Ride.Instance.ActivateFuse();
            }

            yield return new WaitForSeconds(0.3f);
        }


        yield return new WaitForSeconds(.5f);

        InGameUIManager.Instance.generatorUI.gameObject.SetActive(false);

        InGameUIManager.Instance.dialogueUI.SetDialogueBoxState(true, true);

        Ride.Instance.generator.gateAnim.SetBool("OpenGate", true);

        yield return null;
    }

    private void FinishedFirstFight()
    {
        InGameUIManager.Instance.shopUI.SetShopWindow();

        InGameUIManager.Instance.dialogueUI.shopText.text = "";
            
        InGameUIManager.Instance.dialogueUI.DisplayDialogue();

        openedShopAfterFirstFight = true;
        
        InGameUIManager.Instance.changeShopWindowButton.SetActive(true);
        InGameUIManager.Instance.shopUI.switchWindowButtons.SetActive(true);
        InGameUIManager.Instance.SetWalkieTalkieQuestLog(doYourJob);
        tutorialDone = true;
    }

    public void ExplainGenerator()
    {
        if (shotSigns >= shotSignsToGoAhead && Ride.Instance.GetCurrentWaveAsInt() == 0 && Ride.Instance.generator.interactable == false)
        {
            InGameUIManager.Instance.dialogueUI.DisplayDialogue();
            Ride.Instance.generator.interactable = true;
            InGameUIManager.Instance.SetWalkieTalkieQuestLog(activateGen);
            escapeInShop.SetActive(true);
        }
    }

    public void AddAndCheckShotSigns()
    {
        shotSigns++;
        
        if (shotSigns == shotSignsToGoAhead)
        {
            InGameUIManager.Instance.dialogueUI.SetDialogueBoxState(true, true);
            InGameUIManager.Instance.SetWalkieTalkieQuestLog(fillAmmo);
        }
    }
    
    private void PlayStartingDialogue()
    {
        InGameUIManager.Instance.dialogueUI.DisplayDialogue();
        playedFirstDialogue = true;
    }
    
    public void GetFirstWeaponAndWalkieTalkie()
    {
        if (!InGameUIManager.Instance.playerHUD.activeSelf)
        {
            InGameUIManager.Instance.playerHUD.SetActive(true);
        
            WeaponObjectSO _brokenPistol = PlayerBehaviour.Instance.weaponBehaviour.allWeaponPrizes.FirstOrDefault(w => w.weaponName == "Broken Pistol");
        
            PlayerBehaviour.Instance.weaponBehaviour.GetWeapon(_brokenPistol);
            InGameUIManager.Instance.weaponSlot.SetActive(true);
            
            InGameUIManager.Instance.CloseShop();

            InGameUIManager.Instance.dialogueUI.SetDialogueBoxState(true, true);   
        }
    }
}
