using UnityEngine;

public class Generator : MonoBehaviour
{
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
        
        Ride.Instance.waveStarted = true;
        Ride.Instance.ResetRide();
        gateAnim.SetBool("OpenGate", false);
        Ride.Instance.StartEnemyClusterCoroutines();
        
        InGameUIManager.Instance.dialogueUI.SetDialogueBoxState(false, false);
    }
}
