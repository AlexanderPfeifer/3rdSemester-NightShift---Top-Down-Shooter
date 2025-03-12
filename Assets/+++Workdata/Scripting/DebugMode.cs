using System;
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
                Player.Instance.GetWeapon(Player.Instance.allWeaponPrizes[0]);
                break;
            
            case ChoosableWeapons.AR :
                Player.Instance.GetWeapon(Player.Instance.allWeaponPrizes[1]);
                break;
            
            case ChoosableWeapons.MagnumMagnum :
                Player.Instance.GetWeapon(Player.Instance.allWeaponPrizes[2]);
                break;
            
            case ChoosableWeapons.PopcornPistol :
                Player.Instance.GetWeapon(Player.Instance.allWeaponPrizes[3]);
                break;
            
            case ChoosableWeapons.HuntingRifle :
                Player.Instance.GetWeapon(Player.Instance.allWeaponPrizes[4]);
                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }
        
    }
}
