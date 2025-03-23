using UnityEngine;

public class PersistentBounds : MonoBehaviour
{
    private static PersistentBounds instance;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Keep Cinemachine across scenes
        }
    }
}
