using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class TutorialManager : SingletonPersistent<TutorialManager>
{
    [HideInInspector] public int shotSigns;

    [FormerlySerializedAs("openShutterWheelOfFortune")] [HideInInspector] public bool newWeaponsCanBeUnlocked;
    [HideInInspector] public bool fillAmmoForFree;
    [HideInInspector] public bool talkedAboutCurrency;
    private bool playedFirstDialogue;

    protected override void Awake()
    {
        fillAmmoForFree = true;
        talkedAboutCurrency = false;
    }

    public void ExplainCurrency()
    {
        if (!talkedAboutCurrency)
        {
            InGameUIManager.Instance.dialogueUI.SetDialogueBoxState(true, true);
            InGameUIManager.Instance.currencyUI.GetCurrencyText().gameObject.SetActive(true);
            talkedAboutCurrency = true;
        }    
    }

    public void CheckDialogue()
    {
        MakeNewWeaponsUnlockable();
            
        PlayStartingDialogue();

        FinishedFirstFight();
    }

    private void FinishedFirstFight()
    {
        if (Ride.Instance.GetCurrentWaveAsInt() > 0)
        {
            InGameUIManager.Instance.dialogueUI.DisplayDialogue();
        }
    }

    public void ExplainGenerator()
    {
        if (shotSigns >= 3 && Ride.Instance.GetCurrentWaveAsInt() == 0 && Ride.Instance.generator.interactable == false)
        {
            InGameUIManager.Instance.dialogueUI.DisplayDialogue();
            Ride.Instance.generator.interactable = true;
        }
    }

    public void AddAndCheckShotSigns()
    {
        shotSigns++;
        
        if (shotSigns == 3)
        {
            InGameUIManager.Instance.dialogueUI.SetDialogueBoxState(true, true);
        }
    }

    private void MakeNewWeaponsUnlockable()
    {
        if (newWeaponsCanBeUnlocked)
        {
            //Todo: Make this an animation of the shutter
            InGameUIManager.Instance.changeShopWindowButton.SetActive(true);
            InGameUIManager.Instance.shopUI.switchWindowButtons.SetActive(true);
        }
    }
    
    private void PlayStartingDialogue()
    {
        if (!playedFirstDialogue)
        {
            InGameUIManager.Instance.dialogueUI.DisplayDialogue();
            playedFirstDialogue = true;
        }
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
