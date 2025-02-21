using System.Collections.Generic;

[System.Serializable]
public class SaveGameDataManager
{
    public string saveName;
    public string loadedSceneName = GameSaveStateManager.InGameSceneName;
    
    /// A list of all unique identifiers for all collected collectibles 
    public List<string> collectedCollectiblesIdentifiers = new List<string>();
    
    public List<string> collectedWeaponsIdentifiers = new List<string>();
    
    public List<string> weaponsInInventoryIdentifiers = new List<string>();
    
    public List<string> finishedRides = new List<string>();
    
    public Player.PlayerSaveData newPlayerSaveData;

    /// Called whenever a collectible is collected
    /// <param name="identifier">The identifier that is unique for every collectible</param>
    public void AddCollectible(string identifier)
    {
        if (collectedCollectiblesIdentifiers.Contains(identifier))
            return;
        collectedCollectiblesIdentifiers.Add(identifier);
    }

    /// <summary>
    /// Called whenever a collectible is collected
    /// </summary>
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
    
    public void AddRide(string identifier)
    {
        if (finishedRides.Contains(identifier))
            return;
        finishedRides.Add(identifier);
    }
    
    /// <summary>
    /// Called when we try to find out if a collectible was already collected
    /// </summary>
    /// <param name="identifier">The identifier that is unique for every collectible</param>
    /// <returns>Returns true if collectible is collected, otherwise returns false</returns>
    public bool HasCollectible(string identifier)
    {
        return collectedCollectiblesIdentifiers.Contains(identifier);
    }
    
    /// <summary>
    /// Called when we try to find out if a weapon was already collected
    /// </summary>
    public bool HasWeapon(string identifier)
    {
        return collectedWeaponsIdentifiers.Contains(identifier);
    }

    /// <summary>
    /// Called when we try to find out if a weapon was already collected in the inventory
    /// </summary>
    public bool HasWeaponInInventory(string identifier)
    {
        return weaponsInInventoryIdentifiers.Contains(identifier);
    }
    
    /// <summary>
    /// Called when we try to find out if a ride was already finished
    /// </summary>
    public bool HasFinishedRide(string identifier)
    {
        return finishedRides.Contains(identifier);
    }
}
