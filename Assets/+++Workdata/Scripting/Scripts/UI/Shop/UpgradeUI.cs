using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeUI : MonoBehaviour
{
    [Header("Upgraded Weapons")] 
    [SerializeField] private WeaponObjectSO[] lollipopShotgunUpgradeTiers;
    [SerializeField] private WeaponObjectSO[] assaultRifleUpgradeTiers;
    [SerializeField] private WeaponObjectSO[] magnumMagnumUpgradeTiers;
    [SerializeField] private WeaponObjectSO[] huntingRifleUpgradeTiers;
    [SerializeField] private WeaponObjectSO[] popcornLauncherUpgradeTiers;

    [Header("Upgrade Buttons")]
    [SerializeField] private Button lollipopShotgunUpgradeButton;
    [SerializeField] private Button assaultRifleUpgradeButton;
    [SerializeField] private Button magnumMagnumUpgradeButton;
    [SerializeField] private Button huntingRifleUpgradeButton;
    [SerializeField] private Button popcornLauncherUpgradeButton;
    [SerializeField] private Button brokenPistolUpgradeButton;
    
    [Header("Fill Ammo Buttons")]
    [SerializeField] private Button lollipopShotgunFillAmmoButton;
    [SerializeField] private Button assaultRifleFillAmmoButton;
    [SerializeField] private Button magnumMagnumFillAmmoButton;
    [SerializeField] private Button huntingRifleFillAmmoButton;
    [SerializeField] private Button popcornLauncherFillAmmoButton;
    [SerializeField] private Button brokenPistolFillAmmoButton;

    [Header("Costs")] 
    [SerializeField] private int[] tierCosts;
    [SerializeField] private int fillAmmoCost;

    private void OnEnable()
    {
        SetUpgradeButtons();
        PlayerBehaviour.Instance.isPlayerBusy = true;
    }

    public void SetUpgradeButtons()
    {
        foreach (var _weapon in PlayerBehaviour.Instance.weaponBehaviour.allWeaponPrizes.Where(weapon => GameSaveStateManager.Instance.saveGameDataManager.HasWeapon(weapon.weaponName)))
        {
            switch (_weapon.weaponName)
            {
                case "Magnum magnum" :
                    magnumMagnumUpgradeButton.gameObject.SetActive(true);
                    magnumMagnumFillAmmoButton.gameObject.SetActive(true);
                    magnumMagnumUpgradeButton.onClick.AddListener(() => UpgradeWeapon(_weapon, magnumMagnumUpgradeTiers));
                    magnumMagnumFillAmmoButton.onClick.AddListener(() => FillWeaponAmmo(_weapon));
                    break;
                            
                case "French Fries AR" :
                    assaultRifleUpgradeButton.gameObject.SetActive(true);
                    assaultRifleFillAmmoButton.gameObject.SetActive(true);
                    assaultRifleUpgradeButton.onClick.AddListener(() => UpgradeWeapon(_weapon, assaultRifleUpgradeTiers));
                    assaultRifleFillAmmoButton.onClick.AddListener(() => FillWeaponAmmo(_weapon));
                    break;
                            
                case "Lollipop Shotgun" :
                    lollipopShotgunUpgradeButton.gameObject.SetActive(true);
                    lollipopShotgunFillAmmoButton.gameObject.SetActive(true);
                    lollipopShotgunUpgradeButton.onClick.AddListener(() => UpgradeWeapon(_weapon, lollipopShotgunUpgradeTiers));
                    lollipopShotgunFillAmmoButton.onClick.AddListener(() => FillWeaponAmmo(_weapon));
                    break;
                            
                case "Corn Dog Hunting Rifle" :
                    huntingRifleUpgradeButton.gameObject.SetActive(true);
                    huntingRifleFillAmmoButton.gameObject.SetActive(true);
                    huntingRifleUpgradeButton.onClick.AddListener(() => UpgradeWeapon(_weapon, huntingRifleUpgradeTiers));
                    huntingRifleFillAmmoButton.onClick.AddListener(() => FillWeaponAmmo(_weapon));
                    break;
                            
                case "Popcorn Launcher" :
                    popcornLauncherUpgradeButton.gameObject.SetActive(true);
                    popcornLauncherFillAmmoButton.gameObject.SetActive(true);
                    popcornLauncherUpgradeButton.onClick.AddListener(() => UpgradeWeapon(_weapon, popcornLauncherUpgradeTiers));
                    popcornLauncherFillAmmoButton.onClick.AddListener(() => FillWeaponAmmo(_weapon));
                    break;
                            
                case "Broken Pistol" :
                    brokenPistolUpgradeButton.gameObject.SetActive(true);
                    brokenPistolFillAmmoButton.gameObject.SetActive(true);
                    brokenPistolFillAmmoButton.onClick.AddListener(() => FillWeaponAmmo(_weapon));
                    break;
            }
        }
    }

    private void UpgradeWeapon(WeaponObjectSO weapon, IReadOnlyList<WeaponObjectSO> upgradeTiers)
    {
        int _currentTierOnUpgradingWeapon = weapon.upgradeTier;

        if (_currentTierOnUpgradingWeapon < upgradeTiers.Count && PlayerBehaviour.Instance.playerCurrency.SpendCurrency(tierCosts[_currentTierOnUpgradingWeapon]))
        {
            for (int _i = 0; _i < PlayerBehaviour.Instance.weaponBehaviour.allWeaponPrizes.Count; _i++)
            {
                if (PlayerBehaviour.Instance.weaponBehaviour.allWeaponPrizes[_i] != weapon) 
                    continue;
                
                PlayerBehaviour.Instance.weaponBehaviour.allWeaponPrizes[_i] = upgradeTiers[_currentTierOnUpgradingWeapon];
                PlayerBehaviour.Instance.weaponBehaviour.GetWeapon(PlayerBehaviour.Instance.weaponBehaviour.allWeaponPrizes[_i]);
                break;
            }
        }
        else
        {
            //Show that the upgrade cannot be achieved
        }
    }

    private void FillWeaponAmmo(WeaponObjectSO weapon)
    {
        //Checks for broken pistol because there refilling ammo does not cost
        if (TutorialManager.Instance.fillAmmoForFree || (PlayerBehaviour.Instance.playerCurrency.SpendCurrency(fillAmmoCost) && 
                                                     weapon.ammunitionInBackUp != PlayerBehaviour.Instance.weaponBehaviour.ammunitionInBackUp))
        {
            PlayerBehaviour.Instance.weaponBehaviour.ObtainAmmoDrop(null, weapon.ammunitionInBackUp);
            
            if (TutorialManager.Instance.fillAmmoForFree)
            {
                InGameUIManager.Instance.dialogueUI.SetRadioState(true, true);
                TutorialManager.Instance.canActivateGenerator = true;
            }
        }
        else
        {
            //Show that the fill ammo cannot be achieved
        }
    }

    private void OnDisable()
    {
        PlayerBehaviour.Instance.isPlayerBusy = false;
    }
}
