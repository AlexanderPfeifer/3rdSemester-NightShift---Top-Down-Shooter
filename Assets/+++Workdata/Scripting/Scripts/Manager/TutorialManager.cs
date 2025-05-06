using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class TutorialManager : SingletonPersistent<TutorialManager>
{
    [HideInInspector] public int shotSigns;

    [FormerlySerializedAs("openShutterWheelOfFortune")] [HideInInspector] public bool newWeaponsCanBeUnlocked;
    [HideInInspector] public bool fillAmmoForFree;
    [HideInInspector] public bool talkedAboutCurrency;

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

    public void ExplainGenerator()
    {
        if (fillAmmoForFree && talkedAboutCurrency == false)
        {
            InGameUIManager.Instance.dialogueUI.DisplayDialogue();
        }
    }

    public void AddAndCheckShotSigns()
    {
        shotSigns++;
        
        if (shotSigns >= 3)
        {
            InGameUIManager.Instance.dialogueUI.SetDialogueBoxState(true, true);
        }
    }

    public void MakeNewWeaponsUnlockable()
    {
        if (newWeaponsCanBeUnlocked)
        {
            //Todo: Make this an animation of the shutter
            InGameUIManager.Instance.changeShopWindowButton.SetActive(true);
            InGameUIManager.Instance.shopUI.switchWindowButtons.SetActive(true);
        }
    }
    
    public void PlayStartingDialogue()
    {
        if (shotSigns < 3 || !fillAmmoForFree)
        {
            InGameUIManager.Instance.dialogueUI.DisplayDialogue();
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
        
            InGameUIManager.Instance.SetShopUI();
        
            InGameUIManager.Instance.dialogueUI.SetDialogueBoxState(true, true);   
        }
    }
}
