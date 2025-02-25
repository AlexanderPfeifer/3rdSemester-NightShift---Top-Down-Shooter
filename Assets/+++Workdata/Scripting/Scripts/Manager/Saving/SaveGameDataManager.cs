using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class SaveGameDataManager
{
    public string saveName;
    public string loadedSceneName = GameSaveStateManager.InGameSceneName;
    
    [Header("Collectibles Identifier")]
    public List<string> collectedCollectiblesIdentifiers = new();
    public List<string> collectedWeaponsIdentifiers = new();
    public List<string> weaponsInInventoryIdentifiers = new();
    [FormerlySerializedAs("finishedRides")] public List<string> finishedRidesIdentifiers = new();
    
    public Player.PlayerSaveData newPlayerSaveData;

    /// <param name="identifier">The identifier that is unique for every collectible</param>
    public void AddCollectible(string identifier)
    {
        if (collectedCollectiblesIdentifiers.Contains(identifier))
            return;
        collectedCollectiblesIdentifiers.Add(identifier);
    }
    
    /// <param name="identifier">The identifier that is unique for every collectible</param>
    public void AddWeapon(string identifier)
    {
        if (collectedWeaponsIdentifiers.Contains(identifier))
            return;
        collectedWeaponsIdentifiers.Add(identifier);

        if (weaponsInInventoryIdentifiers.Contains(identifier))
            return;
        weaponsInInventoryIdentifiers.Add(identifier);
    }
    
    /// <param name="identifier">The identifier that is unique for every collectible</param>
    public void AddRide(string identifier)
    {
        if (finishedRidesIdentifiers.Contains(identifier))
            return;
        finishedRidesIdentifiers.Add(identifier);
    }
    
    /// <param name="identifier">The identifier that is unique for every collectible</param>
    public bool HasCollectible(string identifier)
    {
        return collectedCollectiblesIdentifiers.Contains(identifier);
    }
    
    /// <param name="identifier">The identifier that is unique for every collectible</param>
    public bool HasWeapon(string identifier)
    {
        return collectedWeaponsIdentifiers.Contains(identifier);
    }
    
    /// <param name="identifier">The identifier that is unique for every collectible</param>
    public bool HasWeaponInInventory(string identifier)
    {
        return weaponsInInventoryIdentifiers.Contains(identifier);
    }
    
    /// <param name="identifier">The identifier that is unique for every collectible</param>
    public bool HasFinishedRide(string identifier)
    {
        return finishedRidesIdentifiers.Contains(identifier);
    }
}
