using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    //the linked collectible data, which is a ScriptableObject.
    //each has to be uniquely named, because with that unique name the data is identified within the
    //save game.
    [SerializeField] private WeaponObjectSO weaponData;

    private void Awake()
    {
        //When loading the scene, we destroy the weapon, if it was already saved as collected.
        if (GameSaveStateManager.instance.saveGameDataManager.HasWeapon(weaponData.weaponName))
            Destroy(gameObject);
    }

    //Collect is called from the FortuneWheel script

    private void Collect()
    {
        //We add the unique name of the collectible to the data, once the collectible is collected.
        //This means that it will be saved as well.
        GameSaveStateManager.instance.saveGameDataManager.AddWeapon(weaponData.weaponName);
        Destroy(gameObject);
    }

    //OnValidate is only called in editor when something about this script changed.
    //Here, we only change the game object name to represent what pickup is linked, 
    //without us having to change the name by hand
    private void OnValidate()
    {
        if (weaponData == null)
            name = "[Collectible] -unasigned-";
        else
            name = "[Collectible] " + weaponData.weaponName;
    }
}
