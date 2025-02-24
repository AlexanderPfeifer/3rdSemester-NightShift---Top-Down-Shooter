using UnityEngine;

public class WalkSoundOnAnimationEvent : MonoBehaviour
{
    [SerializeField] private string[] walkSounds;
    
    public void PlayRandomStepSound()
    {
        AudioManager.Instance.PlayRandomSoundFromListArray(walkSounds);
    }
}
