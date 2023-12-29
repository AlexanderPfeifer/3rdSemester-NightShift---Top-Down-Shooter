using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new Collectibles List", menuName = "Create new Collectibles List", order = 0)]
public class InGameCollectedObjects : ScriptableObject
{
    public List<CollectibleObjectSO> allCollectibles = new List<CollectibleObjectSO>();
    
    public List<WeaponObjectSO> allWeapons = new List<WeaponObjectSO>();

    public CollectibleObjectSO GetCollectibleDataByIdentifier(string identifier)
    {
        for (int index = 0; index < allCollectibles.Count; index++)
        {
            if (allCollectibles[index].header == identifier)
                return allCollectibles[index];
        }

        return null;
    }

    public WeaponObjectSO GetWeaponDataByIdentifier(string identifier)
    {
        for (int index = 0; index < allWeapons.Count; index++)
        {
            if (allWeapons[index].weaponName == identifier)
                return allWeapons[index];
        }

        return null;
    }
}
