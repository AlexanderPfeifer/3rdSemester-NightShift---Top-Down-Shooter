using UnityEngine;

public class ShutterSound : MonoBehaviour
{
    //Here I play the MetalShutterSound as an animation event
    public void MetalShutterDownSound()
    {
        AudioManager.Instance.Play("MetalShutterDown");
    }
}
