using UnityEngine;

public class Generator : MonoBehaviour
{
    [Header("Interactable")]
    [SerializeField] private RidesSO rideData;
    [SerializeField] private GameObject fortuneWheel;
    
    [Header("Music")]
    [SerializeField] public AudioSource fightMusic;
    
    [HideInInspector] public bool isInteractable = true;
    public bool arenaFightFinished;

    private void Awake()
    {
        if (GameSaveStateManager.Instance.saveGameDataManager.HasFinishedRide(rideData.rideName))
        {
            isInteractable = false;
            arenaFightFinished = true;
        }
    }

    public void SetUpFightArena()
    {
        fightMusic.Play();
        AudioManager.Instance.Stop("InGameMusic");
        fortuneWheel.SetActive(true);
        isInteractable = false;
        GetComponentInParent<Ride>().rideLight.SetActive(true);
        GetComponentInParent<Ride>().gameObject.GetComponent<Animator>().SetTrigger("LightOn");
    }
}
