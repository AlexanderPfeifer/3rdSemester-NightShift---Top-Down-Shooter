using UnityEngine;
using UnityEngine.Serialization;

public class Generator : MonoBehaviour
{
    [Header("Interactable")]
    private Ride ride;
    
    [Header("WalkieTalkie")]
    [HideInInspector] public bool canPutAwayWalkieTalkie = true;
    
    [Header("Music")]
    [SerializeField] public AudioSource fightMusic;
    
    [FormerlySerializedAs("genInactive")] [HideInInspector] public bool genInteractable = true;

    private void Start()
    {
        ride = GetComponentInParent<Ride>();
    }

    public void SetUpFightArena()
    {
        fightMusic.Play();
        AudioManager.Instance.Stop("InGameMusic");
        genInteractable = false;
        ride.rideLight.SetActive(true);
        ride.gameObject.GetComponent<Animator>().SetTrigger("LightOn");
        ride.invisibleCollider.SetActive(true);

        if (Player.Instance.hasAbilityUpgrade)
        {
            InGameUIManager.Instance.abilityFillBar.gameObject.SetActive(true);
        }
        InGameUIManager.Instance.fightScene.SetActive(true);
        ride.waveStarted = true;
        ride.ResetRide();
        ride.StartEnemyClusterCoroutines();

        //Check if rideGotDestroyed because at the start the music already plays when turning on the generator
        if (ride.rideGotDestroyed)
            fightMusic.Play();        
        
        Player.Instance.fightAreaCam.Priority = 15;
        
        //Set a bool for the PutAway Animation because the player can leave and enter the collider still inside the fight
        if (canPutAwayWalkieTalkie)
        {
            InGameUIManager.Instance.radioAnim.SetTrigger("PutAway");
        }
        canPutAwayWalkieTalkie = false;
    }
}
