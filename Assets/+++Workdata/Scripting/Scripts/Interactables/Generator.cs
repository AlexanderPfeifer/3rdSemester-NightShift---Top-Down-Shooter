using UnityEngine;

public class Generator : MonoBehaviour
{
    [Header("Interactable")]
    [SerializeField] private RidesSO rideData;
    [SerializeField] private GameObject fortuneWheel;
    private Ride ride;
    
    [Header("WalkieTalkie")]
    [HideInInspector] public bool canPutAwayWalkieTalkie = true;
    
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

    private void Start()
    {
        ride = GetComponentInParent<Ride>();
    }

    public void SetUpFightArena()
    {
        fightMusic.Play();
        AudioManager.Instance.Stop("InGameMusic");
        fortuneWheel.SetActive(true);
        isInteractable = false;
        GetComponentInParent<Ride>().rideLight.SetActive(true);
        GetComponentInParent<Ride>().gameObject.GetComponent<Animator>().SetTrigger("LightOn");
        
        if (arenaFightFinished) 
            return;
        
        ride.ActivationStatusInvisibleWalls(true);
        Player.Instance.fightAreaCam.Priority = 15;
        
        //Set a bool for the PutAway Animation because the player can leave and enter the collider still inside the fight
        if (canPutAwayWalkieTalkie)
        {
            InGameUIManager.Instance.radioAnim.SetTrigger("PutAway");
        }
        canPutAwayWalkieTalkie = false;
    }
}
