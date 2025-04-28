using System.Linq;
using UnityEngine;

public class TutorialManager : SingletonPersistent<TutorialManager>
{
    private int shotSigns;

    [HideInInspector] public bool openShutterWheelOfFortune;
    [HideInInspector] public bool fillAmmoForFree;
    [HideInInspector] public bool talkedAboutCurrency;
    [HideInInspector] public bool canActivateGenerator;

    protected override void Awake()
    {
        fillAmmoForFree = true;
        talkedAboutCurrency = false;
        canActivateGenerator = false;
    }

    public void AddAndCheckShotSigns()
    {
        shotSigns++;
        
        if (shotSigns >= 3)
        {
            InGameUIManager.Instance.dialogueUI.SetRadioState(true, true);
        }
    }
    
    public void GetFirstWeaponAndWalkieTalkie()
    {
        WeaponObjectSO _brokenPistol = PlayerBehaviour.Instance.weaponBehaviour.allWeaponPrizes.FirstOrDefault(w => w.weaponName == "Broken Pistol");
        
        PlayerBehaviour.Instance.weaponBehaviour.GetWeapon(_brokenPistol);
        GameSaveStateManager.Instance.saveGameDataManager.AddWeapon(_brokenPistol.weaponName);
        InGameUIManager.Instance.weaponSlot.SetActive(true);
        
        InGameUIManager.Instance.dialogueUI.dialogueBoxAnim.SetBool("DialogueBoxOn", false);

        InGameUIManager.Instance.dialogueUI.SetRadioState(true, true);
        
        InGameUIManager.Instance.shopScreen.SetActive(false);
    }
}
