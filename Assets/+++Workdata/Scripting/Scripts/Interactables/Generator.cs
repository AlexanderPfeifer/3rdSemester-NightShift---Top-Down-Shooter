using UnityEngine;

public class Generator : MonoBehaviour
{
    [Header("Interactable")]
    [SerializeField] private GameObject fortuneWheel;
    private Ride ride;
    
    [Header("WalkieTalkie")]
    [HideInInspector] public bool canPutAwayWalkieTalkie = true;
    
    [Header("Music")]
    [SerializeField] public AudioSource fightMusic;
    
    [HideInInspector] public bool genInactive = true;

    public void SetUpFightArena()
    {
        fightMusic.Play();
        AudioManager.Instance.Stop("InGameMusic");
        fortuneWheel.SetActive(true);
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
