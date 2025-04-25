using UnityEngine;

/// <summary>
/// Singleton class utilizing the "lazy initialization" pattern to ensure that only one cross-scene-persistent instance of a MonoBehaviour-derived component of type T exists in the scene hierarchy.
/// </summary>
public class SingletonPersistent<T> : Singleton<T> where T : Component
{

    // ===============================================
    // ============ UNITY EVENT FUNCTIONS ============
    // ===============================================
    protected override void Awake()
    {
        base.Awake();

        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }
}