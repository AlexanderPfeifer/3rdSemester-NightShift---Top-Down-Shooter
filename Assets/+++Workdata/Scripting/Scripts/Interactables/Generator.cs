using UnityEngine;

public class Generator : MonoBehaviour
{
    [Header("Interactable")]
    private Ride ride;
    
    [Header("WalkieTalkie")]
    [HideInInspector] public bool canPutAwayWalkieTalkie = true;
    
    [Header("Music")]
    [SerializeField] public AudioSource fightMusic;
    
    [HideInInspector] public bool genInactive = true;

    private void Start()
    {
        ride = GetComponentInParent<Ride>();
    }

    public void SetUpFightArena()
    {
        fightMusic.Play();
        AudioManager.Instance.Stop("InGameMusic");
        genInactive = false;
        GetComponentInParent<Ride>().rideLight.SetActive(true);
        GetComponentInParent<Ride>().gameObject.GetComponent<Animator>().SetTrigger("LightOn");
        ride.ActivationStatusInvisibleWalls(true);
        ride.SetWave();
        Player.Instance.fightAreaCam.Priority = 15;
        
        //Set a bool for the PutAway Animation because the player can leave and enter the collider still inside the fight
        if (canPutAwayWalkieTalkie)
        {
            InGameUIManager.Instance.radioAnim.SetTrigger("PutAway");
        }
        canPutAwayWalkieTalkie = false;
    }
}
