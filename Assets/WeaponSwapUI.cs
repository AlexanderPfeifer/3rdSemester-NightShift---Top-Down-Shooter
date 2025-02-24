using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class WeaponSwapUI : MonoBehaviour
{
    public void KeepWeapon()
    {
        EventSystem.current.SetSelectedGameObject(null);
        
        if (Player.Instance.GetInteractionObjectInRange(Player.Instance.wheelOfFortuneLayer, out Collider2D _interactable))
        {
            var _fortuneWheel = _interactable.GetComponent<FortuneWheel>();
            _fortuneWheel.ride.GetComponent<Ride>().canActivateRide = true;
            _fortuneWheel.DeactivateFortuneWheel();   
        }
        InGameUI.Instance.weaponSwapScreen.SetActive(false);
        Player.Instance.isInteracting = false;
    }
    
    public void SwapWeapon()
    {
        EventSystem.current.SetSelectedGameObject(null);

        foreach (var _weapon in Player.Instance.allWeaponPrizes.Where(weapon => GameSaveStateManager.Instance.saveGameDataManager.HasWeaponInInventory(weapon.weaponName)))
        {
            GameSaveStateManager.Instance.saveGameDataManager.weaponsInInventoryIdentifiers.Remove(_weapon.name);
            InGameUI.Instance.inventoryWeapon.SetActive(true);
        }

        InGameUI.Instance.weaponSwapScreen.SetActive(false);
        InGameUI.Instance.fortuneWheelScreen.SetActive(true);
    }
}
