using UnityEngine;

public class Generator : MonoBehaviour
{
    [SerializeField] private RidesSO rideData;
    public bool isInteractable = true;

    private void Awake()
    {
        //When loading the scene, we destroy the collectible, if it was already saved as collected.
        if (GameSaveStateManager.instance.saveGameDataManager.HasFinishedRide(rideData.rideName))
        {
            //Start Gen Animation
            isInteractable = false;
        }
    }
}
