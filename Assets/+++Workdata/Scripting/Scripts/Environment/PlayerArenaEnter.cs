using UnityEngine;
using UnityEngine.Serialization;

public class PlayerArenaEnter : MonoBehaviour
{
    private Ride ride;
    [HideInInspector] public bool canPutAwayWalkieTalkie = true;
    
    private void Start()
    {
        ride = GetComponentInParent<Ride>();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.gameObject.GetComponent<Player>()) 
            return;

        if (ride.GetComponentInChildren<Generator>().isInteractable ||
            ride.GetComponentInChildren<Generator>().arenaFightFinished) 
            return;
        
        ride.ActivationStatusInvisibleWalls(true);
        ride.fightCam.Priority = 15;
        
        //Set a bool for the PutAway Animation because the player can leave and enter the collider still inside the fight
        if (canPutAwayWalkieTalkie)
        {
            InGameUIManager.Instance.radioAnim.SetTrigger("PutAway");
        }
        canPutAwayWalkieTalkie = false;
    }
}
