#if UNITY_EDITOR
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class DebugMode : MonoBehaviour
{

    [Header("ACTIVATION")]
    public bool debugMode;
    
    [Header("Settings")]
    public bool activateRide;
    public ChoosableWeapons choosableWeapons;
    public int currencyAtStart;

    public static DebugMode Instance;
    
    public enum ChoosableWeapons
    {
        None,
        Shotgun, 
        AR,
        PopcornPistol,
        HuntingRifle,
        MagnumMagnum
    }
    
    private void Awake()
    {
        Instance = this;

        //StartCoroutine(UnloadGameScene());
    }

    private IEnumerator UnloadGameScene()
    {
        while (!UnityEngine.SceneManagement.SceneManager.GetSceneByName("InGame").isLoaded)
        {
            yield return null;
        }
        
        UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync("InGame");
    }
    
    public void GetDebugWeapon()
    {
        switch (choosableWeapons)
        {
            case ChoosableWeapons.None :
                break;
            
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
#endif