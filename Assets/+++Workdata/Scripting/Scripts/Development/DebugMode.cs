using System;
using UnityEngine;

public class DebugMode : SingletonPersistent<DebugMode>
{
    [Header("Settings")]
    public WeaponObjectSO equipWeapon;
    public UnlockWeapons[] UnlockWeapons;
    public bool activateRide;
    public int currencyAtStart;
    [SerializeField] private int playWave;

    public void AddWaves()
    {
        for (int _i = 0; _i < playWave; _i++)
        {
            GameSaveStateManager.Instance.saveGameDataManager.AddWaveCount();
        }
    }
    
    public void GetDebugWeapon()
    {
        if(equipWeapon == null)
            return;

        foreach (var _unlockWeapon in UnlockWeapons)
        {
            if (_unlockWeapon.getWeapon)
            {
                GameSaveStateManager.Instance.saveGameDataManager.AddWeapon(_unlockWeapon.weaponName);
            }
        }
        
        GetDebuggedWeapon(equipWeapon);
    }

    private void GetDebuggedWeapon(WeaponObjectSO weapon)
    {
        PlayerBehaviour.Instance.weaponBehaviour.GetWeapon(weapon);
    }
}

[Serializable]
public class UnlockWeapons
{
    public string weaponName;
    public bool getWeapon;
}
