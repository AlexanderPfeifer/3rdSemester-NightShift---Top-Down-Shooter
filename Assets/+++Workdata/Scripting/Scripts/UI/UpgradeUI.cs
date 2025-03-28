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
    [SerializeField] private WeaponObjectSO[] brokenPistolUpgradeTiers;

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

    [Header("Tier Cost")] 
    [SerializeField] private int[] tierCosts;

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
                    magnumMagnumUpgradeButton.onClick.AddListener(() => UpgradeWeapon("Magnum magnum", magnumMagnumUpgradeTiers));
                    magnumMagnumFillAmmoButton.onClick.AddListener(() => FillWeaponAmmo("Magnum magnum"));
                    break;
                            
                case "French Fries AR" :
                    assaultRifleUpgradeButton.gameObject.SetActive(true);
                    assaultRifleFillAmmoButton.gameObject.SetActive(true);
                    assaultRifleUpgradeButton.onClick.AddListener(() => UpgradeWeapon("French Fries AR", assaultRifleUpgradeTiers));
                    assaultRifleFillAmmoButton.onClick.AddListener(() => FillWeaponAmmo("French Fries AR"));
                    break;
                            
                case "Lollipop Shotgun" :
                    lollipopShotgunUpgradeButton.gameObject.SetActive(true);
                    lollipopShotgunFillAmmoButton.gameObject.SetActive(true);
                    lollipopShotgunUpgradeButton.onClick.AddListener(() => UpgradeWeapon("Lollipop Shotgun", lollipopShotgunUpgradeTiers));
                    lollipopShotgunFillAmmoButton.onClick.AddListener(() => FillWeaponAmmo("Lollipop Shotgun"));
                    break;
                            
                case "Corn Dog Hunting Rifle" :
                    huntingRifleUpgradeButton.gameObject.SetActive(true);
                    huntingRifleFillAmmoButton.gameObject.SetActive(true);
                    huntingRifleUpgradeButton.onClick.AddListener(() => UpgradeWeapon("Corn Dog Hunting Rifle", huntingRifleUpgradeTiers));
                    huntingRifleFillAmmoButton.onClick.AddListener(() => FillWeaponAmmo("Corn Dog Hunting Rifle"));
                    break;
                            
                case "Popcorn Launcher" :
                    popcornLauncherUpgradeButton.gameObject.SetActive(true);
                    popcornLauncherFillAmmoButton.gameObject.SetActive(true);
                    popcornLauncherUpgradeButton.onClick.AddListener(() => UpgradeWeapon("Popcorn Launcher", popcornLauncherUpgradeTiers));
                    popcornLauncherFillAmmoButton.onClick.AddListener(() => FillWeaponAmmo("Popcorn Launcher"));
                    break;
                            
                case "Broken pistol" :
                    brokenPistolUpgradeButton.gameObject.SetActive(true);
                    //brokenPistolUpgradeButton.gameObject.SetActive(true);
                    brokenPistolUpgradeButton.onClick.AddListener(() => UpgradeWeapon("Broken pistol", brokenPistolUpgradeTiers));
                    //popcornLauncherFillAmmoButton.onClick.AddListener(() => FillWeaponAmmo("Popcorn Launcher"));
                    break;
            }
        }
    }

    private void UpgradeWeapon(string weapon, IReadOnlyList<WeaponObjectSO> upgradeTiers)
    {
        for (int _i = 0; _i < PlayerBehaviour.Instance.weaponBehaviour.allWeaponPrizes.Count; _i++)
        {
            int _currentTierOnUpgradingWeapon = PlayerBehaviour.Instance.weaponBehaviour.allWeaponPrizes[_i].upgradeTier;

            if (PlayerBehaviour.Instance.weaponBehaviour.allWeaponPrizes[_i].weaponName == weapon &&
                 _currentTierOnUpgradingWeapon < upgradeTiers.Count &&
                PlayerBehaviour.Instance.playerCurrency.SpendCurrency(tierCosts[_currentTierOnUpgradingWeapon]))
            {
                PlayerBehaviour.Instance.weaponBehaviour.allWeaponPrizes[_i] = upgradeTiers[_currentTierOnUpgradingWeapon]; 
                PlayerBehaviour.Instance.weaponBehaviour.GetWeapon(PlayerBehaviour.Instance.weaponBehaviour.allWeaponPrizes[_i]);
                break;
            }
            else
            {
                //Show that the upgrade cannot be achieved
            }
        }
    }

    private void FillWeaponAmmo(string weapon)
    {
        foreach (var _weapon in PlayerBehaviour.Instance.weaponBehaviour.allWeaponPrizes)
        {
            if (_weapon.weaponName == weapon)
            {
                PlayerBehaviour.Instance.weaponBehaviour.ObtainAmmoDrop(null, _weapon.ammunitionInBackUp);
                break;
            }
            else
            {
                //Show that the fill ammo cannot be achieved
            }
        }
    }

    private void OnDisable()
    {
        PlayerBehaviour.Instance.isPlayerBusy = false;
    }
}
