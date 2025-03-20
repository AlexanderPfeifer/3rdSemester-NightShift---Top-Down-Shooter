using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "new Collectibles List", menuName = "Create new Collectibles List", order = 0)]
public class CollectedCollectibles : ScriptableObject
{
    public List<CollectibleObjectSO> allCollectibles;
    
    public CollectibleObjectSO GetCollectibleDataByIdentifier(string identifier)
    {
        return allCollectibles.FirstOrDefault(collectibleObjectSO => collectibleObjectSO.header == identifier);
    }
}
