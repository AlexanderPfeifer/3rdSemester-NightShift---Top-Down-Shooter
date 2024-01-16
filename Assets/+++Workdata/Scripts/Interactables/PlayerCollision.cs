using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    private Ride ride;

    private void Start()
    {
        ride = GetComponentInParent<Ride>();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.GetComponent<Player>())
        {
            if (!ride.GetComponentInChildren<Generator>().isInteractable)
            {
                ride.ActivateInvisibleWalls(true);
                ride.fightCam.Priority = 15;
                InGameUI.instance.radioAnim.SetTrigger("PutAway");
            }
        }
    }
}
