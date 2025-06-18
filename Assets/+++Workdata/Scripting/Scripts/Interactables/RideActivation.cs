using UnityEngine;

public class RideActivation : MonoBehaviour
{
    [Header("Music")]
    [SerializeField] public AudioSource fightMusic;
    
    [Header("Gate")] 
    public Animator gateAnim;

    [HideInInspector] public bool interactable = false;

    public void SetUpFightArena()
    {
        gateAnim.SetBool("OpenGate", false);

        foreach (var _light in Ride.Instance.rideLight)
        {
            _light.SetActive(true);
        }

        AudioManager.Instance.Stop("InGameMusic");
        fightMusic.Play();

        InGameUIManager.Instance.dialogueUI.SetWalkieTalkieTextBoxAnimation(false, false);

        Ride.Instance.waveStarted = true;
        Ride.Instance.StartEnemyClusterCoroutines();
    }
}
