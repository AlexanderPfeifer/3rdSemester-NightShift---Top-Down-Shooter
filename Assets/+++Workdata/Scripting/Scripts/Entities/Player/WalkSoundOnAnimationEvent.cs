using UnityEngine;

public class WalkSoundOnAnimationEvent : MonoBehaviour
{
    public void PlayRandomStepSound()
    {
        AudioManager.Instance.Play("walkSounds");
    }
}
