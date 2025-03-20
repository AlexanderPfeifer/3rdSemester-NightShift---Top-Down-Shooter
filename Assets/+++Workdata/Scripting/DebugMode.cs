using System;
using System.Linq;
using UnityEngine;

public class DebugMode : MonoBehaviour
{
    [Header("Debugging")]
    public bool debugMode;
    
    public static DebugMode Instance;

    public ChoosableWeapons choosableWeapons;

    public enum ChoosableWeapons
    {
        Shotgun, 
        AR,
        PopcornPistol,
        HuntingRifle,
        MagnumMagnum
    }
    
    private void Awake()
    {
        Instance = this;
    }

    public void GetDebugWeapon()
    {
        switch (choosableWeapons)
        {
            case ChoosableWeapons.Shotgun :
                PlayerBehaviour.Instance.weaponBehaviour.GetWeapon(PlayerBehaviour.Instance.weaponBehaviour.allWeaponPrizes.FirstOrDefault(w => w.weaponName == "Lollipop Shotgun"));
                break;
            
            case ChoosableWeapons.AR :
                PlayerBehaviour.Instance.weaponBehaviour.GetWeapon(PlayerBehaviour.Instance.weaponBehaviour.allWeaponPrizes.FirstOrDefault(w => w.weaponName == "French Fries AR"));                
                break;
            
            case ChoosableWeapons.MagnumMagnum :
                PlayerBehaviour.Instance.weaponBehaviour.GetWeapon(PlayerBehaviour.Instance.weaponBehaviour.allWeaponPrizes.FirstOrDefault(w => w.weaponName == "Magnum magnum"));                
                break;
            
            case ChoosableWeapons.PopcornPistol :
                PlayerBehaviour.Instance.weaponBehaviour.GetWeapon(PlayerBehaviour.Instance.weaponBehaviour.allWeaponPrizes.FirstOrDefault(w => w.weaponName == "Popcorn Launcher"));                
                break;
            
            case ChoosableWeapons.HuntingRifle :
                PlayerBehaviour.Instance.weaponBehaviour.GetWeapon(PlayerBehaviour.Instance.weaponBehaviour.allWeaponPrizes.FirstOrDefault(w => w.weaponName == "Corn Dog Hunting Rifle"));                
                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
