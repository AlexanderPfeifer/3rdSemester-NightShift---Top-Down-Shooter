using UnityEngine;

public class ShutterSound : MonoBehaviour
{
    public void MetalShutterDownSound()
    {
        AudioManager.Instance.Play("MetalShutterDown");
    }
}
