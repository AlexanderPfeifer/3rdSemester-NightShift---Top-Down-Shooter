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
                Player.Instance.GetWeapon(Player.Instance.allWeaponPrizes.FirstOrDefault(w => w.weaponName == "Lollipop Shotgun"));
                break;
            
            case ChoosableWeapons.AR :
                Player.Instance.GetWeapon(Player.Instance.allWeaponPrizes.FirstOrDefault(w => w.weaponName == "French Fries AR"));                
                break;
            
            case ChoosableWeapons.MagnumMagnum :
                Player.Instance.GetWeapon(Player.Instance.allWeaponPrizes.FirstOrDefault(w => w.weaponName == "Magnum magnum"));                
                break;
            
            case ChoosableWeapons.PopcornPistol :
                Player.Instance.GetWeapon(Player.Instance.allWeaponPrizes.FirstOrDefault(w => w.weaponName == "Popcorn Launcher"));                
                break;
            
            case ChoosableWeapons.HuntingRifle :
                Player.Instance.GetWeapon(Player.Instance.allWeaponPrizes.FirstOrDefault(w => w.weaponName == "Corn Dog Hunting Rifle"));                
                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
