using UnityEngine;

public class Generator : MonoBehaviour
{
    [SerializeField] private RidesSO rideData;
    [HideInInspector] public bool isInteractable = true;
    public bool arenaFightFinished;
    [SerializeField] private GameObject fortuneWheel;

    private void Awake()
    {
        //When loading the scene, we destroy the collectible, if it was already saved as collected.
        if (GameSaveStateManager.instance.saveGameDataManager.HasFinishedRide(rideData.rideName))
        {
            isInteractable = false;
            arenaFightFinished = true;
        }
    }

    public void SetFortuneWheel()
    {
        fortuneWheel.SetActive(true);
        isInteractable = false;
        GetComponentInParent<Ride>().rideLight.SetActive(true);
    }
}
