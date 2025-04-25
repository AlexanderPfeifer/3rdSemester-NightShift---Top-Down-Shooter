using UnityEngine;

/// <summary>
/// Singleton class utilizing the "lazy initialization" pattern to ensure that only one instance of a MonoBehaviour-derived component of type T exists in the scene hierarchy.
/// </summary>
/// <typeparam name="T">The type of MonoBehaviour-derived component to create a singleton instance of.</typeparam>
/// <remarks>
/// If an instance of type T does not exist in the scene hierarchy, it will attempt to find one using FindObjectOfType<T>().
/// When a GameObject containing a singleton instance is spawned, any duplicate instances of the singleton will be silently destroyed.
/// When the GameObject containing the singleton instance is destroyed, the singleton instance reference is set to null.
/// This singleton is NOT marked as DontDestroyOnLoad().
/// </remarks>
public class Singleton<T> : MonoBehaviour where T : Component
{
    protected static T _instance;

    // Property to access the singleton instance
    public static T Instance
    {
        get
        {
            // Check if instance is null (not instantiated yet)
            if (_instance == null)
            {
                // Try to find an existing instance in the scene
                _instance = FindAnyObjectByType<T>();
            }

            // Return the singleton instance (even if it's null)
            return _instance;
        }
    }

    // Kill any potential duplicates of this singleton on spawn
    protected virtual void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
    }

    // Make sure to reset the instance reference when the gameObject gets destroyed
    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}