using System.Linq;
using UnityEngine;

public class TutorialManager : SingletonPersistent<TutorialManager>
{
    [HideInInspector] public int shotSigns;

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
            InGameUIManager.Instance.dialogueUI.SetDialogueBoxState(true, true);
        }
    }
    
    public void GetFirstWeaponAndWalkieTalkie()
    {
        WeaponObjectSO _brokenPistol = PlayerBehaviour.Instance.weaponBehaviour.allWeaponPrizes.FirstOrDefault(w => w.weaponName == "Broken Pistol");
        
        PlayerBehaviour.Instance.weaponBehaviour.GetWeapon(_brokenPistol);
        InGameUIManager.Instance.weaponSlot.SetActive(true);
        
        InGameUIManager.Instance.SetShopUI();
        
        InGameUIManager.Instance.dialogueUI.SetDialogueBoxState(true, true);
    }
}
