using UnityEngine;
using UnityEngine.UI;

public class UpgradeUI : MonoBehaviour
{
    [Header("Upgrade Tiers")] 
    [SerializeField] private WeaponObjectSO[] lollipopShotgunUpgradeTiers;
    [SerializeField] private WeaponObjectSO[] assaultRifleUpgradeTiers;
    [SerializeField] private WeaponObjectSO[] magnumMagnumUpgradeTiers;
    [SerializeField] private WeaponObjectSO[] huntingRifleUpgradeTiers;
    [SerializeField] private WeaponObjectSO[] popcornLauncherUpgradeTiers;
    [SerializeField] private WeaponObjectSO[] brokenPistolUpgradeTiers;

    [SerializeField] private Button lollipopShotgunUpgradeButton;
    [SerializeField] private Button assaultRifleUpgradeButton;
    [SerializeField] private Button magnumMagnumUpgradeButton;
    [SerializeField] private Button huntingRifleUpgradeButton;
    [SerializeField] private Button popcornLauncherUpgradeButton;
    [SerializeField] private Button brokenPistolUpgradeButton;

    private void OnEnable()
    {
       SetUpgradeButtons();
    }

    public void SetUpgradeButtons()
    {
        foreach (var _weapon in Player.Instance.allWeaponPrizes)
        {
            if (GameSaveStateManager.Instance.saveGameDataManager.HasWeapon(_weapon.weaponName))
            {
                switch (_weapon.weaponName)
                {
                    case "Magnum magnum" :
                        magnumMagnumUpgradeButton.gameObject.SetActive(true);
                        magnumMagnumUpgradeButton.onClick.AddListener(() => UpgradeWeapon("Magnum magnum", magnumMagnumUpgradeTiers));
                        break;
                            
                    case "French Fries AR" :
                        assaultRifleUpgradeButton.gameObject.SetActive(true);
                        assaultRifleUpgradeButton.onClick.AddListener(() => UpgradeWeapon("French Fries AR", assaultRifleUpgradeTiers));
                        break;
                            
                    case "Lollipop Shotgun" :
                        lollipopShotgunUpgradeButton.gameObject.SetActive(true);
                        lollipopShotgunUpgradeButton.onClick.AddListener(() => UpgradeWeapon("Magnum magnum", lollipopShotgunUpgradeTiers));
                        break;
                            
                    case "Corn Dog Hunting Rifle" :
                        huntingRifleUpgradeButton.gameObject.SetActive(true);
                        huntingRifleUpgradeButton.onClick.AddListener(() => UpgradeWeapon("Corn Dog Hunting Rifle", huntingRifleUpgradeTiers));
                        break;
                            
                    case "Popcorn Launcher" :
                        popcornLauncherUpgradeButton.gameObject.SetActive(true);
                        popcornLauncherUpgradeButton.onClick.AddListener(() => UpgradeWeapon("Popcorn Launcher", popcornLauncherUpgradeTiers));
                        break;
                            
                    case "Broken pistol" :
                        brokenPistolUpgradeButton.gameObject.SetActive(true);
                        brokenPistolUpgradeButton.onClick.AddListener(() => UpgradeWeapon("Broken pistol", brokenPistolUpgradeTiers));
                        break;
                }
            }
        }
    }

    private void UpgradeWeapon(string weapon, WeaponObjectSO[] upgradeTiers)
    {
        for (int _i = 0; _i < Player.Instance.allWeaponPrizes.Count; _i++)
        {
            if (Player.Instance.allWeaponPrizes[_i].weaponName == weapon)
            {
                Player.Instance.allWeaponPrizes[_i] = Player.Instance.allWeaponPrizes[_i].upgradeTier switch
                {
                    0 => upgradeTiers[0],
                    1 => upgradeTiers[1],
                    2 => upgradeTiers[2],
                    _ => Player.Instance.allWeaponPrizes[_i]
                };

                Player.Instance.GetWeapon(Player.Instance.allWeaponPrizes[_i]);
                break;
            }
        }
    }
}
