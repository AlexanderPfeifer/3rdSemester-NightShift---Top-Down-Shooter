using UnityEngine;

public class Collectible : MonoBehaviour
{
    [SerializeField] private CollectibleObjectSO collectibleData;

    private void Awake()
    {
        if (GameSaveStateManager.Instance.saveGameDataManager.HasCollectible(collectibleData.header))
            Destroy(gameObject);
    }

    public void Collect()
    {
        GameSaveStateManager.Instance.saveGameDataManager.AddCollectible(collectibleData.header);
        Destroy(gameObject);
    }
}
