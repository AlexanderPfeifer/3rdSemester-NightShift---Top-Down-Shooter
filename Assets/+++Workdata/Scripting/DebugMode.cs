using UnityEngine;

public class DebugMode : MonoBehaviour
{
    [Header("Debugging")]
    public bool debugMode;

    public static DebugMode Instance;
    
    private void Awake()
    {
        Instance = this;
    }
}
