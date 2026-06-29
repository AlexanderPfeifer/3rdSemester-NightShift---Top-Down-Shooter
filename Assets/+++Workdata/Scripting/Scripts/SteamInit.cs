using Steamworks;
using UnityEngine;

public class SteamInit : MonoBehaviour
{
    public static bool Initialized;

    void Awake()
    {
        if (Initialized) return;

        Initialized = SteamAPI.Init();

        if (!Initialized)
        {
            Debug.LogError("Steam konnte nicht initialisiert werden!");
        }
    }

    void Update()
    {
        if (Initialized)
            SteamAPI.RunCallbacks();
    }

    void OnApplicationQuit()
    {
        if (Initialized)
            SteamAPI.Shutdown();
    }
}
