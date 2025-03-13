using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "new Collectibles List", menuName = "Create new Collectibles List", order = 0)]
public class CollectedWeaponsSO : ScriptableObject
{
    public List<CollectibleObjectSO> allCollectibles;
    
    public List<WeaponObjectSO> allWeapons;

    public CollectibleObjectSO GetCollectibleDataByIdentifier(string identifier)
    {
        return allCollectibles.FirstOrDefault(collectibleObjectSO => collectibleObjectSO.header == identifier);
    }

    //When called gets every weapon by string name
    public WeaponObjectSO GetWeaponDataByIdentifier(string identifier)
    {
        return allWeapons.FirstOrDefault(weaponObjectSO => weaponObjectSO.weaponName == identifier);
    }
}
