using UnityEngine;

/// <summary>
/// Singleton class utilizing the "lazy initialization" pattern to ensure that only one instance of a MonoBehaviour-derived component of type T exists in the scene hierarchy.
/// </summary>
/// <typeparam name="T">The type of MonoBehaviour-derived component to create a singleton instance of.</typeparam>
public class Singleton<T> : MonoBehaviour where T : Component
{
    protected static T _instance;

    // Property to access the singleton instance
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<T>();
            }

            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}