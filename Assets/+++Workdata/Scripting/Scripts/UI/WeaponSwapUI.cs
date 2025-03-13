using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class WeaponSwapUI : MonoBehaviour
{
    public void KeepWeapon()
    {
        EventSystem.current.SetSelectedGameObject(null);
        
        InGameUIManager.Instance.weaponSwapScreen.SetActive(false);
        Player.Instance.isInteracting = false;
    }
    
    public void SwapWeapon()
    {
        EventSystem.current.SetSelectedGameObject(null);

        foreach (var _weapon in Player.Instance.allWeaponPrizes.Where(weapon => GameSaveStateManager.Instance.saveGameDataManager.HasWeaponInInventory(weapon.weaponName)))
        {
            GameSaveStateManager.Instance.saveGameDataManager.weaponsInInventoryIdentifiers.Remove(_weapon.name);
            InGameUIManager.Instance.equippedWeapon.SetActive(true);
        }

        InGameUIManager.Instance.weaponSwapScreen.SetActive(false);
        InGameUIManager.Instance.fortuneWheelScreen.SetActive(true);
    }
}
