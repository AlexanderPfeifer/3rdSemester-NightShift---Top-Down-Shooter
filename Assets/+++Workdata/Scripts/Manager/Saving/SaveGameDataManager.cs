using System.Collections.Generic;

[System.Serializable]
public class SaveGameDataManager
{
    public string loadedSceneName = GameSaveStateManager.InGameSceneName;

    
    /// <summary>
    /// A list of all unique identifiers for all collected collectibles 
    /// </summary>
    public List<string> collectedCollectiblesIdentifiers = new List<string>();
    
    public List<string> collectedWeaponsIdentifiers = new List<string>();
    
    public Player.PlayerSaveData newPlayerSaveData;

    /// <summary>
    /// Called whenever a collectible is collected
    /// </summary>
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
    
    public bool HasWeapon(string identifier)
    {
        return collectedWeaponsIdentifiers.Contains(identifier);
    }
}
