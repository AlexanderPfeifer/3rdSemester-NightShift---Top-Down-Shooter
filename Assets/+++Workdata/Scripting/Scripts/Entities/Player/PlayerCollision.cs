using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    private Ride ride;
    public bool canPutAway = true;
    
    private void Start()
    {
        ride = GetComponentInParent<Ride>();
    }

    //When the player enters the fight area the camera is changed and the walkie-talkie is put away
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.gameObject.GetComponent<Player>()) 
            return;

        if (ride.GetComponentInChildren<Generator>().isInteractable ||
            ride.GetComponentInChildren<Generator>().arenaFightFinished) 
            return;
        
        ride.ActivateInvisibleWalls(true);
        ride.fightCam.Priority = 15;
        
        if (canPutAway)
        {
            InGameUI.Instance.radioAnim.SetTrigger("PutAway");
        }
        canPutAway = false;
    }
}
