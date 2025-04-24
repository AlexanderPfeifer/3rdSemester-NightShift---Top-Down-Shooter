using UnityEngine;
using UnityEngine.Serialization;

public class Generator : MonoBehaviour
{
    [Header("WalkieTalkie")]
    [HideInInspector] public bool canPutAwayWalkieTalkie = true;
    
    [Header("Music")]
    [SerializeField] public AudioSource fightMusic;
    
    [FormerlySerializedAs("genInactive")] [HideInInspector] public bool genInteractable = true;

    public void SetUpFightArena()
    {
        fightMusic.Play();
        AudioManager.Instance.Stop("InGameMusic");
        genInteractable = false;
        Ride.Instance.rideLight.SetActive(true);
        Ride.Instance.gameObject.GetComponent<Animator>().SetTrigger("LightOn");
        Ride.Instance.invisibleCollider.SetActive(true);

        if (PlayerBehaviour.Instance.abilityBehaviour.hasAbilityUpgrade)
        {
            InGameUIManager.Instance.abilityFillBar.gameObject.SetActive(true);
        }
        InGameUIManager.Instance.fightUI.SetActive(true);
        Ride.Instance.waveStarted = true;
        Ride.Instance.ResetRide();
        Ride.Instance.StartEnemyClusterCoroutines();

        //Check if rideGotDestroyed because at the start the music already plays when turning on the generator
        if (Ride.Instance.rideGotDestroyed)
            fightMusic.Play();        
        
        //Set a bool for the PutAway Animation because the player can leave and enter the collider still inside the fight
        if (canPutAwayWalkieTalkie)
        {
            InGameUIManager.Instance.dialogueUI.SetRadioState(false, false);
        }
        canPutAwayWalkieTalkie = false;
    }
}
