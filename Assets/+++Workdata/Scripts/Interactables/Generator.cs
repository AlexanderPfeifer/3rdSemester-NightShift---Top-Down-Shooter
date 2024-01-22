using UnityEngine;

public class Generator : MonoBehaviour
{
    [SerializeField] private RidesSO rideData;
    [SerializeField] private GameObject fortuneWheel;
    [SerializeField] public AudioSource fightMusic;
    
    [HideInInspector] public bool isInteractable = true;
    public bool arenaFightFinished;

    //If the ride was already finished, the generator cannot be interacted with, so no fight can be started
    private void Awake()
    {
        //When loading the scene, we destroy the collectible, if it was already saved as collected.
        if (GameSaveStateManager.Instance.saveGameDataManager.HasFinishedRide(rideData.rideName))
        {
            isInteractable = false;
            arenaFightFinished = true;
        }
    }

    //When the Generator got activated, everything for the fight is made ready
    public void SetFortuneWheel()
    {
        fightMusic.Play();
        AudioManager.Instance.Stop("InGameMusic");
        fortuneWheel.SetActive(true);
        isInteractable = false;
        GetComponentInParent<Ride>().rideLight.SetActive(true);
        GetComponentInParent<Ride>().gameObject.GetComponent<Animator>().SetTrigger("LightOn");
    }
}
