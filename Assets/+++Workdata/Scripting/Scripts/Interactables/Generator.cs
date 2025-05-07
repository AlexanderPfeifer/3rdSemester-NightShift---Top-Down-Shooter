using System;
using UnityEngine;

public class Generator : MonoBehaviour
{
    [Header("WalkieTalkie")]
    [HideInInspector] public bool canPutAwayWalkieTalkie = true;
    
    [Header("Music")]
    [SerializeField] public AudioSource fightMusic;
    
    [HideInInspector] public bool interactable;
    
    [Header("Gate")] 
    public Animator gateAnim;

    private void Start()
    {
        interactable = false;
    }

    public void SetUpFightArena()
    {
        fightMusic.Play();
        AudioManager.Instance.Stop("InGameMusic");
        Ride.Instance.rideLight.SetActive(true);
        //Ride.Instance.rideRenderer.GetComponent<Animator>().SetTrigger("LightOn");
        Ride.Instance.invisibleCollider.SetActive(true);

        if (PlayerBehaviour.Instance.abilityBehaviour.hasAbilityUpgrade)
        {
            InGameUIManager.Instance.abilityFillBar.gameObject.SetActive(true);
        }
        InGameUIManager.Instance.fightUI.SetActive(true);
        Ride.Instance.waveStarted = true;
        Ride.Instance.ResetRide();
        Ride.Instance.StartEnemyClusterCoroutines();

        //Set a bool for the PutAway Animation because the player can leave and enter the collider still inside the fight
        if (canPutAwayWalkieTalkie)
        {
            InGameUIManager.Instance.dialogueUI.SetDialogueBoxState(false, false);
        }
        canPutAwayWalkieTalkie = false;
    }
}
