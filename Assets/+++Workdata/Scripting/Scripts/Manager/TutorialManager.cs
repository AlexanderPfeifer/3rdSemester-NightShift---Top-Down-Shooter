using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class TutorialManager : SingletonPersistent<TutorialManager>
{
    [HideInInspector] public int shotSigns;
    private int hitSigns;

    [HideInInspector] public bool explainedRideSequences;

    [FormerlySerializedAs("openShutterWheelOfFortune")] [HideInInspector] public bool newWeaponsCanBeUnlocked;
    [HideInInspector] public bool fillAmmoForFree;
    [HideInInspector] public bool talkedAboutCurrency;
    private bool playedFirstDialogue;
    private bool openedShopAfterFirstFight;
    [HideInInspector] public bool tutorialDone;

    [Header("QuestLogTexts")] 
    [SerializeField] private string fillAmmo;
    [SerializeField] private string hitSignsQuestLog;
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
        else if(!InGameUIManager.Instance.shopUI.fortuneWheel.activeSelf)
        {
            InGameUIManager.Instance.shopUI.ResetWeaponDescriptions();
        }
    }

    public void ExplainStartupSequences()
    {
        AudioManager.Instance.Play("RideShutDown");
        Ride.Instance.rideLight.SetActive(false);

        StartCoroutine(WaitForShutdown());
        
        explainedRideSequences = true;
    }

    private IEnumerator WaitForShutdown()
    {
        while (AudioManager.Instance.IsPlaying("RideShutDown"))
        {
            yield return null;
        }
        
        Ride.Instance.generator.gateAnim.SetBool("OpenGate", true);
        
        InGameUIManager.Instance.dialogueUI.SetDialogueBoxState(true, true);

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
        if (hitSigns >= 1 && Ride.Instance.GetCurrentWaveAsInt() == 0 && Ride.Instance.generator.interactable == false)
        {
            InGameUIManager.Instance.dialogueUI.DisplayDialogue();
            Ride.Instance.generator.interactable = true;
            InGameUIManager.Instance.SetWalkieTalkieQuestLog(activateGen);
        }
    }
    
    public void AddAndCheckHitSigns()
    {
        if (shotSigns >= 3)
        {
            hitSigns++;
        
            if (hitSigns == 1)
            {
                InGameUIManager.Instance.dialogueUI.SetDialogueBoxState(true, true);
                InGameUIManager.Instance.SetWalkieTalkieQuestLog(fillAmmo);
            }   
        }
    }

    public void AddAndCheckShotSigns()
    {
        shotSigns++;
        
        if (shotSigns == 3)
        {
            InGameUIManager.Instance.dialogueUI.SetDialogueBoxState(true, true);
            InGameUIManager.Instance.SetWalkieTalkieQuestLog(hitSignsQuestLog);
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
