#if UNITY_EDITOR
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class DebugMode : SingletonPersistent<DebugMode>
{
    [Header("ACTIVATION")]
    public bool debugMode;
    
    [Header("Settings")]
    public ChoosableWeapons equipWeapon;
    public UnlockWeapons[] UnlockWeapons;
    public bool activateRide;
    public int currencyAtStart;

    public enum ChoosableWeapons
    {
        None,
        Shotgun, 
        AR,
        PopcornPistol,
        HuntingRifle,
        MagnumMagnum
    }
    
    protected override void Awake()
    {
        base.Awake();
        
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
        foreach (var _unlockWeapon in UnlockWeapons)
        {
            if (_unlockWeapon.getWeapon)
            {
                GameSaveStateManager.Instance.saveGameDataManager.AddWeapon(_unlockWeapon.weaponName);
            }
        }
        
        switch (equipWeapon)
        {
            case ChoosableWeapons.None :
                break;
            
            case ChoosableWeapons.Shotgun :
                GetDebuggedWeapon("Lollipop Shotgun");
                break;
            
            case ChoosableWeapons.AR :
                GetDebuggedWeapon("French Fries AR");
                break;
            
            case ChoosableWeapons.MagnumMagnum :
                GetDebuggedWeapon("Magnum magnum");
                break;
            
            case ChoosableWeapons.PopcornPistol :
                GetDebuggedWeapon("Popcorn Launcher");
                break;
            
            case ChoosableWeapons.HuntingRifle :
                GetDebuggedWeapon("Corn Dog Hunting Rifle");
                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void GetDebuggedWeapon(string weaponName)
    {
        PlayerBehaviour.Instance.weaponBehaviour.GetWeapon(PlayerBehaviour.Instance.weaponBehaviour.allWeaponPrizes.FirstOrDefault(w => w.weaponName == weaponName));
    }
}

[Serializable]
public class UnlockWeapons
{
    public string weaponName;
    public bool getWeapon;
}
#endif